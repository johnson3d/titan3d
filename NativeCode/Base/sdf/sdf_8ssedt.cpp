#include "sdf_8ssedt.h"

#define new VNEW

NS_BEGIN

//https://zhuanlan.zhihu.com/p/337944099
SdfPoint SdfPoint::Inside(0, 0);
SdfPoint SdfPoint::Empty(9999, 9999);

void SdfGrid::Generate_SDF(BYTE* pTargetBuffer, int targetPitch, BYTE* buffer, int w, int h, int srcW, int srcH, int downScale, int spread)
{
	int dy, dy0 = -spread, dy1 = h + spread;

	// For every destination distance sample

	float scale = 255.0f / ((spread + 1) * 2 * downScale);

	for (dy = dy0; dy < dy1; dy++)
	{
		int sy0 = (dy - spread - 1) * downScale;
		int sy1 = (dy + spread + 1) * downScale;
		float cy = (dy + 0.5f) * downScale;

		for (int dx = -spread; dx < (w + spread); dx++)
		{
			// Get distance to every source pixel (within spread)
			int sx0 = (dx - spread - 1) * downScale;
			int sx1 = (dx + spread + 1) * downScale;
			float cx = (dx + 0.5f) * downScale;

			float d0 = 1e22f; // Distance to solid
			float d1 = 1e22f; // Distance to empty

			for (int sy = sy0; sy <= sy1; sy++)
			{
				float yp = sy + 0.5f;

				for (int sx = sx0; sx <= sx1; sx++)
				{
					float xp = sx + 0.5f;
					float d = (cx - xp) * (cx - xp) + (cy - yp) * (cy - yp);
					int p = 0;
					if (sx >= 0 && sy >= 0 && sx < srcW && sy < srcH)
						p = buffer[sx + sy * srcW];

					if (p)
						d0 = std::min(d0, d);
					else
						d1 = std::min(d1, d);
				}
			}

			if (d1 < d0)
			{ // Outside the shape
				float d = std::max(0.0f, std::min(127.5f, std::sqrt(d0) * scale));
				ASSERT(dx - dy * targetPitch >= 0);
				ASSERT(dx - dy * targetPitch < targetPitch * h);
				pTargetBuffer[dx - dy * targetPitch] = (BYTE)(127.5f - d + 0.5f);
			}
			else
			{
				float d = std::max(0.0f, std::min(127.5f, std::sqrt(d1) * scale));
				ASSERT(dx - dy * targetPitch >= 0);
				ASSERT(dx - dy * targetPitch < targetPitch* h);
				pTargetBuffer[dx - dy * targetPitch] = (BYTE)(127.5f + d + 0.5f);
			}
		}
	}
}

//https://tkmikyon.medium.com/computing-the-signed-distance-field-a1fa9ba2fc7d
using namespace std;
// We use 64-bit integers to avoid some annoying integer math
// overflow corner cases.
using Metric = function<float(int64_t, int64_t)>;
float euclidian(int64_t dx, int64_t dy)
{
	return (float)sqrtf((float)dx * dx + (float)dy * dy);
}

void sdf_partial(
	const vector<bool>& in_filled, int width,
	vector<pair<int, int>>* in_half_vector, Metric metric, bool negate);

vector<float> sdf(const vector<bool>& in_filled, int width)
{
	const int height = (int)in_filled.size() / width;
	// Initialize vectors represented as half values.
	vector<pair<int, int>> half_vector(
		in_filled.size(), { 2 * width + 1, 2 * height + 1 });
	sdf_partial(in_filled, width, &half_vector, euclidian, false);
	sdf_partial(in_filled, width, &half_vector, euclidian, true);
	vector<float> out(in_filled.size());
	for (size_t i = 0; i < half_vector.size(); i++) {
		auto [dx, dy] = half_vector[i];
		//out[i] = metric(dx, dy) / 2;
		out[i] = euclidian(dx, dy) / 2;
		if (in_filled[i])
			out[i] = -out[i];
	}
	return out;
}
void sdf_partial(
	const vector<bool>& in_filled, int width,
	vector<pair<int, int>>* in_half_vector, Metric metric, bool negate) {
	assert(width != 0);
	const auto height = in_filled.size() / width;
	assert(height != 0);
	auto valid_pixel = [&](int x, int y) {
		return (x >= 0) && (x < width) && (y >= 0) && (y < height); };
	auto coord = [&](int x, int y) { return x + width * y; };
	auto filled = [&](int x, int y) -> bool {
		if (valid_pixel(x, y))
			return in_filled[coord(x, y)] ^ negate;
		return false ^ negate;
	};
	// Allows us to write loops over a neighborhood of a cell.
	auto do_neighbors = [&](int x, int y, function<void(int, int)> f) {
		for (int dy = -1; dy <= 1; dy++)
			for (int dx = -1; dx <= 1; dx++)
				if (valid_pixel(x + dx, y + dy))
					f(x + dx, y + dy);
	};
	auto& half_vector = *in_half_vector;
	vector<bool> closed(in_filled.size());
	struct QueueElement {
		int x, y, dx, dy;
		float dist;
	};
	struct QueueCompare {
		bool operator() (QueueElement& a, QueueElement& b) {
			return a.dist > b.dist;
		}
	};
	priority_queue<
		QueueElement, vector<QueueElement>, QueueCompare> pq;
	auto add_to_queue = [&](int x, int y, int dx, int dy) {
		pq.push({ x, y, dx, dy, metric(dx, dy) });
	};
	// A. Seed phase: Find all filled (black) pixels that border an
	// empty pixel. Add half distances to every surrounding unfilled
	// (white) pixel.
	for (int y = 0; y < height; y++) {
		for (int x = 0; x < width; x++) {
			if (filled(x, y)) {
				do_neighbors(x, y, [&](int x2, int y2) {
					if (!filled(x2, y2))
					add_to_queue(x2, y2, x2 - x, y2 - y);
					});
			}
		}
	}
	// B. Propagation phase: Add surrounding pixels to queue and
	// discard the ones that are already closed.
	while (!pq.empty()) {
		auto current = pq.top();
		pq.pop();
		// If it's already been closed then the shortest vector has
		// already been found.
		if (closed[coord(current.x, current.y)])
			continue;
		// Close this one and store the half vector.
		closed[coord(current.x, current.y)] = true;
		half_vector[coord(current.x, current.y)] = {
		  current.dx, current.dy };
		// Add all open neighbors to the queue.
		do_neighbors(current.x, current.y, [&](int x2, int y2) {
			if (!filled(x2, y2) && !closed[coord(x2, y2)]) {
				int dx = 2 * (x2 - current.x);
				int dy = 2 * (y2 - current.y);
				auto [ddx, ddy] = half_vector[coord(current.x, current.y)];
				dx += ddx;
				dy += ddy;
				add_to_queue(x2, y2, dx, dy);
			}
			});
	}
}

vector<float> SdfGrid::Generate_SDF2(const vector<bool>& in_filled, int width)
{
	return sdf(in_filled, width);
}

NS_END