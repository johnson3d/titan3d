using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("River", "Water\\River", UPgcGraph.PgcEditorKeyword)]
    public class TtRiverNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn DryMaskPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn RiverMaskPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut RiverMapPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut FlowMaskMapPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut FillElevationPin { get; set; } = new PinOut();
        [Rtti.Meta]
        public UBufferCreator SourceDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        [Rtti.Meta]
        public UBufferCreator ResultDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public TtRiverNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", SourceDesc);
            AddInput(DryMaskPin, "DryMask", UBufferCreator.CreateInstance<USuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1));
            AddInput(RiverMaskPin, "DryMask", UBufferCreator.CreateInstance<USuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1));
            AddOutput(RiverMapPin, "RiverMap", ResultDesc);
            AddOutput(FlowMaskMapPin, "FlowMaskMap", UBufferCreator.CreateInstance<USuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1));
            AddOutput(FillElevationPin, "FillElevation", UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1)); 
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (RiverMapPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HeightPin);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return null;
        }

        static int[] dx = {0,-1,1,-1,1,0,0,-1,1};
        static int[] dy = {0,-1,-1,1,1,-1,1,0,0};
        static float[] dw = {0 ,0.707f, 0.707f, 0.707f, 0.707f, 1, 1, 1, 1, 1};
        const int NO_FLOW = 0;
        const float MASK_THRESHOLD = 0.1f;

        private static bool inside_map(int x, int y, int width, int height) {return x > -1 && x < width && y > -1 && y < height;}
        
        public static unsafe void fill_lake(float* elevations, float* fills, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    fills[i + j * width] = (i == 0 || i == width - 1 || j == 0 || j == height - 1) ? elevations[i + j * width] : 1.0f;
                }
            }
            bool change = true;
            while (change)
            {
                change = false;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int current = i + j * width;
                        if (fills[current] > elevations[current])
                        {
                            for (int n = 1; n <= 8; n++)
                            {
                                if (inside_map(i + dx[n], j + dy[n], width, height))
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
        public static unsafe void fill_lake(float[] elevations, float[] fills, int width, int height)
        {
            fixed(float* pE = &elevations[0])
            fixed (float* pF = &fills[0])
            {
                fill_lake(pE, pF, width, height);
            }
        }


        public unsafe static void calc_flow_directions(float* elevations, int[] FD, int width, int height, bool write_flat = false)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int min_flow = NO_FLOW;
                    float min_step = 0;
                    for (int n = 1; n <= 8; n++)
                    {

                        if (i + dx[n] >= 0 && i + dx[n] <= width - 1 && j + dy[n] >= 0 && j + dy[n] <= height - 1)
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
        public unsafe static void calc_flow_directions(float[] elevations, int[] FD, int width, int height, bool write_flat = false)
        {
            fixed(float* pE = &elevations[0])
            {
                calc_flow_directions(pE, FD, width, height, write_flat);
            }
        }

        private unsafe static void calc_flow_accumulations(int[] FD, int[] FA, int* mask, int width, int height)
        {
            for (int i = 0; i < width * width; i++)
            {
                FA[i] = 0;
            }
            for (int j = 0; j < width; j++)
            {
                for (int i = 0; i < height; i++)
                {
                    compute_flow(i, j, width, height, FD, FA, mask);
                }
            }
        }

        private unsafe static void compute_flow(int i, int j, int width, int height, int[] FD, int[] FA, int* mask)
        {
            if (mask[i + j * width] > 0)
                return; 
            FA[i + j * width] ++;
            int ni = 0, nj = 0;
            if (get_next(i, j, ref ni, ref nj, FD, width, height))
            {
                compute_flow(ni, nj, width, height, FD, FA, mask);
            }
        }

        private static bool get_next(int i, int j, ref int ni, ref int nj, int[] FD, int width, int height)
        {
            if (!inside_map(i, j, width, height)) 
                return false;
            int d = FD[i + j * width];
            ni = dx[d] + i;
            nj = dy[d] + j;
            if (ni == i && nj == j) return false;
            if (!inside_map(ni, nj, width, height)) 
                return false;
            return true;
        }


        private unsafe static void gradient_towards_lower(float* elevations, List<Vector2i> flats, int[] flows, int[] inc1, int width, int height)
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
                        if (inside_map(x + dx[n], y + dy[n], width, height))
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

        private unsafe static void gradient_from_higher(float* elevations, List<Vector2i> flats, int[] flows, int[] inc2, int width, int height)
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
                        if (inside_map(x + dx[n], y + dy[n], width, height))
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

        private unsafe static void redirect_flats(float* elevations, List<Vector2i> flats, int[] inc1, int[] inc2, int[] FD,int width, int height)
        {
            foreach(var f in flats)
            {
                int x = f.X;
                int y = f.Y;
                float max_step = 0.0f;
                int flow_target = NO_FLOW;
                for (int n = 1; n <= 8; n++)
                {
                    if (inside_map(x + dx[n], y + dy[n], width, height))
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

        public unsafe static void calc_flow_directions_flats(float* elevations, int[] FD, int width, int height)
        {
            List<Vector2i> flats = new List<Vector2i>();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < width; j++)
                    if (FD[i + j * width] == NO_FLOW)
                        flats.Add(new Vector2i(i, j));
            int[] inc1 = new int[width * width];
            int[] inc2 = new int[width * width];
            gradient_towards_lower(elevations, flats, FD, inc1, width, height);
            gradient_from_higher(elevations, flats, FD, inc2, width, height);
            redirect_flats(elevations, flats, inc1, inc2, FD, width, height);
        }
        public unsafe static void calc_flow_directions_flats(float[] elevations, int[] FD, int width, int height)
        {
            fixed(float* pE = &elevations[0])
            {
                calc_flow_directions_flats(pE, FD, width, height);
            }
        }

        //public static UBufferComponent GenerateRiver(float[] elevations, int width, int[] dry_mask, int[] river_mask)
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var HeightMap = graph.BufferCache.FindBuffer(HeightPin);
            var DryMaskMap = graph.BufferCache.FindBuffer(DryMaskPin);
            var RiverMaskMap = graph.BufferCache.FindBuffer(RiverMaskPin);
            var RiverMap = graph.BufferCache.FindBuffer(RiverMapPin);
            var FlowMaskMap = graph.BufferCache.FindBuffer(FlowMaskMapPin);
            var FillElevation = graph.BufferCache.FindBuffer(FillElevationPin); 
            // read data
            for (int i = 0; i < HeightMap.Width; i++) 
            {
                for (int j = 0; j < HeightMap.Height; j++) 
                {
                    if (DryMaskMap == null)
                    {

                    }
                    sbyte mask = (sbyte)(DryMaskMap.GetPixel<sbyte>(i, j) + RiverMaskMap.GetPixel<sbyte>(i, j));
                    FlowMaskMap.SetPixel<sbyte>(i, j, mask);
                }
            }
            int width = HeightMap.Width;
            int height = HeightMap.Height;
            int count = width * height;
            // Fill Lake
            fill_lake((float*)HeightMap.SuperPixels.DataPointer, (float*)FillElevation.SuperPixels.DataPointer, width, height);
            // Calculate Flow Direction and Accumulation
            int[] FD = new int[count];
            int[] FA = new int[count];
            calc_flow_directions((float*)FillElevation.SuperPixels.DataPointer, FD, width, height);
            calc_flow_directions_flats((float*)FillElevation.SuperPixels.DataPointer, FD, width, height);
            calc_flow_accumulations(FD, FA, (int*)FlowMaskMap.SuperPixels.DataPointer, width, height);
            // map into [0, 1]
            int max = 0;
            for (int i = 0; i < count; i++)
            {
                max = (max < FA[i]) ? FA[i] : max;
            }
            float[] result = new float[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = (float)FA[i] / max;
            }
            //// fill mask
            //for (int i = 0; i < count; i++)
            //{
            //    result[i] = river_mask[i] != 0 ? river_mask[i] : result[i];
            //}
            // write data
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var rm = RiverMaskMap.GetPixel<sbyte>(i, j);
                    if (rm != 0)
                    {
                        RiverMap.SetPixel(i, j, (float)rm); 
                    }
                    else
                    {
                        RiverMap.SetPixel(i, j, result[i]);
                    }
                }
            }
            return true;
        }
        
    }

    [Bricks.CodeBuilder.ContextMenu("Lake", "Water\\Lake", UPgcGraph.PgcEditorKeyword)]
    public class TtLakeNode : Node.UAnyTypeMonocular
    {
        static int[] dx = { 0, -1, 1, -1, 1, 0, 0, -1, 1 };
        static int[] dy = { 0, -1, -1, 1, 1, -1, 1, 0, 0 };
        private static bool inside_map(int x, int y, int width, int height) 
        { 
            return x > -1 && x < width && y > -1 && y < height; 
        }
        //public static unsafe void fill_lake(float* elevations, float* fills, int width, int height)
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var Input = graph.BufferCache.FindBuffer(SrcPin);
            var Output = graph.BufferCache.FindBuffer(ResultPin);

            for (int i = 0; i < Input.Width; i++)
            {
                for (int j = 0; j < Input.Height; j++)
                {
                    if (i == 0 || i == Input.Width - 1 || j == 0 || j == Input.Height - 1)
                    {
                        Output.SetPixel<float>(i, j, 0, Input.GetPixel<float>(i, j, 0));
                    }
                    else
                    {
                        Output.SetPixel<float>(i, j, 0, 1.0f);
                    }
                }
            }
            float* elevations = (float*)Input.GetSuperPixelAddress(0, 0, 0);
            float* fills = (float*)Output.GetSuperPixelAddress(0, 0, 0);
            int width = Input.Width;
            int height = Input.Height;
            bool change = true;
            while (change)
            {
                change = false;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int current = i + j * width;
                        if (fills[current] > elevations[current])
                        {
                            for (int n = 1; n <= 8; n++)
                            {
                                if (inside_map(i + dx[n], j + dy[n], width, height))
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
            return true;
        }
    }
}