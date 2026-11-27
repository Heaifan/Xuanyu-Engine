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
    vec2 uv = vec2(texelFetch(heightPage, grid + ivec2(0, 33), 0).r,
        texelFetch(heightPage, grid + ivec2(0, 66), 0).r) / 32767.0;
    return normalize(vec3(uv, max(0.0, 1.0 - abs(uv.x) - abs(uv.y))));
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