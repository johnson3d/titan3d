using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Algorithm
{
    public class FastPoissonDisk
    {
        private static Vector2 InsideUnitCircle(Random random)
        {
            var angle = (float)random.NextDouble() * 2f * Math.PI;
            var r = (float)random.NextDouble();
            return new Vector2(
                r * (float)Math.Cos(angle),
                r * (float)Math.Sin(angle));
        }
        public static List<Vector2> Calculate2D(UInt64 width, UInt64 height, float radius, int seed, int calculateDeep = 30)
        {
            var xCount = (int)Math.Ceiling(width / radius);
            var yCount = (int)Math.Ceiling(height / radius);

            var grids = new int[xCount, yCount];
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    grids[i, j] = -1;
                }
            }

            var random = new System.Random(seed);
            var x0 = new Vector2((float)(random.NextDouble() * width), (float)(random.NextDouble() * height));
            var col = (int)Math.Floor(x0.X / radius);
            var row = (int)Math.Floor(x0.Y / radius);

            var points = new List<Vector2>();
            var x0_idx = points.Count;
            points.Add(x0);
            grids[row, col] = x0_idx;

            var active_list = new List<int>();
            active_list.Add(x0_idx);

            while (active_list.Count > 0)
            {
                var xi_idx = active_list[random.Next(active_list.Count)];
                var xi = points[xi_idx];
                var found = false;

                for (var i = 0; i < calculateDeep; ++i)
                {
                    var dir = InsideUnitCircle(random);
                    var dirNormal = dir;
                    dirNormal.Normalize();
                    var xk = xi + (dirNormal * radius + dir * radius);
                    if (xk.X < 0 || xk.X >= width || xk.Y < 0 || xk.Y >= height)
                        continue;

                    col = (int)Math.Floor(xk.X / radius);
                    row = (int)Math.Floor(xk.Y / radius);

                    if (grids[row, col] != -1)
                        continue;

                    var isValid = true;
                    var min_r = (int)Math.Floor((xk.Y - radius) / radius);
                    var max_r = (int)Math.Floor((xk.Y + radius) / radius);
                    var min_c = (int)Math.Floor((xk.X - radius) / radius);
                    var max_c = (int)Math.Floor((xk.X + radius) / radius);
                    for (var or = min_r; or <= max_r; or++)
                    {
                        if (or < 0 || or >= yCount)
                            continue;

                        for (var oc = min_c; oc <= max_c; oc++)
                        {
                            if (oc < 0 || oc >= xCount)
                                continue;

                            var xj_idx = grids[or, oc];
                            if (xj_idx != -1)
                            {
                                var xj = points[xj_idx];
                                var dist = (xj - xk).Length();
                                if (dist < radius)
                                {
                                    isValid = false;
                                    break;
                                }
                            }

                            if (isValid == false)
                                break;
                        }

                        if (isValid == false)
                            break;
                    }

                    if (isValid)
                    {
                        var xk_idx = points.Count;
                        points.Add(xk);
                        grids[row, col] = xk_idx;
                        active_list.Add(xk_idx);

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    active_list.Remove(xi_idx);
                }
            }

            return points;
        }
    }
}
