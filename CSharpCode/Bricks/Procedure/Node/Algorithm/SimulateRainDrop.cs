using EngineNS.Profiler;
using System.Collections;
using System.Collections.Generic;

namespace EngineNS.Bricks.Procedure.Algorithm
{
    public class SimulateRainDrop
    {
        public UBufferComponent SimTex;
        public int SimulateSize = 512;
        public UBufferComponent FlowMap;

        public UBufferComponent TrackMap;

        public bool ShouldDrawSlope = false;

        public enum SimulateMode
        {
            SpreadMode,
            TrackMode,
            AreaMode
        }


        public bool ShouldSimulateFlow = true;

        public SimulateMode Mode = SimulateMode.SpreadMode;
        public int InitHeight = 10;
        public bool ShouldExpansion = false;
        public bool ShouldUpFlow = false;
        public int UpFlowDistance = 5;

        // Start is called before the first frame update
        void Start()
        {
            ClearSimulateTex();
            if (ShouldDrawSlope)
                DrawSlope();
            if (ShouldSimulateFlow)
                SimulateFlow();

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SimulateFlow()
        {
            switch (Mode)
            {
                case SimulateMode.SpreadMode:
                    FloodSpread();
                    break;
                case SimulateMode.TrackMode:
                    {
                        var nu = EngineNS.UEngine.RunCoroutine(IESimulateTrack());
                    }
                    break;
            }
        }
        public void ClearSimulateTex()
        {
            // SimTex = new Texture2D(SimulateSize, SimulateSize);
            for (int i = 0; i < SimTex.Width; i++)
            {
                for (int j = 0; j < SimTex.Height; j++)
                {
                    SimTex.SetPixel(i, j, Color4f.FromABGR(Color.White));
                }
            }
            //SimTex.Apply();
        }

        float COLOR_ASPECT = 127.5f / 128.0f;
        public void DrawSlope()
        {
            for (int i = 0; i < SimTex.Width; i++)
            {
                for (int j = 0; j < SimTex.Height; j++)
                {
                    SimTex.SetPixel(i, j,
                        Mode == SimulateMode.SpreadMode ?
                        FlowMap.GetPixel<Vector4>(i, j) :
                        TrackMap.GetPixel<Vector4>(i, j)
                    );
                }
            }
            //SimTex.Apply();
        }

        struct FlowField
        {
            public Vector2i Pos;
            public Vector2i Tend;
            public int Height;
        };
        public void FloodSpread()
        {
            System.DateTime beginTime = System.DateTime.Now;
            Profiler.Log.WriteInfoSimple("Start Simulate Spread");
            int ini_height = InitHeight;
            Queue<FlowField> Drops = new Queue<FlowField>();
            List<Vector2i> Record = new List<Vector2i>();
            float draw_density = 1 / (float)ini_height;

            float[] value = new float[SimTex.Width * SimTex.Width];
            int iteration = 1024;
            FlowField temp;
            while (iteration-- > 0)
            {
                Drops.Clear();
                Record.Clear();
                {
                    temp.Pos = new Vector2i(MathHelper.RandomRange(0, 512), MathHelper.RandomRange(0, 512));
                    temp.Tend = Vector2i.Zero;
                    temp.Height = ini_height;
                    Drops.Enqueue(temp);
                }
                while (Drops.Count != 0)
                {
                    FlowField current = Drops.Dequeue();
                    Vector2i pos = current.Pos;
                    int height = current.Height;
                    // Add Flow
                    if (height < 0) continue;
                    if (!checkBound(pos)) continue;
                    value[pos.X + pos.Y * SimTex.Width] += draw_density;
                    List<Vector2i> FlowList = RandomFlowSpread(pos);
                    foreach (var flow in FlowList)
                    {
                        if (!Record.Contains(pos + flow))
                        {
                            temp.Pos = pos + flow;
                            temp.Tend = flow;
                            temp.Height = height - 1;
                            Drops.Enqueue(temp);
                            Record.Add(temp.Pos);
                        }
                    }
                }
                //SimTex.Apply();
            }
            System.DateTime endTime = System.DateTime.Now;
            Profiler.Log.WriteInfoSimple("End Simulate Spread");
            Profiler.Log.WriteInfoSimple("Use Time : " + endTime.Subtract(beginTime).TotalMilliseconds + " ms");

            for (int i = 0; i < SimTex.Width * SimTex.Width; i++) value[i] = value[i] * 2 - 1;

            for (int j = 0; j < SimTex.Width; j++) for (int i = 0; i < SimTex.Width; i++) SimTex.SetPixel(i, j, new Color4f(1 - value[i + j * SimTex.Width], 1 - value[i + j * SimTex.Width], 1));

            //SimTex.Apply();
            //PCGNode.WrapNode.SaveTexture2D(SimTex, "Simulate");
        }

        static List<Vector2i> SpreadAllList = new List<Vector2i>(){
        Vector2i.Left, Vector2i.Right, Vector2i.Up, Vector2i.Down
    };
        private List<Vector2i> RandomFlowSpread(Vector2i pos)
        {
            Vector2 flow = getDirection(FlowMap, pos);
            if (flow.Length() > 0.01f
            // && MathHelper.RandomRange(0.0f, 1.0f) > 0.1f
            )
            {
                List<Vector2i> FlowList = new List<Vector2i>();
                float absX = MathHelper.Abs(flow.X);
                float absY = MathHelper.Abs(flow.Y);
                if (absX > 0.01f) FlowList.Add(flow.X > 0 ? Vector2i.Right : Vector2i.Left);
                if (absY > 0.01f) FlowList.Add(flow.Y > 0 ? Vector2i.Up : Vector2i.Down);
                return FlowList;
            }
            else
            {
                return SpreadAllList;
            }


        }

        public async IAsyncEnumerable<object> IESimulateTrack()
        {
            Profiler.Log.WriteInfoSimple("Start Simulate Track");
            yield return null;
            Queue<FlowField> Drops = new Queue<FlowField>();
            int iteration = 10;
            int seg = (SimulateSize * SimulateSize) / 100;
            int total = 100000;
            int update_wait = 1000;

            float time = System.DateTime.Now.Second;
            List<Vector2i> Record = new List<Vector2i>();
            List<Vector2i> LowSet = new List<Vector2i>();

            FlowField temp;
            temp.Tend = Vector2i.Zero;
            temp.Height = 0;

            // Track
            while (iteration-- > 0)
            {
                int n = seg;
                while (n-- > 0)
                {
                    temp.Pos = new Vector2i(MathHelper.RandomRange(0, SimulateSize), MathHelper.RandomRange(0, SimulateSize));
                    temp.Tend = Vector2i.Zero;
                    Drops.Enqueue(temp);
                }

                while (Drops.Count != 0 && total-- > 0)
                {
                    while (Drops.Count != 0)
                    {
                        FlowField current = Drops.Dequeue();
                        Vector2i pos = current.Pos;
                        Vector2i tend = current.Tend;
                        int height = current.Height;
                        var old = SimTex.GetPixel<Color4f>(pos.X, pos.Y);
                        SimTex.SetPixel(pos.X, pos.Y, old - new Color4f(0.1f, 0.1f, 0, 0));
                        // Add Track
                        if (!checkBound(pos)) break;
                        if (height < 0) break;
                        bool isLowest;
                        Vector2i flow = RandomFlowTrack(pos, tend, out isLowest);
                        {
                            if (!isLowest)
                            {
                                if (flow + tend != Vector2i.Zero)
                                {
                                    temp.Pos = pos + flow;
                                    temp.Tend = flow;
                                    temp.Height = height + 1;
                                    if (LowSet.Contains(temp.Pos))
                                    {
                                        Record.Add(pos);
                                    }
                                    else
                                    {
                                        Drops.Enqueue(temp);
                                        Record.Add(pos);
                                    }
                                }
                                else
                                {
                                    // Debug.LogWarning("Error Target !!! " + pos + " tend " + tend + " flow " + flow);
                                    Record.Add(pos);
                                    LowSet.Add(pos);
                                }
                            }
                            else if (temp.Tend != Vector2i.Zero)
                            {
                                Record.Add(pos);
                                LowSet.Add(pos);
                            }
                        }
                    }
                    //SimTex.Apply();
                    if (total % update_wait == 0)
                    {
                        Profiler.Log.WriteInfoSimple("Iter " + iteration + " Total left " + total);
                        yield return null;// new WaitForSeconds(0.1f);
                    }
                }
                //SimTex.Apply();
                //PCGNode.WrapNode.SaveTexture2D(SimTex, "DEMO/Flow/SimTex");
                Profiler.Log.WriteInfoSimple("End Iter " + iteration);
            }

            // Expansion
            bool addNew = ShouldExpansion;
            while (addNew)
            {
                addNew = false;
                List<Vector2i> toAddList = new List<Vector2i>();
                foreach (var pos in LowSet)
                {
                    List<Vector2i> result = checkExpansion(pos, ref LowSet);
                    if (result.Count != 0)
                    {
                        addNew = true;
                        foreach (var toAdd in result)
                        {
                            if (!toAddList.Contains(toAdd))
                            {
                                toAddList.Add(toAdd);
                            }
                        }
                    }
                }

                foreach (var pos in toAddList)
                {
                    LowSet.Add(pos);
                    SimTex.SetPixel(pos.X, pos.Y, Color4f.FromABGR(Color.Blue));
                }
            }
            // UpFlow
            int upFlow = ShouldUpFlow ? UpFlowDistance : 0;
            while (upFlow > 0)
            {
                List<Vector2i> toAddList = new List<Vector2i>();
                foreach (var pos in LowSet)
                {
                    List<Vector2i> result = checkUpFlow(pos, ref LowSet);
                    foreach (var toAdd in result)
                    {
                        if (!toAddList.Contains(toAdd))
                        {
                            toAddList.Add(toAdd);
                        }
                    }
                }

                foreach (var pos in toAddList)
                {
                    LowSet.Add(pos);
                    var old = SimTex.GetPixel<Color4f>(pos.X, pos.Y);
                    SimTex.SetPixel(pos.X, pos.Y, old - new Color4f(0.01f * upFlow, 0.01f * upFlow, 0, 0));
                }
                upFlow--;
            }
        }

        private Vector2i RandomFlowTrack(Vector2i pos, Vector2i tend, out bool isLowest)
        {
            Vector2 flow = getDirection(TrackMap, pos);
            if (flow.Length() > 0.01f)
            {
                isLowest = false;
                float xProb = MathHelper.Abs(flow.X);
                float yProb = MathHelper.Abs(flow.Y);
                if (MathHelper.RandomRange(0, xProb + yProb) < xProb)
                {
                    return flow.X < 0 ? Vector2i.Left : Vector2i.Right;
                }
                else
                {
                    return flow.Y < 0 ? Vector2i.Down : Vector2i.Up;
                }
            }
            else
            {
                isLowest = true;
                return Vector2i.Zero;
            }
        }

        private bool checkBound(Vector2i target)
        {
            return !(target.X < 0 || target.Y < 0 || target.X > SimTex.Width - 1 || target.Y > SimTex.Height - 1);
        }


        private Vector2 getDirection(UBufferComponent tex, Vector2i pos)
        {
            var color = tex.GetPixel<Color4f>(pos.X, pos.Y);
            float r = (color.Red * COLOR_ASPECT / 0.5f - 1.0f);
            float g = (color.Green * COLOR_ASPECT / 0.5f - 1.0f);
            Vector2 flow = new Vector2(r, g);
            return flow;
        }

        private Vector2i getDirectionInt(UBufferComponent tex, Vector2i pos)
        {
            Vector2 flow = getDirection(tex, pos);
            return new Vector2i(
                (MathHelper.Abs(flow.X) > 0.01f) ? (flow.X < 0 ? -1 : 1) : 0,
                (MathHelper.Abs(flow.Y) > 0.01f) ? (flow.Y < 0 ? -1 : 1) : 0
                );
        }
        private List<Vector2i> checkExpansion(Vector2i pos, ref List<Vector2i> set)
        {
            List<Vector2i> result = new List<Vector2i>();
            Vector2i temp = Vector2i.Zero;
            temp = pos + Vector2i.Left;
            if (checkBound(temp) && !set.Contains(temp)) if (getDirection(TrackMap, temp).Length() < 0.01f) result.Add(temp);
            temp = pos + Vector2i.Right;
            if (checkBound(temp) && !set.Contains(temp)) if (getDirection(TrackMap, temp).Length() < 0.01f) result.Add(temp);
            temp = pos + Vector2i.Up;
            if (checkBound(temp) && !set.Contains(temp)) if (getDirection(TrackMap, temp).Length() < 0.01f) result.Add(temp);
            temp = pos + Vector2i.Down;
            if (checkBound(temp) && !set.Contains(temp)) if (getDirection(TrackMap, temp).Length() < 0.01f) result.Add(temp);
            return result;
        }

        private List<Vector2i> checkUpFlow(Vector2i pos, ref List<Vector2i> set)
        {
            List<Vector2i> result = new List<Vector2i>();
            Vector2i temp = Vector2i.Zero;
            temp = pos + Vector2i.Left;
            if (!set.Contains(temp) && set.Contains(temp + getDirectionInt(TrackMap, temp))) result.Add(temp);
            temp = pos + Vector2i.Right;
            if (!set.Contains(temp) && set.Contains(temp + getDirectionInt(TrackMap, temp))) result.Add(temp);
            temp = pos + Vector2i.Up;
            if (!set.Contains(temp) && set.Contains(temp + getDirectionInt(TrackMap, temp))) result.Add(temp);
            temp = pos + Vector2i.Down;
            if (!set.Contains(temp) && set.Contains(temp + getDirectionInt(TrackMap, temp))) result.Add(temp);
            return result;
        }
    }

}

