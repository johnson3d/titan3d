using System.Collections.Generic;

namespace EngineNS.Bricks.Procedure.Node
{
    public class FlowNode
    {
        static float[] cr = { 0.5f, 0, 1, 0, 1, 0.5f, 0.5f, 0, 1 };
        static float[] cg = { 0.5f, 0, 0, 1, 1, 0, 1, 0.5f, 0.5f };
        public unsafe static UBufferComponent GenerateFlowMap_GIS(UBufferComponent map)
        {
            var creator = UBufferCreator.CreateInstance<USuperBuffer<Vector4, FFloat4Operator>>(map.Width, map.Height, 1);
            var flowMap = UBufferComponent.CreateInstance(creator);
            int width = map.Width;
            int height = map.Height;
            int count = width * width;
            float[] elevations = Algorithm.Common.WrapNode.Unpack(map);
            float[] filled_elevations = new float[count];
            TtRiverNode.fill_lake(elevations, filled_elevations, width, height);
            int[] FD = new int[count];
            TtRiverNode.calc_flow_directions(filled_elevations, FD, width, height);
            TtRiverNode.calc_flow_directions_flats(filled_elevations, FD, width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int fd = FD[i + j * width];
                    flowMap.SetPixel(i, j, new Color4f(cr[fd], cg[fd], 0));
                }
            }
            return flowMap;

        }

        public static UBufferComponent GenearteFlowMap_Watershed(UBufferComponent map, int level = 20, int grad = 5, bool only_slope = true)
        {
            var creator = UBufferCreator.CreateInstance<USuperBuffer<Vector4, FFloat4Operator>>(map.Width, map.Height, 1);
            var flowMap = UBufferComponent.CreateInstance(creator);
            int width = map.Width;
            int count = map.Width * map.Height;
            // Divide map date into different level
            int[] dividedResult = divide_tex(map, level);
            // Grourp levels
            SortedDictionary<int, List<int>> groupedResult = group_result(ref dividedResult);
            // Watershed algorithm
            {
                float[] result = watershed_track(groupedResult, width, count, only_slope, true);
                for (int i = 0; i < count; i++)
                {
                    var color = new Color4f(
                        ((float)result[i * 2] / grad + 1) / 2,
                        ((float)result[i * 2 + 1] / grad + 1) / 2,
                        0,
                        1
                    );
                    flowMap.SetPixel(
                        i % width,
                        i / width,
                        color
                    );
                }
            }
            //flowMap.Apply();
            return flowMap;
        }
        public static unsafe int[] divide_tex(UBufferComponent toDivide, int level)
        {
            int[] result = new int[toDivide.Width * toDivide.Height];
            var pixels = (Color4f*)toDivide.GetSuperPixelAddress(0, 0, 0);
            for (int i = 0; i < result.Length; i++) result[i] = MathHelper.FloorToInt(pixels[i].Red * level);
            return result;
        }
        public static SortedDictionary<int, List<int>> group_result(ref int[] dividedResult)
        {
            SortedDictionary<int, List<int>> groupedResult = new SortedDictionary<int, List<int>>();
            for (int i = 0; i < dividedResult.Length; i++)
            {
                if (!groupedResult.ContainsKey(dividedResult[i]))
                {
                    groupedResult.Add(dividedResult[i], new List<int>() { i });
                }
                else
                {
                    groupedResult[dividedResult[i]].Add(i);
                }
            }
            // Sort (have been sorted automatically)

            return groupedResult;
        }

        public static float[] watershed_track(SortedDictionary<int, List<int>> groupedResult, int width, int count, bool only_slope, bool should_lerp)
        {
            float[] result = new float[count * 2];
            int[] record = new int[count];
            for (int i = 0; i < record.Length; i++) record[i] = 0;

            int lastLevel = -1;
            float adj_x = -1, adj_y = -1;
            foreach (var pair in groupedResult)
            {
                int level = pair.Key;
                List<int> pixels = pair.Value;
                List<int> temp = new List<int>();
                // check
                bool newAdd = true;
                while (pixels.Count != 0 && newAdd)
                {
                    newAdd = false;
                    foreach (var pixel in pixels)
                    {
                        if (searchAroundTrack(pixel, level, width, count, ref record, ref result, ref adj_x, ref adj_y, only_slope, should_lerp))
                        {
                            result[2 * pixel] = adj_x;
                            result[2 * pixel + 1] = adj_y;
                            newAdd = true;
                            temp.Add(pixel);
                        }
                    }
                    foreach (var pixel in temp)
                    {
                        record[pixel] = level;
                        pixels.Remove(pixel);
                    }

                    temp.Clear();
                }

                foreach (var pixel in pixels)
                {
                    result[2 * pixel] = 0;
                    result[2 * pixel + 1] = 0;
                    record[pixel] = level;
                }


                lastLevel = level;
            }

            return result;
        }

        private static bool searchAroundTrack(int index, int level, int width, int count, ref int[] record, ref float[] result, ref float adj_x, ref float adj_y, bool only_slope, bool should_lerp)
        {
            bool find = false;
            adj_x = 0;
            adj_y = 0;
            if (index - 1 > -1 && index + 1 < count && index - width > -1 && index + width < count)
            {
                // left and right
                if (record[index - 1] != 0)
                {
                    adj_x = -(level - record[index - 1]);
                    find = true;
                }
                else if (record[index + 1] != 0)
                {
                    adj_x = (level - record[index + 1]);
                    find = true;
                }
                // up and down
                if (record[index - width] != 0)
                {
                    adj_y = -(level - record[index - width]);
                    find = true;
                }
                else if (record[index + width] != 0)
                {
                    adj_y = (level - record[index + width]);
                    find = true;
                }
            }
            if (!only_slope && find && adj_x == 0 && adj_y == 0)
            {
                int rec_x = 0, rec_y = 0;
                if (record[index - 1] != 0) rec_x = -1;
                else if (record[index + 1] != 0) rec_x = 1;
                if (record[index - width] != 0) rec_y = -1;
                else if (record[index + width] != 0) rec_y = 1;

                if (should_lerp)
                {
                    if (rec_x != 0 && rec_y != 0)
                    {
                        adj_x = (result[(index + rec_x) * 2] + result[(index + rec_y * width) * 2]) / 2;
                        adj_y = (result[(index + rec_x) * 2 + 1] + result[(index + rec_y * width) * 2 + 1]) / 2;
                    }
                    else if (rec_x != 0 && rec_y == 0)
                    {
                        adj_x = result[(index + rec_x) * 2];
                    }
                    else if (rec_y != 0 && rec_x == 0)
                    {
                        adj_y = result[(index + rec_y * width) * 2 + 1];
                    }
                }
                else
                {
                    adj_x = rec_x;
                    adj_y = rec_y;
                }

            }
            return find;
        }

        private static float[] CalcWatershed_WaterLevel(SortedDictionary<int, List<int>> groupedResult, int width, int count, bool use_local, float color_step, float water_level)
        {
            float[] result = new float[count];
            for (int i = 0; i < result.Length; i++) result[i] = 0.0f;
            int[] record = new int[count];
            for (int i = 0; i < record.Length; i++) record[i] = -1;
            int searched_index = 0;
            int local_height = 0;
            foreach (var pair in groupedResult)
            {
                int level = pair.Key;
                List<int> pixels = pair.Value;
                List<int> temp = new List<int>();
                // check
                bool newAdd = true;
                while (pixels.Count != 0 && newAdd)
                {
                    newAdd = false;
                    foreach (var pixel in pixels)
                    {
                        if (searchAroundArea(pixel, level, width, count, ref record, ref searched_index, ref local_height))
                        {
                            if (level > water_level)
                            {
                                // float target = result[searched_index] - ((local_height == 1) ? color_step : color_step / 1.41f);
                                float target = result[searched_index] - color_step;
                                target = target < 0 ? 0 : target;
                                result[pixel] = target;
                            }
                            else
                            {
                                result[pixel] = 1;
                            }
                            newAdd = true;
                            temp.Add(pixel);
                        }
                    }
                    Profiler.Log.WriteInfoSimple("Add " + temp.Count);
                    foreach (var pixel in temp)
                    {
                        record[pixel] = level;
                        pixels.Remove(pixel);
                    }
                    temp.Clear();
                }
                foreach (var pixel in pixels)
                {
                    if (level < water_level) result[pixel] = 1;
                    // if (use_local)
                    // {
                    //     result[pixel] = 1;
                    // }
                    record[pixel] = level;
                }
            }
            return result;
        }

        private static bool searchAroundArea(int index, int level, int width, int count, ref int[] record, ref int searched_index, ref int local_height)
        {
            bool find = false;
            // int left = 0, right = 0, up = 0, down = 0, min = 99999999;
            local_height = 0;
            if (index - 1 > -1 && index + 1 < count && index - width > -1 && index + width < count)
            {
                if (record[index - 1] != -1)
                {
                    searched_index = index - 1;
                    local_height++;
                    find = true;
                }
                if (record[index + 1] != -1)
                {
                    searched_index = index + 1;
                    local_height++;
                    find = true;
                }
                if (record[index - width] != -1)
                {
                    searched_index = index - width;
                    local_height++;
                    find = true;
                }
                if (record[index + width] != -1)
                {
                    searched_index = index + width;
                    local_height++;
                    find = true;
                }
            }

            return find;
        }

        private static float[] watershed_lowset(SortedDictionary<int, List<int>> groupedResult, int width, int count, float color_step, int min_lake_area, float min_water_level)
        {
            float[] result = new float[count];
            for (int i = 0; i < result.Length; i++) result[i] = 0.0f;
            int[] record = new int[count];
            for (int i = 0; i < record.Length; i++) record[i] = -1;
            int searched = 0;
            foreach (var pair in groupedResult)
            {
                int level = pair.Key;
                List<int> pixels = pair.Value;
                List<int> temp = new List<int>();
                // check
                bool newAdd = true;
                while (pixels.Count != 0 && newAdd)
                {
                    newAdd = false;
                    foreach (var pixel in pixels)
                    {
                        if (searchAround(pixel, width, count, ref record, ref searched))
                        {
                            newAdd = true;
                            float value = result[searched] - color_step;
                            result[pixel] = value < 0 ? 0 : value;
                            temp.Add(pixel);
                        }
                    }
                    foreach (var pixel in temp)
                    {
                        record[pixel] = 1;
                        pixels.Remove(pixel);
                    }
                    temp.Clear();
                }
                // handle lake
                if (level < min_water_level)
                {
                    foreach (var pixel in pixels)
                    {
                        record[pixel] = 1;
                    }
                }
                else
                {
                    List<int> lake = new List<int>();
                    while (pixels.Count != 0)
                    {
                        Profiler.Log.WriteInfoSimple(level + "before " + pixels.Count);
                        lake.Clear();
                        lake.Add(pixels[0]);
                        record[pixels[0]] = 1;
                        pixels.RemoveAt(0);
                        newAdd = true;
                        while (newAdd)
                        {
                            newAdd = false;
                            temp.Clear();
                            foreach (var pixel in pixels)
                            {
                                if (searchAroundSet(pixel, width, ref lake))
                                {
                                    temp.Add(pixel);
                                    newAdd = true;
                                }
                            }
                            foreach (var pixel in temp)
                            {
                                record[pixel] = 1;
                                lake.Add(pixel);
                                pixels.Remove(pixel);
                            }
                            // Debug.Log(level + " find" + lake.Count + " " + newAdd); 
                        }
                        if (lake.Count > min_lake_area)
                        {
                            Profiler.Log.WriteInfoSimple("Lake Area " + lake.Count);
                            foreach (var pixel in lake)
                            {
                                result[pixel] = 1;
                                //    if (temp.Count < 100)
                                //    Debug.Log((pixel % PixelWidth) + " " + (pixel / PixelWidth));
                            }
                        }
                        Profiler.Log.WriteInfoSimple(level + "after " + pixels.Count);
                    }
                }
            }
            return result;
        }

        public static bool searchAround(int index, int width, int count, ref int[] record, ref int searched)
        {
            if (index - 1 > -1 && index + 1 < count && index - width > -1 && index + width < count)
            {
                if (record[index - 1] != -1)
                {
                    searched = index - 1;
                    return true;
                }
                else if (record[index + 1] != -1)
                {
                    searched = index + 1;
                    return true;
                }
                else if (record[index - width] != -1)
                {
                    searched = index - width;
                    return true;
                }
                else if (record[index + width] != -1)
                {
                    searched = index + width;
                    return true;
                }
            }
            return false;
        }

        public static bool searchAroundSet(int index, int width, ref List<int> set)
        {
            return set.Contains(index - 1) || set.Contains(index + 1) || set.Contains(index - width) || set.Contains(index + width);
        }
    }
}
