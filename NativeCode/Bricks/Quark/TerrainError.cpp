#include "TerrainError.h"
#include <algorithm>
#include <cfloat>
#include <cmath>
#include <limits>

NS_BEGIN

namespace
{
    int64_t FloorDiv(int64_t a, int64_t b)
    {
        int64_t q = a / b;
        if (a % b != 0 && ((a < 0) != (b < 0)))
            --q;
        return q;
    }

    int64_t CeilDiv(int64_t a, int64_t b)
    {
        return -FloorDiv(-a, b);
    }

    // 双精度求值（权重非负的凸组合）的相对舍入远小于 2^-48，padding 取 2^-40 * max|h| 留足余量。
    float RoundUpError(double maxError, double maxAbsHeight)
    {
        if (!std::isfinite(maxError) || !std::isfinite(maxAbsHeight))
            return FLT_MAX;
        double padded = maxError + std::ldexp(maxAbsHeight, -40);
        if (padded == 0.0)
            return 0.0f;
        padded = std::nextafter(padded, std::numeric_limits<double>::infinity());
        if (!(padded < double(FLT_MAX)))
            return FLT_MAX;
        float result = float(padded);
        if (double(result) < padded)
            result = std::nextafter(result, FLT_MAX);
        return result;
    }
}

double TerrainTriangleVerticalError(const FTerrainHeightGrid& grid,
    const int64_t x[3], const int64_t z[3], const double y[3])
{
    const double invalid = std::numeric_limits<double>::infinity();
    const int64_t n = grid.Size;
    if (grid.Heights == nullptr || n < 2)
        return invalid;
    for (int k = 0; k < 3; ++k)
    {
        if (x[k] < 0 || z[k] < 0 || x[k] >= n || z[k] >= n || !std::isfinite(y[k]))
            return invalid;
    }
    const int64_t area2 = (x[1] - x[0]) * (z[2] - z[0]) - (x[2] - x[0]) * (z[1] - z[0]);
    if (area2 <= 0)
        return invalid;
    auto H = [&](int64_t i, int64_t j)
    {
        return double(grid.Heights[j * n + i]);
    };

    double maxError = 0.0;
    auto track = [&](double p, double f)
    {
        maxError = std::max(maxError, std::fabs(p - f));
    };

    // 闭三角形内的格点：e_k 为有向边 v_k→v_{k+1} 的边函数（逆时针时内部 >= 0），
    // 在 v_{k+2} 处等于 2A，因此 P = (e0*y2 + e1*y0 + e2*y1) / 2A。逐行用整数除法求 x 区间。
    const int64_t minX = std::min({x[0], x[1], x[2]}), maxX = std::max({x[0], x[1], x[2]});
    const int64_t minZ = std::min({z[0], z[1], z[2]}), maxZ = std::max({z[0], z[1], z[2]});
    for (int64_t pz = minZ; pz <= maxZ; ++pz)
    {
        int64_t lo = minX, hi = maxX;
        for (int k = 0; k < 3 && lo <= hi; ++k)
        {
            const int p = k, q = (k + 1) % 3;
            const int64_t dz = z[q] - z[p];
            const int64_t c0 = (x[q] - x[p]) * (pz - z[p]);
            // e = c0 - dz * (px - x[p]) >= 0
            if (dz == 0)
            {
                if (c0 < 0)
                    hi = lo - 1;
            }
            else if (dz > 0)
                hi = std::min(hi, x[p] + FloorDiv(c0, dz));
            else
                lo = std::max(lo, x[p] + CeilDiv(-c0, -dz));
        }
        for (int64_t px = lo; px <= hi; ++px)
        {
            int64_t e[3];
            for (int k = 0; k < 3; ++k)
            {
                const int p = k, q = (k + 1) % 3;
                e[k] = (x[q] - x[p]) * (pz - z[p]) - (z[q] - z[p]) * (px - x[p]);
            }
            const double p = (double(e[0]) * y[2] + double(e[1]) * y[0] + double(e[2]) * y[1]) / double(area2);
            track(p, H(px, pz));
        }
    }

    // 边与网格线的非格点交点：P 沿边线性，F 沿网格边在相邻两格点间线性。
    for (int k = 0; k < 3; ++k)
    {
        const int64_t x0 = x[k], z0 = z[k], x1 = x[(k + 1) % 3], z1 = z[(k + 1) % 3];
        const double y0 = y[k], y1 = y[(k + 1) % 3];
        const int64_t dx = x1 - x0, dz = z1 - z0;
        if (dx != 0)
        {
            for (int64_t i = std::min(x0, x1) + 1; i < std::max(x0, x1); ++i)
            {
                const int64_t num = (i - x0) * dz;
                const int64_t q = FloorDiv(num, dx);
                const int64_t rem = num - q * dx;
                if (rem == 0)
                    continue;
                const int64_t j = z0 + q;
                const double f = double(rem) / double(dx);
                const double t = double(i - x0) / double(dx);
                track(y0 * (1.0 - t) + y1 * t, H(i, j) * (1.0 - f) + H(i, j + 1) * f);
            }
        }
        if (dz != 0)
        {
            for (int64_t j = std::min(z0, z1) + 1; j < std::max(z0, z1); ++j)
            {
                const int64_t num = (j - z0) * dx;
                const int64_t q = FloorDiv(num, dz);
                const int64_t rem = num - q * dz;
                if (rem == 0)
                    continue;
                const int64_t i = x0 + q;
                const double f = double(rem) / double(dz);
                const double t = double(j - z0) / double(dz);
                track(y0 * (1.0 - t) + y1 * t, H(i, j) * (1.0 - f) + H(i + 1, j) * f);
            }
        }
        const int64_t u0 = x0 - z0, u1 = x1 - z1, du = u1 - u0;
        if (du != 0)
        {
            for (int64_t c = std::min(u0, u1) + 1; c < std::max(u0, u1); ++c)
            {
                const int64_t num = (c - u0) * dx;
                const int64_t q = FloorDiv(num, du);
                const int64_t rem = num - q * du;
                if (rem == 0)
                    continue;
                const int64_t i = x0 + q;
                const double f = double(rem) / double(du);
                const double t = double(c - u0) / double(du);
                track(y0 * (1.0 - t) + y1 * t, H(i, i - c) * (1.0 - f) + H(i + 1, i + 1 - c) * f);
            }
        }
    }
    return maxError;
}

float TerrainClusterVerticalError(const FTerrainHeightGrid& grid, const QuarkCluster& cluster)
{
    if (cluster.Indexes.size() % 3 != 0)
        return FLT_MAX;
    double maxError = 0.0;
    for (size_t t = 0; t < cluster.Indexes.size(); t += 3)
    {
        int64_t x[3], z[3];
        double y[3];
        for (int k = 0; k < 3; ++k)
        {
            const UINT v = cluster.Indexes[t + k];
            if (v >= cluster.NumVerts)
                return FLT_MAX;
            const auto& p = cluster.GetPositionConst(v);
            if (!std::isfinite(p.X) || !std::isfinite(p.Z) || std::floor(p.X) != p.X || std::floor(p.Z) != p.Z)
                return FLT_MAX;
            x[k] = int64_t(p.X);
            z[k] = int64_t(p.Z);
            y[k] = p.Y;
        }
        maxError = std::max(maxError, TerrainTriangleVerticalError(grid, x, z, y));
        if (!std::isfinite(maxError))
            return FLT_MAX;
    }
    return RoundUpError(maxError, grid.MaxAbsHeight);
}

NS_END
