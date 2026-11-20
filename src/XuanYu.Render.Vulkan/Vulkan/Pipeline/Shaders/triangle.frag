#version 450
layout(location = 0) in vec4 shadedColor;
layout(location = 1) in float elevation;
layout(location = 2) flat in vec4 contourStyle;
layout(location = 0) out vec4 outColor;

float coverage(float level, float levelsPerPixel, float widthPixels) {
    float distancePixels = abs(fract(level + 0.5) - 0.5) / max(levelsPerPixel, 1e-8);
    float halfWidth = widthPixels * 0.5;
    float ink = 1.0 - smoothstep(halfWidth - 0.5, halfWidth + 0.5, distancePixels);
    return ink * (1.0 - smoothstep(0.20, 0.45, levelsPerPixel));
}

void main() {
    outColor = shadedColor;
    if (contourStyle.x <= 0.0 || contourStyle.y <= 0.0) return;
    float level = elevation / contourStyle.x;
    float slope = fwidth(level);
    if (slope < 1e-7) return;
    float minor = coverage(level, slope, 1.0);
    float major = coverage(level / 5.0, slope / 5.0, 1.5);
    float ink = max(minor, major * 1.8);
    if (contourStyle.z > 0.0 && contourStyle.w > 0.0) {
        float nextLevel = elevation / contourStyle.z;
        float nextSlope = fwidth(nextLevel);
        float nextMinor = coverage(nextLevel, nextSlope, 1.0);
        float nextMajor = coverage(nextLevel / 5.0, nextSlope / 5.0, 1.5);
        float nextInk = max(nextMinor, nextMajor * 1.8);
        ink = mix(ink, nextInk, contourStyle.w);
    }
    ink *= contourStyle.y;
    outColor.rgb = mix(shadedColor.rgb, vec3(0.12, 0.16, 0.14), ink);
}
