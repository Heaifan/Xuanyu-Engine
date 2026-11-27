const float MinorWidthPx = 1.2;
const float MajorWidthPx = 2.0;
const float EdgeFeatherPx = 0.55;
const vec3 ContourColor = vec3(0.27, 0.30, 0.24);
#include "../../Pipeline/Shaders/Terrain/contour-ink.glsl"
#include "../../Pipeline/Shaders/Terrain/contour-gap.glsl"

vec3 shadeG07Contours(vec3 rgb) {
    if (p.contours.x <= 0.0 || p.contours.y <= 0.0) return rgb;
    float slope = fwidth(elevation);
    bool gap = false;
    float gapElevation = 0.0;
    for (int i = 0; i < int(p.overlayFlags.y); ++i) {
        if (!contourLabelGap(p.labelGaps[i])) continue;
        gap = true;
        gapElevation = p.labelGaps[i].w;
        break;
    }
    float ink = labeledInk(elevation, slope, p.contours.x, gap, gapElevation);
    if (p.contours.z > 0.0 && p.contours.w > 0.0)
        ink = mix(ink, labeledInk(elevation, slope, p.contours.z, gap, gapElevation), p.contours.w);
    return mix(rgb, ContourColor, clamp(ink * p.contours.y, 0.0, 1.0));
}

// 每轴独立计算像素间距；细网格隐藏时主网格仍可保留。
vec2 gridInk(vec2 coord, vec2 slope, float widthPx) {
    vec2 distancePx = abs(coord - round(coord)) / slope;
    return 1.0 - smoothstep(vec2(max(0.0, widthPx * 0.5 - 0.5)),
        vec2(widthPx * 0.5 + 0.5), distancePx);
}

float gridLayer(vec2 coord, vec2 slope) {
    vec2 pitch = 1.0 / slope;
    vec2 minor = gridInk(coord, slope, 0.9) * 0.28 *
        smoothstep(vec2(8.0), vec2(20.0), pitch);
    vec2 major = gridInk(coord / 5.0, slope / 5.0, 1.2) * 0.55 *
        smoothstep(vec2(8.0), vec2(20.0), pitch * 5.0);
    vec2 ink = max(minor, major);
    return max(ink.x, ink.y);
}

vec3 shadeG07Grid(vec3 rgb) {
    if (p.grid.w == 0.0) return rgb;
    bool geographic = p.grid.w == 1.0;
    vec2 slope = max(fwidth(mapGrid), vec2(1e-8));
    float edge = max(abs(mapGrid.x), abs(mapGrid.y)) / 32.0;
    float fade = geographic ? 1.0 : 1.0 - smoothstep(0.65, 1.0, edge);
    float ink = gridLayer(mapGrid + p.overlayFlags.zw, slope) * fade;
    float ratio = p.gridNext.z, blend = p.gridNext.w;
    if (ratio > 0.0 && blend > 0.0) {
        float nextInk = gridLayer(mapGrid * ratio + p.gridNext.xy, max(slope * ratio, vec2(1e-8)));
        float nextFade = geographic ? 1.0 : 1.0 - smoothstep(0.65, 1.0, edge * ratio);
        ink = mix(ink, nextInk * nextFade, blend);
    }
    vec3 tint = geographic ? vec3(0.20, 0.71, 0.77) : vec3(0.52);
    return mix(rgb, tint, ink);
}

vec3 shadeG07Selection(vec3 rgb) {
    if (p.overlayFlags.x == 0.0) return rgb;
    vec2 slope = max(fwidth(gridCoord), vec2(1e-8));
    vec2 inner = min(gridCoord - p.selection.xy, p.selection.zw - gridCoord);
    if (any(lessThan(inner, vec2(0.0)))) return rgb;
    float distancePx = min(inner.x / slope.x, inner.y / slope.y);
    float outline = 1.0 - smoothstep(1.0, 2.0, distancePx);
    return mix(rgb, vec3(1.0, 0.722, 0.188), outline);
}