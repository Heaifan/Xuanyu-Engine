#version 450
#extension GL_GOOGLE_include_directive : require
#include "water.glsl"
layout(set = 0, binding = 0) uniform isampler2D rawElevation;
layout(location = 0) in vec4 shadedColor;
layout(location = 1) in float elevation;
layout(location = 2) flat in vec4 contourStyle;
layout(location = 3) in vec2 rawUv;
layout(location = 4) flat in int rawEnabled;
layout(location = 0) out vec4 outColor;

const float MinorWidthPx = 1.2;
const float MajorWidthPx = 2.0;
const float EdgeFeatherPx = 0.55;
const vec3 ContourColor = vec3(0.12, 0.16, 0.14);

bool sampleRaw(vec2 uv, out float h, out float heightSlope) {
    vec2 grid = uv * 3600.0;
    vec2 dx = dFdx(grid);
    vec2 dy = dFdy(grid);
    h = 0.0;
    heightSlope = 0.0;
    // Allow only sub-sample coordinate rounding, not extended edge texels.
    if (any(lessThan(grid, vec2(-0.05))) ||
        any(greaterThan(grid, vec2(3600.05)))) return false;
    vec2 xy = clamp(grid, 0.0, 3600.0);
    ivec2 cell = min(ivec2(floor(xy)), ivec2(3599));
    vec2 t = xy - vec2(cell);
    int a = texelFetch(rawElevation, cell, 0).r;
    int b = texelFetch(rawElevation, cell + ivec2(1, 0), 0).r;
    int c = texelFetch(rawElevation, cell + ivec2(0, 1), 0).r;
    int d = texelFetch(rawElevation, cell + ivec2(1, 1), 0).r;
    // Do not reconstruct a contour across an incomplete DEM cell.
    if (a == -32768 || b == -32768 || c == -32768 || d == -32768)
        return false;
    float fa = float(a), fb = float(b), fc = float(c), fd = float(d);
    h = mix(mix(fa, fb, t.x), mix(fc, fd, t.x), t.y);
    vec2 gradient = vec2(mix(fb - fa, fd - fc, t.y),
                         mix(fc - fa, fd - fb, t.x));
    // Exact local bilinear gradient; a constant cell has zero slope.
    heightSlope = abs(dot(gradient, dx)) + abs(dot(gradient, dy));
    return true;
}

float coverage(float level, float levelsPerPixel, float widthPixels) {
    float distancePixels = abs(fract(level + 0.5) - 0.5)
        / max(levelsPerPixel, 1e-8);
    float halfWidth = widthPixels * 0.5;
    float ink = 1.0 - smoothstep(halfWidth - EdgeFeatherPx,
        halfWidth + EdgeFeatherPx, distancePixels);
    return ink * (1.0 - smoothstep(0.20, 0.45, levelsPerPixel));
}

float contourInk(float level, float slope) {
    if (slope < 1e-7) return 0.0;
    float minor = coverage(level, slope, MinorWidthPx);
    float major = coverage(level / 5.0, slope / 5.0, MajorWidthPx);
    return max(minor, min(major * 1.35, 1.0));
}

float legacyInk(float heightMeters, float intervalMeters) {
    float level = heightMeters / intervalMeters;
    return contourInk(level, fwidth(level));
}

void shadeContours() {
    if (contourStyle.x <= 0.0 || contourStyle.y <= 0.0) return;
    bool next = contourStyle.z > 0.0 && contourStyle.w > 0.0;
    float ink;
    if (rawEnabled != 0) {
        float h, slope;
        if (!sampleRaw(rawUv, h, slope)) return;
        ink = contourInk(h / contourStyle.x, slope / contourStyle.x);
        if (next) ink = mix(ink,
            contourInk(h / contourStyle.z, slope / contourStyle.z),
            clamp(contourStyle.w, 0.0, 1.0));
    } else {
        ink = legacyInk(elevation, contourStyle.x);
        if (next) ink = mix(ink, legacyInk(elevation, contourStyle.z),
            clamp(contourStyle.w, 0.0, 1.0));
    }
    ink = clamp(ink * contourStyle.y, 0.0, 1.0);
    outColor.rgb = mix(shadedColor.rgb, ContourColor, ink);
}

void main() {
    float water = swbWaterCoverage(waterUv);
    outColor = shadedColor;
    shadeContours();
    outColor.rgb = mix(outColor.rgb, WaterColor, water);
}