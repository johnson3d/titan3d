namespace EngineNS.Bricks.Procedure.Algorithm
{
    public class ErosionNode
    {
        private static float init_speed = 1;
        private static float init_water = 1;
        private static int max_life = 30; 
        private static float inertia = 0.05f;
        private static float sediment_capacity_factor = 4;
        private static float min_sediment_capacity = 0.01f;
        private static float deposit_speed = 0.3f, erode_speed = 0.4f, evaporate_speed = 0.01f;
        private static float gravity = 4;
        private static void calculate_height(
            float[] map, 
            int width, 
            float x, 
            float y, 
            out float height)
        {
            int i = (int) x,
                j = (int) y;
            int index = i + j * width;
            float u = x - i;
            float v = y - j;
            float h1 = map[index];
            float h2 = map[index + 1];
            float h3 = map[index + width];
            float h4 = map[index + width + 1];
            height = h1 * (1 - u) * (1 - v) + h2 * u * (1 - v) + h3 * (1 - u) * v + h4 * u * v;
        }
        private static void calculate_height_and_gradient(
            float[] map, 
            int width, 
            int index,
            float u, 
            float v, 
            out float height, 
            out float grad_x, 
            out float grad_y)
        {
            float h1 = map[index];
            float h2 = map[index + 1];
            float h3 = map[index + width];
            float h4 = map[index + width + 1];
            // gradient
            grad_x = (h2 - h1) * (1 - v) + (h4 - h3) * v;
            grad_y = (h3 - h1) * (1 - u) + (h4 - h2) * u;
            // height
            height = h1 * (1 - u) * (1 - v) + h2 * u * (1 - v) + h3 * (1 - u) * v + h4 * u * v;
        }
        /// <summary>
        /// Hydraulic erosion simulation
        /// </summary>
        /// <param name="map">高度图数组</param>
        /// <param name="width">高度图边长</param>
        /// <param name="iterations">模拟次数</param>
        public static void erode(float[] map, int width, int iterations)
        {
            while (iterations --> 0)
            {
                float pos_x = MathHelper.RandomRange(0, width - 1);
                float pos_y = MathHelper.RandomRange(0, width - 1);
                float dir_x = 0, dir_y = 0;
                float speed = init_speed;
                float water = init_water;
                float sediment = 0;
                for (int life = 0; life < max_life; life++)
                {
                    int grid_x = (int) pos_x;
                    int grid_y = (int) pos_y;
                    int grid_index = grid_x + grid_y * width;
                    float offset_x = pos_x - grid_x;
                    float offset_y = pos_y - grid_y;
                    // Calculate Height and Gradient
                    float height, grad_x, grad_y;
                    calculate_height_and_gradient(map, width, grid_index, offset_x, offset_y, out height, out grad_x, out grad_y);
                    // Update direction and position
                    dir_x = dir_x * inertia - grad_x * (1 - inertia);
                    dir_y = dir_y * inertia - grad_y * (1 - inertia);
                    float len = MathHelper.Sqrt(dir_x * dir_x + dir_y * dir_y);
                    if (len != 0) {dir_x /= len; dir_y /= len;}
                    pos_x += dir_x;
                    pos_y += dir_y;
                    // Stop if not moving, or moving out of map
                    if (dir_x == 0 && dir_y == 0) break;
                    if (pos_x < 0 || pos_x >= width - 1 || pos_y < 0 || pos_y >= width - 1) break;
                    // Calculate Change
                    float new_height, delta_height;
                    calculate_height(map, width, pos_x, pos_y, out new_height);
                    delta_height = new_height - height;
                    // Calculate Sediment : if enough -> deposit, if lack -> erode
                    float sediment_capacity = MathHelper.Max(
                        -delta_height * speed * water * sediment_capacity_factor,
                        min_sediment_capacity
                    );
                    if (sediment > sediment_capacity || delta_height > 0)
                    {
                        float deposit = (delta_height > 0) ? MathHelper.Min(delta_height, sediment) : (sediment - sediment_capacity) * deposit_speed;
                        map[grid_index] += deposit * (1 - offset_x) * (1 - offset_y);
                        map[grid_index + 1] += deposit * offset_x * (1 - offset_y);
                        map[grid_index + width] += deposit * (1 - offset_x) * offset_y;
                        map[grid_index + width + 1] += deposit * offset_x * offset_y;
                        sediment -= deposit;
                    }
                    else 
                    {
                        float erode = MathHelper.Min((sediment_capacity - sediment) * erode_speed, -delta_height);
                        map[grid_index] -= erode * (1 - offset_x) * (1 - offset_y);
                        map[grid_index + 1] -= erode * offset_x * (1 - offset_y);
                        map[grid_index + width] -= erode * (1 - offset_x) * offset_y;
                        map[grid_index + width + 1] -= erode * offset_x * offset_y;
                        sediment += erode;
                    }
                    // Gravity and Evaporation
                    speed = MathHelper.Sqrt(speed * speed + delta_height * gravity);
                    water -= water * evaporate_speed;
                }
            }
        }
    }
}