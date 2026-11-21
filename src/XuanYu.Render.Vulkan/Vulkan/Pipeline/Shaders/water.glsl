layout(set = 1, binding = 0) uniform isampler2D swbAtlas;
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
void swbWeights(float t, out vec4 w, out vec4 dw) {
    float u = 1.0 - t, t2 = t * t, u2 = u * u;
    float s = t2 * (3.0 - 2.0 * t), ds = 6.0 * t * u;
    vec4 cubic = vec4(u2*u, 4.0 - 6.0*t2 + 3.0*t2*t,
                      4.0 - 6.0*u2 + 3.0*u2*u, t2*t) / 6.0;
    vec4 deriv = vec4(-3.0*u2, -12.0*t + 9.0*t2,
                       12.0*u - 9.0*u2, 3.0*t2) / 6.0;
    w = mix(vec4(0.0, 1.0-s, s, 0.0), cubic, 0.5);
    dw = mix(vec4(0.0, -ds, ds, 0.0), deriv, 0.5);
}
vec3 swbField(vec4 center, ivec2 cell, vec2 t, ivec2 tile,
              ivec2 extent, int offset, int side) {
    vec4 wx, wy, gx, gy;
    swbWeights(t.x, wx, gx);
    swbWeights(t.y, wy, gy);
    vec3 field = vec3(0.0);
    for (int j = 0; j < 4; ++j) {
        ivec2 p = cell + ivec2(-1, j - 1);
        vec4 values;
        values.x = swbNeighbor(p, tile, extent, offset, side);
        values.w = swbNeighbor(p + ivec2(3, 0), tile, extent, offset, side);
        if (j == 1) values.yz = center.xy;
        else if (j == 2) values.yz = center.zw;
        else values.yz = vec2(swbNeighbor(p + ivec2(1, 0), tile, extent, offset, side),
                              swbNeighbor(p + ivec2(2, 0), tile, extent, offset, side));
        float rowValue = dot(values, wx);
        field += vec3(rowValue * wy[j], dot(values, gx) * wy[j], rowValue * gy[j]);
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
    // Bound ink to mixed cells; fade subpixel shoreline detail in distant views.
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
    vec4 center = vec4(swbSample(cell, offset, side),
                       swbSample(cell + ivec2(1,0), offset, side),
                       swbSample(cell + ivec2(0,1), offset, side),
                       swbSample(cell + ivec2(1,1), offset, side));
    if (all(equal(center, vec4(center.x)))) return vec2(center.x, 0.0);
    return swbStyle(swbField(center, cell, grid - vec2(cell), tile, extent, offset, side), dx, dy);
}