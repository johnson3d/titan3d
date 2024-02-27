using NPOI.Util;

namespace EngineNS.Bricks.Procedure.Node
{
    //https://zhuanlan.zhihu.com/p/434435957
    //https://github.com/bshishov/UnityTerrainErosionGPU?tab=readme-ov-file
    [Bricks.CodeBuilder.ContextMenu("Erosion", "Float1\\Erosion", UPgcGraph.PgcEditorKeyword)]
    public class TtErosionNode : Node.UAnyTypeMonocular
    {
        [Rtti.Meta]
        public int DropNum { get; set; } = 100;
        [Rtti.Meta]
        public float init_speed { get; set; } = 1;
        [Rtti.Meta]
        public float init_water { get; set; } = 1;
        [Rtti.Meta]
        public int max_life { get; set; } = 30;
        [Rtti.Meta]
        public float inertia { get; set; } = 0.05f;
        [Rtti.Meta]
        public float sediment_capacity_factor { get; set; } = 4;
        [Rtti.Meta]
        public float min_sediment_capacity { get; set; } = 0.01f;
        [Rtti.Meta]
        public float deposit_speed { get; set; } = 0.3f;
        [Rtti.Meta]
        public float erode_speed { get; set; } = 0.4f;
        [Rtti.Meta]
        public float evaporate_speed { get; set; } = 0.01f;
        [Rtti.Meta]
        public float gravity { get; set; } = 4;
        int mSeed = (int)Support.Time.GetTickCount();
        [Rtti.Meta]
        public int Seed
        {
            get => mSeed;
            set
            {
                mSeed = value;
            }
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var Input = graph.BufferCache.FindBuffer(SrcPin);
            var Output = graph.BufferCache.FindBuffer(ResultPin);

            var rnd = new Support.URandom(Seed);
            for (int i = 0; i < Input.Width; i++)
            {
                for (int j = 0; j < Input.Height; j++)
                {
                    var src = Input.GetPixel<float>(i, j);
                    Output.SetPixel(i, j, src);
                }
            }

            int step = DropNum;
            while (step-- > 0)
            {
                Vector2 pos = new Vector2();
                pos.X = rnd.GetRange(0, Input.Width - 1);
                pos.Y = rnd.GetRange(0, Input.Height - 1);
                Vector2 dir = Vector2.Zero;
                float speed = init_speed;
                float water = init_water;
                float sediment = 0;
                for (int life = 0; life < max_life; life++)
                {
                    int grid_x = (int) pos.X;
                    int grid_y = (int) pos.Y;
                    Vector2 uv = new Vector2();
                    float offset_x = pos.X - grid_x;
                    float offset_y = pos.Y - grid_y;
                    uv.U = offset_x;
                    uv.V = offset_y;
                    // Calculate Height and Gradient
                    var uvh = Output.GetGradAndHeight(grid_x, grid_y, 0, in uv);
                    Vector2 grad = uvh.XY;
                    float height = uvh.Z;
                    // Update direction and position
                    dir = dir * inertia - grad * (1 - inertia);
                    dir.Normalize();
                    pos += dir;
                    // Stop if not moving, or moving out of map
                    if (dir.X == 0 && dir.Y == 0) 
                        break;
                    if (pos.X < 0 || pos.X >= Input.Width - 1 || pos.Y < 0 || pos.Y >= Input.Height - 1) 
                        break;
                    // Calculate Change
                    float new_height, delta_height;
                    new_height = Output.GetHeight(pos.X, pos.Y);
                    delta_height = new_height - height;
                    // Calculate Sediment : if enough -> deposit, if lack -> erode
                    float sediment_capacity = MathHelper.Max(
                        -delta_height * speed * water * sediment_capacity_factor,
                        min_sediment_capacity
                    );
                    if (sediment > sediment_capacity || delta_height > 0)
                    {
                        float deposit = (delta_height > 0) ? MathHelper.Min(delta_height, sediment) : (sediment - sediment_capacity) * deposit_speed;
                        var t = Output.GetFloat1(grid_x, grid_y, 0);
                        Output.SetFloat1(grid_x, grid_y, 0, t + deposit * (1 - offset_x) * (1 - offset_y));
                        t = Output.GetFloat1(grid_x + 1, grid_y, 0);
                        Output.SetFloat1(grid_x + 1, grid_y, 0, t + deposit * offset_x * (1 - offset_y));
                        t = Output.GetFloat1(grid_x, grid_y + 1, 0);
                        Output.SetFloat1(grid_x, grid_y + 1, 0, t + deposit * (1 - offset_x) * offset_y);
                        t = Output.GetFloat1(grid_x + 1, grid_y + 1, 0);
                        Output.SetFloat1(grid_x + 1, grid_y + 1, 0, t + deposit * offset_x * offset_y);

                        //map[grid_index] += deposit * (1 - offset_x) * (1 - offset_y);
                        //map[grid_index + 1] += deposit * offset_x * (1 - offset_y);
                        //map[grid_index + width] += deposit * (1 - offset_x) * offset_y;
                        //map[grid_index + width + 1] += deposit * offset_x * offset_y;
                        sediment -= deposit;
                    }
                    else 
                    {
                        float erode = MathHelper.Min((sediment_capacity - sediment) * erode_speed, -delta_height);

                        var t = Output.GetFloat1(grid_x, grid_y, 0);
                        Output.SetFloat1(grid_x, grid_y, 0, t - erode * (1 - offset_x) * (1 - offset_y));
                        t = Output.GetFloat1(grid_x + 1, grid_y, 0);
                        Output.SetFloat1(grid_x + 1, grid_y, 0, t - erode * offset_x * (1 - offset_y));
                        t = Output.GetFloat1(grid_x, grid_y + 1, 0);
                        Output.SetFloat1(grid_x, grid_y + 1, 0, t - erode * (1 - offset_x) * offset_y);
                        t = Output.GetFloat1(grid_x + 1, grid_y + 1, 0);
                        Output.SetFloat1(grid_x + 1, grid_y + 1, 0, t - erode * offset_x * offset_y);

                        //map[grid_index] -= erode * (1 - offset_x) * (1 - offset_y);
                        //map[grid_index + 1] -= erode * offset_x * (1 - offset_y);
                        //map[grid_index + width] -= erode * (1 - offset_x) * offset_y;
                        //map[grid_index + width + 1] -= erode * offset_x * offset_y;
                        sediment += erode;
                    }
                    // Gravity and Evaporation
                    if (delta_height < 0)
                    {
                        var sqSpeed = speed * speed - delta_height * gravity;
                        speed = MathHelper.Sqrt(sqSpeed);
                    }
                    water -= water * evaporate_speed;
                }
            }
            return true;
        }
    }
}