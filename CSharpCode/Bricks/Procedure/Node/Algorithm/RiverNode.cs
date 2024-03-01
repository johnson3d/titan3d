using System.Collections.Generic;

namespace EngineNS.Bricks.Procedure.Algorithm
{
    public class RiverNode
    {
        static int[] dx = {0,-1,1,-1,1,0,0,-1,1};
        static int[] dy = {0,-1,-1,1,1,-1,1,0,0};
        static float[] dw = {0 ,0.707f, 0.707f, 0.707f, 0.707f, 1, 1, 1, 1, 1};
        const int NO_FLOW = 0;
        const float MASK_THRESHOLD = 0.1f;

        private static bool inside_map(int x, int y, int width) {return x > -1 && x < width && y > -1 && y < width;}
        
        public static void fill_lake(ref float[] elevations, ref float[] fills, int width)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    fills[i + j * width] = (i == 0 || i == width - 1 || j == 0 || j == width - 1) ? elevations[i + j * width] : 1.0f;
                }
            }
            bool change = true;
            while (change)
            {
                change = false;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int current = i + j * width;
                        if (fills[current] > elevations[current])
                        {
                            for (int n = 1; n <= 8; n++)
                            {
                                if (inside_map(i + dx[n], j + dy[n], width))
                                {
                                    int target = i + dx[n] + (j + dy[n]) * width;
                                    if (elevations[current] >= fills[target])
                                    {
                                        fills[current] = elevations[current];
                                        change = true;
                                    }
                                    else if (fills[current] > fills[target])
                                    {
                                        fills[current] = fills[target];
                                        change = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public static void calc_flow_directions(ref float[] elevations, ref int[] FD, int width, bool write_flat = false)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int min_flow = NO_FLOW;
                    float min_step = 0;
                    for (int n = 1; n <= 8; n++)
                    {

                        if (i + dx[n] >= 0 && i + dx[n] <= width - 1 && j + dy[n] >= 0 && j + dy[n] <= width - 1)
                        {
                            int target = i + dx[n] + (j + dy[n]) * width;
                            float step = (elevations[target] - elevations[i + j * width]) * dw[n];
                            if (
                                write_flat ?
                                (step <= min_step) :
                                (step < min_step)
                                )
                            {
                                min_step = step;
                                min_flow = n;
                            }
                        }
                    }
                    FD[i + j * width] = min_flow;
                }
            }

        }

        private static void calc_flow_accumulations(ref int[] FD, ref int[] FA, ref int[] mask, int width)
        {
            for (int i = 0; i < width * width; i++) FA[i] = 0;
            for (int j = 0; j < width; j++)
            for (int i = 0; i < width; i++)
                compute_flow(i, j, width, ref FD, ref FA, ref mask);
        }

        private static void compute_flow(int i, int j, int width, ref int[] FD, ref int[] FA, ref int[] mask)
        {
            if (mask[i + j * width] > 0) return; 
            FA[i + j * width] ++;
            int ni = 0, nj = 0;
            if (get_next(i, j, ref ni, ref nj, ref FD, width))
                compute_flow(ni, nj, width, ref FD, ref FA, ref mask);
        }

        private static bool get_next(int i, int j, ref int ni, ref int nj, ref int[] FD, int width)
        {
            if (!inside_map(i, j, width)) return false;
            int d = FD[i + j * width];
            ni = dx[d] + i;
            nj = dy[d] + j;
            if (ni == i && nj == j) return false;
            if (!inside_map(ni, nj, width)) return false;
            return true;
        }


        private static void gradient_towards_lower(ref float[] elevations, ref List<Vector2i> flats, ref int[] flows, ref int[] inc1, int width)
        {
            int loops = 0;
            int number_incremented = -1;
            foreach(var f in flats) inc1[f.X + f.Y * width]++;
            while (number_incremented != 0)
            {
                number_incremented = 0;
                for (int i = 0; i < flats.Count; i++)
                {
                    bool increment_elevation = true;
                    int x = flats[i].X;
                    int y = flats[i].Y;
                    for (int n = 1; n <= 8; n++)
                    {
                        if (inside_map(x + dx[n], y + dy[n], width))
                        {
                            int current = x + y * width;
                            int target = (x + dx[n]) + (y + dy[n]) * width;
                            if (elevations[target] == elevations[current] && flows[target] != NO_FLOW)
                            {
                                increment_elevation = false; break;
                            }
                            else if (elevations[target] == elevations[current] && inc1[target] < loops)
                            {
                                increment_elevation = false; break;
                            }
                        }
                        else
                        {
                            increment_elevation = false; break;
                        }
                    }
                    if (increment_elevation)
                    {
                        inc1[x + y * width] ++;
                        number_incremented ++;
                    }
                }
                loops++;
            }
        }

        private static void gradient_from_higher(ref float[] elevations, ref List<Vector2i> flats, ref int[] flows, ref int[] inc2, int width)
        {
            int last_incremented = -1;
            int number_incremented = 0;
            List<int> to_inc = new List<int>();

            while (last_incremented < number_incremented)
            {
                foreach(var t in to_inc) inc2[t]++;
                to_inc.Clear();
                for (int i = 0; i < flats.Count; i++)
                {
                    bool has_higher = false;
                    int x = flats[i].X;
                    int y = flats[i].Y;
                    for (int n = 1; n <= 8; n++)
                    {
                        if (inside_map(x + dx[n], y + dy[n], width))
                        {
                            int current = x + y * width;
                            int target = x + dx[n] + (y + dy[n]) * width;
                            if (!has_higher && 
                            (elevations[target] > elevations[current] || inc2[target] != 0))
                            {
                                has_higher = true;
                            }
                        }
                    }
                    if (has_higher)
                    {
                        to_inc.Add(x + y * width);
                    }
                }
                last_incremented = number_incremented;
                number_incremented = to_inc.Count;
            }
        }

        private static void redirect_flats(ref float[] elevations, ref List<Vector2i> flats, ref int[] inc1, ref int[] inc2, ref int[] FD,int width)
        {
            foreach(var f in flats)
            {
                int x = f.X;
                int y = f.Y;
                float max_step = 0.0f;
                int flow_target = NO_FLOW;
                for (int n = 1; n <= 8; n++)
                {
                    if (inside_map(x + dx[n], y + dy[n], width))
                    {
                        int current = x + y * width;
                        int target = x + dx[n] + (y + dy[n]) * width;
                        int inc_current = inc1[current] + inc2[current];
                        int inc_target = inc1[target] + inc2[target];
                        if (inc_target != 0 && inc_current >= inc_target)
                        {
                            float current_step = (inc_current - inc_target + 1) * dw[n];
                            if (current_step > max_step)
                            {
                                max_step = current_step;
                                flow_target = n;
                            }
                        }
                        else if (inc_target == 0 && elevations[current] == elevations[target])
                        {
                            max_step = float.MaxValue;
                            flow_target = n;
                        }
                    }
                    else
                    {
                        max_step = float.MaxValue;
                        flow_target = n;
                    }
                }
                FD[x + y * width] = flow_target;
            }
        }

        public static void calc_flow_directions_flats(ref float[] elevations, ref int[] FD, int width)
        {
            List<Vector2i> flats = new List<Vector2i>();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < width; j++)
                    if (FD[i + j * width] == NO_FLOW)
                        flats.Add(new Vector2i(i, j));
            int[] inc1 = new int[width * width];
            int[] inc2 = new int[width * width];
            gradient_towards_lower(ref elevations, ref flats, ref FD, ref inc1, width);
            gradient_from_higher(ref elevations, ref flats, ref FD, ref inc2, width);
            redirect_flats(ref elevations, ref flats, ref inc1, ref inc2, ref FD, width);
        }

        public static UBufferComponent GenerateRiver(float[] elevations, int width, int[] dry_mask, int[] river_mask)
        {
            int count = width * width;
            var creator = UBufferCreator.CreateInstance<USuperBuffer<Vector4, FFloat4Operator>>(width, width, 1);
            var RiverMap = UBufferComponent.CreateInstance(creator);
            // read data
            int[] flow_mask = new int[count];
            for (int i = 0; i < width; i++) 
            {
                for (int j = 0; j < width; j++) 
                {
                    flow_mask[i + j * width] = dry_mask[i + j * width] + river_mask[i + j * width];
                }
            }
            // Fill Lake
            float[] filled_elevations = new float[count];
            fill_lake(ref elevations, ref filled_elevations, width);
            // Calculate Flow Direction and Accumulation
            int[] FD = new int[count];
            int[] FA = new int[count];
            calc_flow_directions(ref filled_elevations, ref FD, width);
            calc_flow_directions_flats(ref filled_elevations, ref FD, width);
            calc_flow_accumulations(ref FD, ref FA, ref flow_mask, width);
            // map into [0, 1]
            int max = 0;
            for (int i = 0; i < count; i++) max = (max < FA[i]) ? FA[i] : max;
            float[] result = new float[count];
            for (int i = 0; i < count; i++) result[i] = (float) FA[i] / max;
            // fill mask
            for (int i = 0; i < count; i++) result[i] = river_mask[i] != 0 ? river_mask[i] : result[i];
            // write data
            for (int i = 0; i < width; i++)
                for (int j = 0; j < width; j++)
                    RiverMap.SetPixel(i, j, new Color4f(result[i + j * width], result[i + j * width], result[i + j * width]));            
            //RiverMap.Apply();
            return RiverMap;
        }
        
    }
}