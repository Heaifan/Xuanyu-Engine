#version 450
#extension GL_ARB_gpu_shader_fp64 : require
#extension GL_GOOGLE_include_directive : require
#include "../Math/terrain_math.glsl"
#include "../Math/terrain_params.glsl"
layout(set = 0, binding = 0) uniform isampler2D heightPage;
layout(location = 0) out float elevation;
layout(location = 1) centroid out vec2 gridCoord;
layout(location = 8) flat out vec4 clipCoverage[4];
layout(location = 5) out vec2 waterUv;
layout(location = 6) out vec3 slopeNormal;
layout(location = 7) centroid out vec2 mapGrid;

vec3 pageNormal(ivec2 grid) {
    ivec2 lo = max(grid - ivec2(1), ivec2(0));
    ivec2 hi = min(grid + ivec2(1), ivec2(32));
    float dx = float(texelFetch(heightPage, ivec2(hi.x, grid.y), 0).r -
        texelFetch(heightPage, ivec2(lo.x, grid.y), 0).r) / float(hi.x - lo.x);
    float dy = float(texelFetch(heightPage, ivec2(grid.x, hi.y), 0).r -
        texelFetch(heightPage, ivec2(grid.x, lo.y), 0).r) / float(hi.y - lo.y);
    dvec2 lat = terrainRotate(p.baseSinCos.x, p.baseSinCos.y, double(grid.y) * p.originEcefAndStep.w);
    float s = float(lat.x), c = float(lat.y);
    const float e2 = 0.00669437999014;
    float w = sqrt(1.0 - e2 * s * s);
    float n = 6378137.0 / w;
    float m = n * (1.0 - e2) / (w * w);
    vec2 spacing = vec2(max(n * c, 1.0), m) * float(p.originEcefAndStep.w);
    return normalize(vec3(-vec2(dx, dy) / spacing, 1.0));
}

void main() {
    ivec2 grid = ivec2(gl_VertexIndex % 33, gl_VertexIndex / 33);
    int height = texelFetch(heightPage, grid, 0).r;
    dvec3 position = terrainPosition(grid, height, p.baseSinCos, p.anchorSinCos, p.originEcefAndStep);
    gl_Position = p.mvp * vec4(vec3(position), 1.0);
    elevation = float(height);
    gridCoord = vec2(grid);
    mapGrid = vec2(0.0);
    if (p.grid.w == 1.0)
        mapGrid = p.gridPhase.xy + vec2(grid) * p.gridPhase.zw;
    else if (p.grid.w == 2.0) {
        dvec3 ground = terrainPosition(grid, 0, p.baseSinCos, p.anchorSinCos, p.originEcefAndStep);
        mapGrid = vec2(ground.xy / double(p.grid.x) - dvec2(p.gridPhase.xy));
    }
    for (int i = 0; i < 4; ++i) clipCoverage[i] = p.clipRects[i];
    waterUv = p.visual.xy + vec2(grid) * p.visual.z;
    slopeNormal = p.visual.w > 0.0 ? pageNormal(grid) : vec3(0.0, 0.0, 1.0);
}