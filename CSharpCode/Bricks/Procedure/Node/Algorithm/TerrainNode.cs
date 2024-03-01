namespace EngineNS.Bricks.Procedure.Algorithm
{
    public class TerrainNode
    {
        #region MPD Algorithm
        public static unsafe UBufferComponent GenerateMPD(int exp, float spread = 0.3f, float spread_decay = 0.5f)
        {
            int size = (1 << exp) + 1;
            int count = size * size;
            var creator = UBufferCreator.CreateInstance<USuperBuffer<Vector4, FFloat4Operator>>(size, size, 1);
            var buffer = UBufferComponent.CreateInstance(creator);
            var result = (float*)buffer.GetSuperPixelAddress(0, 0, 0);
            mpd_init_corner(result, size);
            int chunk = 1;
            for (int i = 0; i < exp; i++)
            {
                int chunk_width = (size - 1) / chunk;
                // do
                for (int xchunk = 0; xchunk < chunk; xchunk++)
                {
                    for (int ychunk = 0; ychunk < chunk; ychunk++)
                    {
                        int lx = chunk_width * xchunk;
                        int rx = lx + chunk_width;
                        int by = chunk_width * ychunk;
                        int ty = by + chunk_width;
                        mpd_displace(result, size, lx, rx, by, ty, spread);
                    }
                }
                chunk <<= 1;
                spread *= spread_decay;
            }
            return buffer;
        }

        private unsafe static void mpd_init_corner(float* map, int size)
        {
            map[0] = MathHelper.RandomDouble();
            map[size - 1] = MathHelper.RandomDouble();
            map[size * size - size] = MathHelper.RandomDouble();
            map[size * size - 1] = MathHelper.RandomDouble();
        }

        private unsafe static void mpd_displace(float* map, int size, int lx, int rx, int by, int ty, float spread)
        {
            int mx = midpoint(lx, rx);
            int my = midpoint(by, ty);
            float bottom_left = map[lx + by * size];
            float bottom_right = map[rx + by * size];
            float top_left = map[lx + ty * size];
            float top_right = map[rx + ty * size];
            float bottom = average2(bottom_left, bottom_right);
            float top = average2(top_left, top_right);
            float left = average2(bottom_left, top_left);
            float right = average2(bottom_right, top_right);
            float center = average4(bottom, top, left, right);
            map[mx + by * size] = jitter(bottom, spread);
            map[mx + ty * size] = jitter(top, spread);
            map[lx + my * size] = jitter(left, spread);
            map[rx + my * size] = jitter(right, spread);
            map[mx + my * size] = jitter(center, spread);
        }

        private static float jitter(float value, float spread) {return value + (MathHelper.RandomDouble() * 2 - 1) * spread;}
        private static float average2(float a, float b) {return (a + b) / 2;}
        private static float average4(float a, float b, float c, float d) {return (a + b + c + d) / 4;}
        private static int midpoint(int a, int b) {return (a + b) / 2;}
        
        #endregion
    }
}