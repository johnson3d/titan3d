#include "TerrainSimplifier.h"
#include "meshoptimizer.h"
#include <algorithm>
#include <array>
#include <cmath>
#include <cstdint>
#include <limits>
#include <map>
#include <set>

NS_BEGIN

namespace
{
    struct FGridPoint
    {
        int64_t X;
        int64_t Z;
    };

    struct FTopology
    {
        std::map<uint64_t, UINT> Directed;
        std::set<uint64_t> Boundary;
        int64_t Area = 0;
        int64_t Euler = 0;
        bool Valid = true;
    };

    uint64_t EdgeKey(UINT a, UINT b)
    {
        return (uint64_t(a) << 32) | b;
    }

    // 坐标先验证为有限整数且有界，叉积在 int64 的安全范围内。
    FTopology Inspect(const std::vector<UINT>& indices, const std::vector<FGridPoint>& grid,
        std::vector<unsigned char>& badVertices)
    {
        FTopology result;
        std::set<UINT> vertices;
        for (size_t t = 0; t < indices.size(); t += 3)
        {
            UINT a = indices[t], b = indices[t + 1], c = indices[t + 2];
            const auto& p = grid[a];
            const auto& q = grid[b];
            const auto& r = grid[c];
            int64_t area = (q.X - p.X) * (r.Z - p.Z) - (r.X - p.X) * (q.Z - p.Z);
            result.Area += area;
            if (area <= 0)
            {
                result.Valid = false;
                badVertices[a] = badVertices[b] = badVertices[c] = 1;
            }
            vertices.insert(a);
            vertices.insert(b);
            vertices.insert(c);
            ++result.Directed[EdgeKey(a, b)];
            ++result.Directed[EdgeKey(b, c)];
            ++result.Directed[EdgeKey(c, a)];
        }

        int64_t edges = 0;
        for (const auto& edge : result.Directed)
        {
            UINT a = UINT(edge.first >> 32), b = UINT(edge.first);
            auto reverse = result.Directed.find(EdgeKey(b, a));
            if (a == b || edge.second != 1)
            {
                result.Valid = false;
                badVertices[a] = badVertices[b] = 1;
            }
            if (reverse == result.Directed.end())
                result.Boundary.insert(edge.first);
            if (reverse == result.Directed.end() || a <= b)
                ++edges;
        }
        result.Euler = int64_t(vertices.size()) - edges + int64_t(indices.size() / 3);
        return result;
    }

    // 闭 1-ring 来自未简化的输入拓扑；只锁坏面自身的残留顶点不能阻止邻点塌到这些顶点。
    UINT LockNeighborhood(const std::vector<UINT>& original, const std::vector<unsigned char>& bad,
        std::vector<unsigned char>& locks)
    {
        std::vector<unsigned char> neighborhood = bad;
        for (size_t t = 0; t < original.size(); t += 3)
        {
            UINT a = original[t], b = original[t + 1], c = original[t + 2];
            if (bad[a] || bad[b] || bad[c])
                neighborhood[a] = neighborhood[b] = neighborhood[c] = 1;
        }
        UINT added = 0;
        for (size_t v = 0; v < locks.size(); ++v)
        {
            if (neighborhood[v] && !locks[v])
            {
                locks[v] = meshopt_SimplifyVertex_Lock;
                ++added;
            }
        }
        return added;
    }

    void Commit(QuarkCluster& cluster, const std::vector<UINT>& candidate)
    {
        const UINT stride = cluster.GetVertSize();
        std::vector<unsigned char> used(cluster.NumVerts, 0);
        for (UINT v : candidate)
            used[v] = 1;
        std::vector<UINT> remap(cluster.NumVerts, ~0u);
        std::vector<float> verts;
        UINT count = 0;
        for (UINT v = 0; v < cluster.NumVerts; ++v)
        {
            if (!used[v])
                continue;
            remap[v] = count++;
            // 始终从原始属性池拷贝，临时缩放位置从不写回。
            verts.insert(verts.end(), cluster.Verts.begin() + v * stride,
                cluster.Verts.begin() + (v + 1) * stride);
        }
        cluster.Indexes.resize(candidate.size());
        for (size_t i = 0; i < candidate.size(); ++i)
            cluster.Indexes[i] = remap[candidate[i]];
        cluster.Verts.swap(verts);
        cluster.NumVerts = count;
        cluster.NumTris = UINT(candidate.size() / 3);
        cluster.MaterialIndexes.assign(cluster.NumTris, 0);
        cluster.FindExternalEdges(cluster.BuildAdjacency());
        cluster.Bound();
    }
}

float SimplifyTerrainCluster(QuarkCluster& cluster, UINT targetTris, float targetError,
    const FTerrainHeightGrid& heightGrid, FTerrainSimplifyStats& stats)
{
    ++stats.Groups;
    if (cluster.NumTris <= targetTris)
        return 0.0f;

    auto preserve = [&]()
    {
        ++stats.PreservedGroups;
        ++stats.MissedTargets;
        return 0.0f; // 几何未改变；DAG 仍按 max 继承子误差。
    };
    auto unsupported = [&]()
    {
        ++stats.UnsupportedGroups;
        return preserve();
    };
    if (cluster.NumVerts == 0 || cluster.NumTris > 1000000 ||
        cluster.Indexes.size() != size_t(cluster.NumTris) * 3 ||
        cluster.Verts.size() != size_t(cluster.NumVerts) * cluster.GetVertSize())
        return unsupported();
    for (INT32 material : cluster.MaterialIndexes)
    {
        if (material != 0)
            return unsupported();
    }
    std::set<std::pair<int64_t, int64_t>> uniqueGrid;
    std::vector<FGridPoint> grid(cluster.NumVerts);
    std::vector<float> positions(cluster.NumVerts * 3);
    float minY = std::numeric_limits<float>::max();
    for (UINT v = 0; v < cluster.NumVerts; ++v)
    {
        const auto& p = cluster.GetPositionConst(v);
        if (!std::isfinite(p.X) || !std::isfinite(p.Y) || !std::isfinite(p.Z) ||
            std::fabs(p.X) > 1000000.0f || std::fabs(p.Z) > 1000000.0f ||
            std::floor(p.X) != p.X || std::floor(p.Z) != p.Z)
            return unsupported();
        grid[v] = { int64_t(p.X), int64_t(p.Z) };
        if (!uniqueGrid.insert({grid[v].X, grid[v].Z}).second)
            return unsupported();
        for (UINT attribute = 3; attribute < cluster.GetVertSize(); ++attribute)
        {
            if (cluster.Verts[v * cluster.GetVertSize() + attribute] != cluster.Verts[attribute])
                return unsupported();
        }
        minY = std::min(minY, p.Y);
    }
    for (UINT v : cluster.Indexes)
    {
        if (v >= cluster.NumVerts)
            return unsupported();
    }
    std::vector<unsigned char> bad(cluster.NumVerts, 0);
    const FTopology original = Inspect(cluster.Indexes, grid, bad);
    if (!original.Valid)
        return unsupported();

    // 用最大边坡度选择固定的 2 的幂缩放，让临时几何接近单值平面；后验检查才是保证。
    double maxSlope = 0.0;
    for (const auto& edge : original.Directed)
    {
        const auto& a = cluster.GetPositionConst(UINT(edge.first >> 32));
        const auto& b = cluster.GetPositionConst(UINT(edge.first));
        double dx = double(b.X) - a.X, dz = double(b.Z) - a.Z;
        double length = std::sqrt(dx * dx + dz * dz);
        if (length > 0.0)
            maxSlope = std::max(maxSlope, std::fabs(double(b.Y) - a.Y) / length);
    }
    float heightScale = 1.0f;
    while (maxSlope * heightScale > 0.25 && heightScale > 1.0f / 256.0f)
        heightScale *= 0.5f;
    for (UINT v = 0; v < cluster.NumVerts; ++v)
    {
        const auto& p = cluster.GetPositionConst(v);
        positions[v * 3] = p.X;
        positions[v * 3 + 1] = float((double(p.Y) - minY) * heightScale);
        positions[v * 3 + 2] = p.Z;
    }

    float originalScale = meshopt_simplifyScale(cluster.Verts.data(), cluster.NumVerts,
        cluster.GetVertSize() * sizeof(float));
    double absoluteTarget = double(originalScale) * (targetError > 0.0f ? targetError : 0.05f) * heightScale;
    float errorTarget = float(std::min(absoluteTarget, double(std::numeric_limits<float>::max())));
    std::vector<unsigned char> locks(cluster.NumVerts, 0);
    std::vector<UINT> candidate(cluster.Indexes.size());
    // 固定上限 + 每轮至少新增 lock；不从上一轮失败几何继续简化。
    for (UINT attempt = 0; attempt < 8; ++attempt)
    {
        ++stats.Attempts;
        float error = 0.0f;
        candidate.resize(cluster.Indexes.size());
        size_t count = meshopt_simplifyWithAttributes(candidate.data(), cluster.Indexes.data(),
            cluster.Indexes.size(), positions.data(), cluster.NumVerts, 3 * sizeof(float),
            nullptr, 0, nullptr, 0, locks.data(), size_t(targetTris) * 3, errorTarget,
            meshopt_SimplifyLockBorder | meshopt_SimplifyErrorAbsolute, &error);
        if (count == 0 || count >= cluster.Indexes.size() || count % 3 != 0)
            return preserve();
        candidate.resize(count);
        for (UINT v : candidate)
        {
            if (v >= cluster.NumVerts)
                return preserve();
        }
        std::fill(bad.begin(), bad.end(), 0);
        FTopology output = Inspect(candidate, grid, bad);
        if (output.Boundary != original.Boundary)
        {
            output.Valid = false;
            for (uint64_t edge : output.Boundary)
            {
                if (!original.Boundary.count(edge))
                    bad[UINT(edge >> 32)] = bad[UINT(edge)] = 1;
            }
            for (uint64_t edge : original.Boundary)
            {
                if (!output.Boundary.count(edge))
                    bad[UINT(edge >> 32)] = bad[UINT(edge)] = 1;
            }
        }
        if (output.Area != original.Area || output.Euler != original.Euler)
        {
            output.Valid = false;
            // 无法仅用坏边定位时，锁住新增面的原始邻域；绝不接受全局判据失败的候选。
            std::set<std::array<UINT, 3>> originalFaces;
            for (size_t t = 0; t < cluster.Indexes.size(); t += 3)
            {
                std::array<UINT, 3> face = {cluster.Indexes[t], cluster.Indexes[t + 1], cluster.Indexes[t + 2]};
                std::sort(face.begin(), face.end());
                originalFaces.insert(face);
            }
            for (size_t t = 0; t < candidate.size(); t += 3)
            {
                std::array<UINT, 3> face = {candidate[t], candidate[t + 1], candidate[t + 2]};
                std::sort(face.begin(), face.end());
                if (!originalFaces.count(face))
                    bad[face[0]] = bad[face[1]] = bad[face[2]] = 1;
            }
        }
        if (output.Valid)
        {
            if (count > size_t(targetTris) * 3)
                ++stats.MissedTargets;
            Commit(cluster, candidate);
            // 缩放 QEM 只用于驱动简化；记录的误差是相对全分辨率高度场的精确竖直 L∞ 上界。
            return TerrainClusterVerticalError(heightGrid, cluster);
        }
        ++stats.RejectedCandidates;
        if (attempt == 0)
            ++stats.RetriedGroups;
        UINT added = LockNeighborhood(cluster.Indexes, bad, locks);
        stats.LockedVertices += added;
        if (added == 0)
            return preserve();
    }
    return preserve();
}

NS_END
