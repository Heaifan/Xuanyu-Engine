#version 450
layout(location = 0) in vec4 shadedColor;
layout(location = 1) in float elevation;
layout(location = 2) flat in vec4 contourStyle;
layout(location = 0) out vec4 outColor;

const float MinorWidthPx = 1.2;
const float MajorWidthPx = 2.0;
const float EdgeFeatherPx = 0.55;
const vec3 ContourColor = vec3(0.12, 0.16, 0.14);

float coverage(float level, float levelsPerPixel, float widthPixels) {
    float distancePixels = abs(fract(level + 0.5) - 0.5)
        / max(levelsPerPixel, 1e-8);
    float halfWidth = widthPixels * 0.5;
    float ink = 1.0 - smoothstep(halfWidth - EdgeFeatherPx,
        halfWidth + EdgeFeatherPx, distancePixels);
    return ink * (1.0 - smoothstep(0.20, 0.45, levelsPerPixel));
}

float contourInk(float heightMeters, float intervalMeters) {
    float level = heightMeters / intervalMeters;
    float slope = fwidth(level);
    if (slope < 1e-7) return 0.0;
    float minor = coverage(level, slope, MinorWidthPx);
    float major = coverage(level / 5.0, slope / 5.0, MajorWidthPx);
    return max(minor, min(major * 1.35, 1.0));
}

void main() {
    outColor = shadedColor;
    if (contourStyle.x <= 0.0 || contourStyle.y <= 0.0) return;
    float ink = contourInk(elevation, contourStyle.x);
    if (contourStyle.z > 0.0 && contourStyle.w > 0.0) {
        float nextInk = contourInk(elevation, contourStyle.z);
        ink = mix(ink, nextInk, clamp(contourStyle.w, 0.0, 1.0));
    }
    ink = clamp(ink * contourStyle.y, 0.0, 1.0);
    outColor.rgb = mix(shadedColor.rgb, ContourColor, ink);
}