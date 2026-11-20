#version 450
layout(push_constant) uniform PushConstants {
    mat4 mvp;
    vec4 tint;
    vec4 contour;
} pc;
layout(location = 0) in vec3 position;
layout(location = 1) in vec4 color;
layout(location = 0) out vec4 shadedColor;
layout(location = 1) out float elevation;
layout(location = 2) flat out vec4 contourStyle;

void main() {
    gl_Position = pc.mvp * vec4(position, 1.0);
    bool terrain = pc.contour.z > 0.0;
    shadedColor = vec4(mix(color.rgb, pc.tint.rgb, pc.tint.a), terrain ? 1.0 : color.a);
    elevation = terrain ? color.a : 0.0;
    contourStyle = terrain
        ? vec4(pc.contour.xy, pc.contour.w, clamp(pc.contour.z - 1.0, 0.0, 1.0))
        : vec4(0.0);
}
