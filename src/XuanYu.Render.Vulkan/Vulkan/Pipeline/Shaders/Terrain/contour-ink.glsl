float coverage(float level, float levelsPerPixel, float widthPixels) {
    float distancePixels = abs(fract(level + 0.5) - 0.5)
        / max(levelsPerPixel, 1e-8);
    float halfWidth = widthPixels * 0.5;
    float ink = 1.0 - smoothstep(halfWidth - EdgeFeatherPx,
        halfWidth + EdgeFeatherPx, distancePixels);
    return ink * (1.0 - smoothstep(0.20, 0.45, levelsPerPixel));
}

float contourInk(float level, float slope) {
    if (slope < 1e-7) return 0.0;
    float minor = coverage(level, slope, MinorWidthPx);
    float major = coverage(level / 5.0, slope / 5.0, MajorWidthPx);
    return max(minor, min(major * 1.35, 1.0));
}

float labeledInk(float h, float slope, float interval,
                 bool gap, float gapElevation) {
    if (slope / interval < 1e-7) return 0.0;
    float level = h / interval;
    float minor = coverage(level, slope / interval, MinorWidthPx);
    float major = coverage(level / 5.0, slope / (interval * 5.0), MajorWidthPx);
    if (gap && abs(round(level) * interval - gapElevation) < 0.01) minor = 0.0;
    if (gap && abs(round(level / 5.0) * interval * 5.0 - gapElevation) < 0.01)
        major = 0.0;
    return max(minor, min(major * 1.35, 1.0));
}

float legacyInk(float heightMeters, float intervalMeters) {
    float level = heightMeters / intervalMeters;
    return contourInk(level, fwidth(level));
}