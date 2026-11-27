layout(std140, set = 0, binding = 1) uniform TerrainParams {
    mat4 mvp;
    dvec4 baseSinCos;
    dvec4 anchorSinCos;
    dvec4 originEcefAndStep;
    vec4 clipRects[4];
    vec4 visual;
    vec4 palette[7];
    vec4 contours;
    vec4 grid;
    vec4 gridPhase;
    vec4 selection;
    vec4 overlayFlags;
    vec4 labelGaps[8];
    vec4 gridNext;
} p;