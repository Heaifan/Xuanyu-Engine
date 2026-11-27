#version 450
#extension GL_ARB_gpu_shader_fp64 : require
#extension GL_GOOGLE_include_directive : require
#include "../Math/terrain_params.glsl"
#define SWB_LAYOUT set = 0, binding = 2
#include "../../Pipeline/Shaders/water.glsl"
layout(location = 0) in float elevation;
layout(location = 1) centroid in vec2 gridCoord;
layout(location = 8) flat in vec4 clipCoverage[4];
layout(location = 6) in vec3 slopeNormal;
layout(location = 7) centroid in vec2 mapGrid;
layout(location = 0) out vec4 color;

vec3 elevationColor(float h) {
    if (h <= p.palette[0].w) return p.palette[0].rgb;
    for (int i = 1; i < 7; ++i) {
        if (h > p.palette[i].w) continue;
        float t = (h - p.palette[i - 1].w) / (p.palette[i].w - p.palette[i - 1].w);
        return mix(p.palette[i - 1].rgb, p.palette[i].rgb, t);
    }
    return p.palette[6].rgb;
}

#include "../Math/terrain_overlays.glsl"

void main() {
    vec2 water = p.visual.w >= 0.0 ? swbWaterStyle(waterUv) : vec2(0.0);
    bool covered = false;
    for (int i = 0; i < 4; ++i) {
        vec4 r = clipCoverage[i];
        covered = covered || (all(greaterThanEqual(gridCoord, r.xy)) &&
            all(lessThanEqual(gridCoord, r.zw)));
    }
    if (!covered) discard;
    if (p.visual.w < 0.0) {
        float h = clamp(elevation / 4000.0, 0.0, 1.0);
        color = vec4(mix(vec3(0.09, 0.32, 0.10), vec3(0.70, 0.70, 0.63), h), 1.0);
        return;
    }
    vec3 rgb = elevationColor(elevation);
    if (p.visual.w > 0.0) {
        const vec3 sun = vec3(-0.5, 0.5, 0.70710678);
        rgb *= 0.75 + 0.25 * max(0.0, dot(normalize(slopeNormal), sun));
    }
    rgb = shadeG07Contours(rgb);
    rgb = mix(rgb, WaterColor, water.x);
    rgb = mix(rgb, ShoreColor, water.y);
    rgb = shadeG07Grid(rgb);
    rgb = shadeG07Selection(rgb);
    color = vec4(rgb, 1.0);
}