layout(set = 1, binding = 0) uniform isampler2D swbAtlas;
layout(location = 5) in vec2 waterUv;

const int SwbSide = 3601;
const int SwbCells = SwbSide - 1;
const int SwbWordsPerTile = (SwbSide * SwbSide + 15) / 16;
const vec3 WaterColor = vec3(0.32, 0.61, 0.84);

int swbWord(int index, int side) {
    return texelFetch(swbAtlas, ivec2(index % side, index / side), 0).r;
}

float swbSample(ivec2 sampleAt, int tileOffset, int side) {
    int sampleIndex = sampleAt.y * SwbSide + sampleAt.x;
    uint bits = uint(swbWord(tileOffset + sampleIndex / 16, side));
    return (bits & (1u << uint(sampleIndex % 16))) != 0u ? 1.0 : 0.0;
}

float swbCellCoverage(vec2 grid, vec2 dx, vec2 dy, int offset, int side) {
    // Keep the last cell inside the tile, including its shared edge sample.
    ivec2 cell = min(ivec2(floor(grid)), ivec2(SwbCells - 1));
    vec2 t = grid - vec2(cell);
    float a = swbSample(cell, offset, side);
    float b = swbSample(cell + ivec2(1, 0), offset, side);
    float c = swbSample(cell + ivec2(0, 1), offset, side);
    float d = swbSample(cell + ivec2(1, 1), offset, side);
    if (a == b && a == c && a == d) return a;

    float field = mix(mix(a, b, t.x), mix(c, d, t.x), t.y);
    vec2 gradient = vec2(mix(b - a, d - c, t.y),
                         mix(c - a, d - b, t.x));
    float pixelSlope = abs(dot(gradient, dx)) + abs(dot(gradient, dy));
    float halfWidth = clamp(0.5 * pixelSlope, 1e-5, 0.5);
    return smoothstep(0.5 - halfWidth, 0.5 + halfWidth, field);
}

float swbWaterCoverage(vec2 uv) {
    // Derivatives must be computed before non-uniform tile/coverage branches.
    vec2 dx = dFdx(uv) * float(SwbCells);
    vec2 dy = dFdy(uv) * float(SwbCells);
    int side = textureSize(swbAtlas, 0).x;
    if (side < 2) return 0.0;
    ivec2 extent = ivec2(swbWord(0, side), swbWord(1, side));
    if (any(lessThan(uv, vec2(0.0))) || any(greaterThan(uv, vec2(extent))))
        return 0.0;
    ivec2 tile = min(ivec2(floor(uv)), extent - 1);
    int slot = swbWord(2 + tile.y * extent.x + tile.x, side);
    if (slot == 0) return 0.0;
    int offset = 2 + extent.x * extent.y + (slot - 1) * SwbWordsPerTile;
    vec2 grid = clamp(uv - vec2(tile), 0.0, 1.0) * float(SwbCells);
    return swbCellCoverage(grid, dx, dy, offset, side);
}