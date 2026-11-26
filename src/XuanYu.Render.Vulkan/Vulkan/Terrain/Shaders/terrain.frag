#version 450
layout(location = 0) in float elevation;
layout(location = 0) out vec4 color;
void main() {
    float h = clamp(elevation / 4000.0, 0.0, 1.0);
    color = vec4(mix(vec3(0.09, 0.32, 0.10), vec3(0.70, 0.70, 0.63), h), 1.0);
}
