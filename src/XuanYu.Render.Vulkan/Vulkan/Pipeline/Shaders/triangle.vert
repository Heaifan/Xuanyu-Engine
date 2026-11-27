#version 450
layout(push_constant) uniform PushConstants {
    mat4 mvp;
    vec4 tint;
    vec4 contour;
    vec4 water;
    vec4 label;
} pc;
layout(location = 0) in vec3 position;
layout(location = 1) in vec4 color;
layout(location = 2) in vec2 demUv;
layout(location = 3) in vec3 baseColor;
layout(location = 0) out vec4 shadedColor;
layout(location = 1) out float elevation;
layout(location = 2) flat out vec4 contourStyle;
layout(location = 3) out vec2 rawUv;
layout(location = 4) flat out int rawEnabled;
layout(location = 5) out vec2 waterUv;
layout(location = 6) out vec3 terrainBaseColor;
layout(location = 7) flat out float shadeSouth;

void main() {
    gl_Position = pc.mvp * vec4(position, 1.0);
    bool terrain = pc.contour.z > 0.0;
    rawEnabled = pc.contour.z >= 3.0 ? 1 : 0;
    float blend = rawEnabled != 0 ? pc.contour.z - 3.0 : pc.contour.z - 1.0;
    vec3 surfaceColor = terrain && pc.label.w > 0.0 ? baseColor : color.rgb;
    shadedColor = vec4(mix(surfaceColor, pc.tint.rgb, pc.tint.a), terrain ? 1.0 : color.a);
    elevation = terrain ? color.a : 0.0;
    contourStyle = terrain
        ? vec4(pc.contour.xy, pc.contour.w, clamp(blend, 0.0, 1.0))
        : vec4(0.0);
    rawUv = demUv - pc.water.xy;
    waterUv = terrain && pc.water.z > 0.0
        ? vec2(demUv.x, 1.0 - demUv.y) : vec2(-1.0);
    terrainBaseColor = baseColor;
    shadeSouth = terrain ? pc.water.w : 0.0;
}