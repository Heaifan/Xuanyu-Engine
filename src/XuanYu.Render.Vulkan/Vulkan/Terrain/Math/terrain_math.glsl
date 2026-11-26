dvec2 terrainRotate(double sine, double cosine, double delta) {
    double d2 = delta * delta;
    double sd = delta * (1.0lf - d2 / 6.0lf + d2 * d2 / 120.0lf - d2 * d2 * d2 / 5040.0lf);
    double cd = 1.0lf - d2 / 2.0lf + d2 * d2 / 24.0lf - d2 * d2 * d2 / 720.0lf;
    return dvec2(sine * cd + cosine * sd, cosine * cd - sine * sd);
}

dvec3 terrainPosition(ivec2 grid, int height, dvec4 baseSinCos, dvec4 anchorSinCos, dvec4 originEcefAndStep) {
    dvec2 lat = terrainRotate(baseSinCos.x, baseSinCos.y, double(grid.y) * originEcefAndStep.w);
    dvec2 lon = terrainRotate(baseSinCos.z, baseSinCos.w, double(grid.x) * originEcefAndStep.w);
    double radius = 6378137.0lf / sqrt(1.0lf - 0.0066943799901413165lf * lat.x * lat.x);
    dvec3 ecef = dvec3(radius * lat.y * lon.y, radius * lat.y * lon.x,
        radius * (1.0lf - 0.0066943799901413165lf) * lat.x);
    dvec3 delta = ecef - originEcefAndStep.xyz;
    double slat = anchorSinCos.x, clat = anchorSinCos.y;
    double slon = anchorSinCos.z, clon = anchorSinCos.w;
    return dvec3(-slon * delta.x + clon * delta.y,
        -slat * clon * delta.x - slat * slon * delta.y + clat * delta.z,
        clat * clon * delta.x + clat * slon * delta.y + slat * delta.z + double(height));
}
