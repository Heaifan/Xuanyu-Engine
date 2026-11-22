layout(location = 6) in vec3 terrainBaseColor;
layout(location = 7) flat in float shadeSouth;

// HGT 边缘以单侧差分取代跨界缺样；内部仍采用中心差分。
bool hillshadeGradient(ivec2 p, out vec2 gradient) {
    ivec2 lo = max(p - ivec2(1), ivec2(0));
    ivec2 hi = min(p + ivec2(1), ivec2(3600));
    int west = texelFetch(rawElevation, ivec2(lo.x, p.y), 0).r;
    int east = texelFetch(rawElevation, ivec2(hi.x, p.y), 0).r;
    int north = texelFetch(rawElevation, ivec2(p.x, lo.y), 0).r;
    int south = texelFetch(rawElevation, ivec2(p.x, hi.y), 0).r;
    gradient = vec2(float(east - west), float(north - south)) / vec2(hi - lo);
    return min(min(west, east), min(north, south)) != -32768;
}

vec3 terrainHillshade(vec3 fallback, vec2 uv) {
    vec2 grid = uv * 3600.0;
    float footprint = max(length(dFdx(grid)), length(dFdy(grid)));
    if (shadeSouth <= 0.0) return fallback;
    if (any(lessThan(grid, vec2(-0.05))) ||
        any(greaterThan(grid, vec2(3600.05)))) return fallback;
    float weight = 1.0 - smoothstep(2.0, 8.0, footprint);
    if (weight <= 0.0) return fallback;
    vec2 xy = clamp(grid, 0.0, 3600.0);
    ivec2 cell = min(ivec2(floor(xy)), ivec2(3599));
    vec2 t = xy - vec2(cell);
    vec2 a, b, c, d;
    if (!hillshadeGradient(cell, a) || !hillshadeGradient(cell + ivec2(1, 0), b) ||
        !hillshadeGradient(cell + ivec2(0, 1), c) ||
        !hillshadeGradient(cell + ivec2(1, 1), d)) return fallback;
    vec2 gradient = mix(mix(a, b, t.x), mix(c, d, t.x), t.y);
    float phi = radians(shadeSouth - 91.0 + 1.0 - xy.y / 3600.0);
    const float e2 = 0.00669437999014;
    float s = sin(phi);
    float w = sqrt(1.0 - e2 * s * s);
    float n = 6378137.0 / w;
    float m = n * (1.0 - e2) / (w * w);
    vec2 spacing = vec2(n * cos(phi), m) * radians(1.0 / 3600.0);
    vec3 normal = normalize(vec3(-gradient / spacing, 1.0));
    const vec3 sun = vec3(-0.5, 0.5, 0.70710678);
    float light = 0.75 + 0.25 * max(0.0, dot(normal, sun));
    return mix(fallback, terrainBaseColor * light, weight);
}