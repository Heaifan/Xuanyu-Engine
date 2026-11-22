#version 450
#extension GL_GOOGLE_include_directive : require
#include "water.glsl"
layout(set = 0, binding = 0) uniform isampler2D rawElevation;
layout(set = 2, binding = 0, std430) readonly buffer ContourGaps {
    vec4 entries[];
} contourGaps;
layout(location = 0) in vec4 shadedColor;
layout(location = 1) in float elevation;
layout(location = 2) flat in vec4 contourStyle;
layout(location = 3) in vec2 rawUv;
layout(location = 4) flat in int rawEnabled;
layout(location = 0) out vec4 outColor;
#include "Terrain/hillshade.glsl"
#include "Terrain/contour-gap.glsl"
layout(push_constant) uniform PushConstants {
    mat4 mvp;
    vec4 tint;
    vec4 contour;
    vec4 water;
    vec4 label;
} pc;

const float MinorWidthPx = 1.2;
const float MajorWidthPx = 2.0;
const float EdgeFeatherPx = 0.55;
const vec3 ContourColor = vec3(0.27, 0.30, 0.24);

bool sampleRaw(vec2 uv, out float h, out float heightSlope) {
    vec2 grid = uv * 3600.0;
    vec2 dx = dFdx(grid);
    vec2 dy = dFdy(grid);
    h = 0.0;
    heightSlope = 0.0;
    if (any(lessThan(grid, vec2(-0.05))) ||
        any(greaterThan(grid, vec2(3600.05)))) return false;
    vec2 xy = clamp(grid, 0.0, 3600.0);
    ivec2 cell = min(ivec2(floor(xy)), ivec2(3599));
    vec2 t = xy - vec2(cell);
    int a = texelFetch(rawElevation, cell, 0).r;
    int b = texelFetch(rawElevation, cell + ivec2(1, 0), 0).r;
    int c = texelFetch(rawElevation, cell + ivec2(0, 1), 0).r;
    int d = texelFetch(rawElevation, cell + ivec2(1, 1), 0).r;
    if (a == -32768 || b == -32768 || c == -32768 || d == -32768) return false;
    float fa = float(a), fb = float(b), fc = float(c), fd = float(d);
    h = mix(mix(fa, fb, t.x), mix(fc, fd, t.x), t.y);
    vec2 gradient = vec2(mix(fb - fa, fd - fc, t.y),
                         mix(fc - fa, fd - fb, t.x));
    heightSlope = abs(dot(gradient, dx)) + abs(dot(gradient, dy));
    return true;
}

#include "Terrain/contour-ink.glsl"

void shadeContours() {
    if (contourStyle.x <= 0.0 || contourStyle.y <= 0.0) return;
    bool next = contourStyle.z > 0.0 && contourStyle.w > 0.0;
    float ink;
    if (rawEnabled != 0) {
        float h, slope;
        if (!sampleRaw(rawUv, h, slope)) return;
        bool gap = false;
        float gapElevation = 0.0;
        for (int i = 0; i < int(pc.label.y); i++) {
            vec4 entry = contourGaps.entries[int(pc.label.x) + i];
            if (!contourLabelGap(entry)) continue;
            gap = true;
            gapElevation = entry.w;
            break;
        }
        ink = labeledInk(h, slope, contourStyle.x, gap, gapElevation);
        if (next) ink = mix(ink,
            labeledInk(h, slope, contourStyle.z, gap, gapElevation),
            clamp(contourStyle.w, 0.0, 1.0));
    } else {
        ink = legacyInk(elevation, contourStyle.x);
        if (next) ink = mix(ink, legacyInk(elevation, contourStyle.z),
            clamp(contourStyle.w, 0.0, 1.0));
    }
    ink = clamp(ink * contourStyle.y, 0.0, 1.0);
    outColor.rgb = mix(outColor.rgb, ContourColor, ink);
}

void main() {
    vec2 water = swbWaterStyle(waterUv);
    outColor = shadedColor;
    outColor.rgb = terrainHillshade(outColor.rgb, rawUv);
    shadeContours();
    outColor.rgb = mix(outColor.rgb, WaterColor, water.x);
    outColor.rgb = mix(outColor.rgb, ShoreColor, water.y);
}