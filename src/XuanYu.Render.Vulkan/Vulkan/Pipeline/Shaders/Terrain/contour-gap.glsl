// label.z packs screen angle and measured half-width; label.w is the DEM level.
bool contourLabelGap(vec4 label) {
    vec2 shape = unpackHalf2x16(floatBitsToUint(label.z));
    if (shape.y <= 0.0) return false;
    vec2 delta = gl_FragCoord.xy - label.xy;
    vec2 tangent = vec2(cos(shape.x), sin(shape.x));
    return abs(dot(delta, tangent)) < shape.y &&
           abs(dot(delta, vec2(-tangent.y, tangent.x))) < 8.0;
}
