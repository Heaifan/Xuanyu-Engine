#version 450
#extension GL_ARB_gpu_shader_fp64 : require
#extension GL_GOOGLE_include_directive : require
#include "../Math/terrain_math.glsl"
layout(set = 0, binding = 0) uniform isampler2D heightPage;
layout(std140, set = 0, binding = 1) uniform TerrainParams {
    mat4 mvp;
    dvec4 baseSinCos;
    dvec4 anchorSinCos;
    dvec4 originEcefAndStep;
} p;
layout(location = 0) out float elevation;
void main() {
    ivec2 grid = ivec2(gl_VertexIndex % 33, gl_VertexIndex / 33);
    int height = texelFetch(heightPage, grid, 0).r;
    dvec3 position = terrainPosition(grid, height, p.baseSinCos, p.anchorSinCos, p.originEcefAndStep);
    gl_Position = p.mvp * vec4(vec3(position), 1.0);
    elevation = float(height);
}
