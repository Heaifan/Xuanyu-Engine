#ifndef SWB_LAYOUT
#define SWB_LAYOUT set = 1, binding = 0
#endif
layout(SWB_LAYOUT) uniform isampler2D swbAtlas;
layout(location = 5) in vec2 waterUv;

const int SwbSide = 3601;
const int SwbCells = SwbSide - 1;
const int SwbWordsPerTile = (SwbSide * SwbSide + 15) / 16;
const vec3 WaterColor = vec3(0.32, 0.61, 0.84);
const vec3 ShoreColor = vec3(50.0, 104.0, 120.0) / 255.0;

int swbWord(int index, int side) {
    return texelFetch(swbAtlas, ivec2(index % side, index / side), 0).r;
}
int swbOffset(ivec2 tile, ivec2 extent, int side) {
    if (any(lessThan(tile, ivec2(0))) || any(greaterThanEqual(tile, extent))) return -1;
    int slot = swbWord(2 + tile.y * extent.x + tile.x, side);
    return slot == 0 ? -1 : 2 + extent.x * extent.y + (slot - 1) * SwbWordsPerTile;
}
float swbSample(ivec2 p, int offset, int side) {
    int index = p.y * SwbSide + p.x;
    uint bits = uint(swbWord(offset + index / 16, side));
    return (bits & (1u << uint(index % 16))) != 0u ? 1.0 : 0.0;
}
float swbNeighbor(ivec2 p, ivec2 tile, ivec2 extent, int offset, int side) {
    ivec2 shift = ivec2(p.x < 0 ? -1 : (p.x > SwbCells ? 1 : 0),
                        p.y < 0 ? -1 : (p.y > SwbCells ? 1 : 0));
    if (all(equal(shift, ivec2(0)))) return swbSample(p, offset, side);
    int next = swbOffset(tile + shift, extent, side);
    // Shared end samples repeat; neighboring grids advance by 3600, not 3601.
    return next >= 0 ? swbSample(p - shift * SwbCells, next, side)
                     : swbSample(clamp(p, ivec2(0), ivec2(SwbCells)), offset, side);
}
void swbWeights(float t, out vec2 w[6]) {
    float u = 1.0 - t, t2 = t*t, u2 = u*u;
    vec4 b = vec4(u2*u, 4.0 - 6.0*t2 + 3.0*t2*t,
                  4.0 - 6.0*u2 + 3.0*u2*u, t2*t) / 6.0;
    vec4 d = vec4(-3.0*u2, -12.0*t + 9.0*t2,
                   12.0*u - 9.0*u2, 3.0*t2) / 6.0;
    // Convolve the cubic basis with [1, 2, 1]/4; x is weight, y is derivative.
    for (int i = 0; i < 6; ++i) w[i] = vec2(0.0);
    for (int i = 0; i < 4; ++i) {
        vec2 v = vec2(b[i], d[i]) * 0.25;
        w[i] += v;
        w[i + 1] += v * 2.0;
        w[i + 2] += v;
    }
}
vec3 swbField(vec4 core[4], ivec2 cell, vec2 t, ivec2 tile,
              ivec2 extent, int offset, int side) {
    vec2 wx[6], wy[6];
    swbWeights(t.x, wx); swbWeights(t.y, wy);
    vec3 field = vec3(0.0);
    for (int j = 0; j < 6; ++j) {
        vec2 rowField = vec2(0.0);
        for (int i = 0; i < 6; ++i) {
            float v;
            if (i > 0 && i < 5 && j > 0 && j < 5) v = core[j - 1][i - 1];
            else v = swbNeighbor(cell + ivec2(i - 2, j - 2), tile, extent, offset, side);
            rowField += v * wx[i];
        }
        field += vec3(rowField * wy[j].x, rowField.x * wy[j].y);
    }
    return field;
}
vec2 swbStyle(vec3 field, vec2 dx, vec2 dy) {
    float slope = length(vec2(dot(field.yz, dx), dot(field.yz, dy)));
    float delta = abs(field.x - 0.5);
    float width = clamp(0.85 * slope, 1e-5, 0.32);
    float water = smoothstep(0.5 - width, 0.5 + width, field.x);
    float distancePx = delta / max(slope, 1e-5);
    float ink = 1.0 - smoothstep(0.15, 1.05, distancePx);
    // Bound ink to the shoreline field; fade subpixel detail in distant views.
    ink *= 1.0 - smoothstep(0.24, 0.32, delta);
    ink *= 1.0 - smoothstep(0.75, 2.0, max(length(dx), length(dy)));
    ink *= smoothstep(1e-6, 1e-5, slope);
    return vec2(water, ink * 0.65);
}
vec2 swbWaterStyle(vec2 uv) {
    // Compute derivatives before divergent tile or mask branches.
    vec2 dx = dFdx(uv) * float(SwbCells), dy = dFdy(uv) * float(SwbCells);
    int side = textureSize(swbAtlas, 0).x;
    if (side < 2) return vec2(0.0);
    ivec2 extent = ivec2(swbWord(0, side), swbWord(1, side));
    if (any(lessThan(uv, vec2(0.0))) || any(greaterThan(uv, vec2(extent)))) return vec2(0.0);
    ivec2 tile = min(ivec2(floor(uv)), extent - 1);
    int offset = swbOffset(tile, extent, side);
    if (offset < 0) return vec2(0.0);
    vec2 grid = clamp(uv - vec2(tile), 0.0, 1.0) * float(SwbCells);
    ivec2 cell = min(ivec2(floor(grid)), ivec2(SwbCells - 1));
    vec4 core[4];
    for (int j = 0; j < 4; ++j)
        for (int i = 0; i < 4; ++i)
            core[j][i] = swbNeighbor(cell + ivec2(i - 1, j - 1), tile, extent, offset, side);
    vec4 first = vec4(core[0].x);
    if (all(equal(core[0], first)) && all(equal(core[1], first)) &&
        all(equal(core[2], first)) && all(equal(core[3], first))) return vec2(first.x, 0.0);
    return swbStyle(swbField(core, cell, grid - vec2(cell), tile, extent, offset, side), dx, dy);
}