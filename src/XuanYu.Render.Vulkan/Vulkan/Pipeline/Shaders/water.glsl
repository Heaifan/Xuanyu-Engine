layout(set = 1, binding = 0) uniform isampler2D swbAtlas;
layout(location = 5) in vec2 waterUv;

const int SwbSide = 3601;
const int SwbWordsPerTile = (SwbSide * SwbSide + 15) / 16;
const vec3 WaterColor = vec3(0.32, 0.61, 0.84);

int swbWord(int index, int side) {
    return texelFetch(swbAtlas, ivec2(index % side, index / side), 0).r;
}

bool isSwbWater(vec2 uv) {
    int side = textureSize(swbAtlas, 0).x;
    if (side < 2) return false;
    ivec2 extent = ivec2(swbWord(0, side), swbWord(1, side));
    if (any(lessThan(uv, vec2(0.0))) || any(greaterThan(uv, vec2(extent))))
        return false;
    ivec2 tile = min(ivec2(floor(uv)), extent - 1);
    int slot = swbWord(2 + tile.y * extent.x + tile.x, side);
    if (slot == 0) return false;
    ivec2 sampleAt = ivec2(round(clamp(uv - vec2(tile), 0.0, 1.0) * 3600.0));
    int sampleIndex = sampleAt.y * SwbSide + sampleAt.x;
    int wordIndex = 2 + extent.x * extent.y + (slot - 1) * SwbWordsPerTile + sampleIndex / 16;
    uint bits = uint(swbWord(wordIndex, side));
    return (bits & (1u << uint(sampleIndex % 16))) != 0u;
}