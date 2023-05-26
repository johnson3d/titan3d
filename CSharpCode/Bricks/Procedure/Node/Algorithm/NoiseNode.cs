namespace EngineNS.Bricks.Procedure.Algorithm
{
    public class NoiseNode
    {
        static int floor(float t) {return t > 0 ? (int)t : (int)t - 1;}
        static float fade(float t) { return t * t * t * (t * (t * 6 - 15) + 10); }
        static float lerp(float t, float a, float b) { return a + t * (b - a); }
        

        #region Perlin Noise
        static int[] p = { 
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };
        static int[] x_grad3 = new int[16]{ 1,-1, 1,-1, 1,-1, 1,-1, 0, 0, 0, 0, 1, 0,-1, 0};
        static int[] y_grad3 = new int[16]{ 1, 1,-1,-1, 0, 0, 0, 0, 1,-1, 1,-1, 1,-1, 1,-1};
        static int[] z_grad3 = new int[16]{ 0, 0, 0, 0, 1, 1,-1,-1, 1, 1,-1,-1, 0, 1, 0,-1};
        static float[] x_grad2 = new float[]{1,-1,1,-1,1,-1,0,0}; 
        static float[] y_grad2 = new float[]{1,1,-1,-1,0,0,1,-1}; 

        public static float grad3_old(int hash, float x, float y, float z)
        {
            int h = hash & 15;
            float u = h < 8 ? x : y;
            float vt = ((h == 12) || (h == 14)) ? x : z;
            float v = h < 4 ? y : vt;
            return ((h&1) == 0 ? u : -u) + ((h&2) == 0 ? v : -v);
        }
        public static float grad3(int hash, float x, float y, float z) {
            return  (x_grad3[hash] > 0 ? x : (x_grad3[hash] < 0 ? -x : 0)) +
                    (y_grad3[hash] > 0 ? y : (y_grad3[hash] < 0 ? -y : 0)) +
                    (z_grad3[hash] > 0 ? z : (z_grad3[hash] < 0 ? -z : 0));
        }

        static float grad3_0(int hash, float x, float y) {
            return  (x_grad3[hash] > 0 ? x : x_grad3[hash] < 0 ? -x : 0) +
                    (y_grad3[hash] > 0 ? y : y_grad3[hash] < 0 ? -y : 0);
        }
        // static float grad2_old(int hash, float x, float y)
        // {
        //     int h = hash & 7;
        //     float u = h < 4 ? x : y;
        //     float v = 2.0f * (h < 4 ? y : x);
        //     return ((h&1) == 0 ? u : -u) + ((h&2) == 0 ? v : -v);
        // }
        static float grad2(int hash, float x, float y)
        {
            return  (x_grad2[hash] > 0 ? x : x_grad2[hash] < 0 ? -x : 0) +
                    (y_grad2[hash] > 0 ? y : y_grad2[hash] < 0 ? -y : 0);
        }


        static public float noise_3d(float x, float y, float z) {
            int ox = floor(x),
                oy = floor(y),
                oz = floor(z);
            int X = ox & 255,
                Y = oy & 255,                  
                Z = oz & 255;
            x -= ox;                                
            y -= oy;                                
            z -= oz;
            float   u = fade(x),     
                    v = fade(y),                                
                    w = fade(z);
            int A = p[X  ]+Y, AA = p[A]+Z, AB = p[A+1]+Z,      
                B = p[X+1]+Y, BA = p[B]+Z, BB = p[B+1]+Z;      
            return lerp(w, lerp(v,  lerp(u, grad3(p[AA  ]&15, x  , y  , z   ),  
                                            grad3(p[BA  ]&15, x-1, y  , z   )), 
                                    lerp(u, grad3(p[AB  ]&15, x  , y-1, z   ),  
                                            grad3(p[BB  ]&15, x-1, y-1, z   ))),
                            lerp(v, lerp(u, grad3(p[AA+1]&15, x  , y  , z-1 ),  
                                            grad3(p[BA+1]&15, x-1, y  , z-1 )), 
                                    lerp(u, grad3(p[AB+1]&15, x  , y-1, z-1 ),
                                            grad3(p[BB+1]&15, x-1, y-1, z-1 ))));
        }

        static public float noise_3d0(float x, float y) {
            int ox = floor(x), 
                oy = floor(y);
            int X = ox & 255, 
                Y = oy & 255;                  
            x -= ox; 
            y -= oy;                                
            float   u = fade(x), 
                    v = fade(y);                                
            int A = p[X  ]+Y, AA = p[A], AB = p[A+1],      
                B = p[X+1]+Y, BA = p[B], BB = p[B+1];      
            float   k1 = grad3_0(p[AA]&15, x, y),
                    k2 = grad3_0(p[BA]&15, x-1, y),
                    k3 = grad3_0(p[AB]&15, x, y-1),
                    k4 = grad3_0(p[BB]&15, x-1, y-1);
            return k1 + u * (k2 - k1) + v * (k3 - k1) + u * v * (k1 - k2 - k3 + k4);
        }

        static public float noise_2d(float x, float y) {
            int ox = floor(x), 
                oy = floor(y);
            int X = ox & 255, 
                Y = oy & 255;                  
            x -= ox; 
            y -= oy;                                
            float   u = fade(x), 
                    v = fade(y);                                
            int A = p[X  ]+Y, AA = p[A], AB = p[A+1],      
                B = p[X+1]+Y, BA = p[B], BB = p[B+1];      
            float   k1 = grad2(p[AA]&7, x, y),
                    k2 = grad2(p[BA]&7, x-1, y),
                    k3 = grad2(p[AB]&7, x, y-1),
                    k4 = grad2(p[BB]&7, x-1, y-1);
            return k1 + u * (k2 - k1) + v * (k3 - k1) + u * v * (k1 - k2 - k3 + k4);
        }


        public static float fractal_noise_2d_smooth(float x, float y, float detail, float roughness)
        {
            float fscale = 1.0f;
            float amp = 1.0f;
            float maxamp = 0.0f;
            float sum = 0.0f;
            int octaves = (int) detail;
            for (int i = 0; i <= octaves; i++)
            {
                float t = noise_3d0(fscale * x, fscale * y) * 0.5f + 0.5f;
                sum += t * amp;
                maxamp += amp;
                amp *= roughness;
                fscale *= 2.0f;
            }
            float remain = detail - octaves;
            if (remain != 0.0f)
            {
                float t = noise_3d0(fscale * x, fscale * y);
                float sum2 = sum + t * amp;
                sum /= maxamp;
                sum2 /= maxamp + amp;
                return (1.0f - remain) * sum + remain * sum2;
            }
            else return sum / maxamp;
        }
        public static float fractal_noise_2d(float x, float y, int octaves)
        {
            float fscale = 1.0f;
            float amp = 1.0f;
            float maxamp = 0.0f;
            float sum = 0.0f;
            for (int i = 0; i <= octaves; i++)
            {
                float t = noise_3d0(fscale * x, fscale * y) * 0.5f + 0.5f;
                sum += t * amp;
                maxamp += amp;
                amp /= 2;
                fscale *= 2;
            }
            return sum / maxamp;
        }


        #endregion
    
        #region VoronoiNoise


        // Blender源码标注的Hash噪声算法来源是这个
        /* ***** Jenkins Lookup3 Hash Functions ***** */

        /* Source: http://burtleburtle.net/bob/c/lookup3.c */

        // #define rot(x, k) (((x) << (k)) | ((x) >> (32 - (k))))
        // #define mix(a, b, c) \
        // { \
        //     a -= c; \
        //     a ^= rot(c, 4); \
        //     c += b; \
        //     b -= a; \
        //     b ^= rot(a, 6); \
        //     a += c; \
        //     c -= b; \
        //     c ^= rot(b, 8); \
        //     b += a; \
        //     a -= c; \
        //     a ^= rot(c, 16); \
        //     c += b; \
        //     b -= a; \
        //     b ^= rot(a, 19); \
        //     a += c; \
        //     c -= b; \
        //     c ^= rot(b, 4); \
        //     b += a; \
        // } \
        // ((void)0)
        // #define final(a, b, c) \
        // { \
        //     c ^= b; \
        //     c -= rot(b, 14); \
        //     a ^= c; \
        //     a -= rot(c, 11); \
        //     b ^= a; \
        //     b -= rot(a, 25); \
        //     c ^= b; \
        //     c -= rot(b, 16); \
        //     a ^= c; \
        //     a -= rot(c, 4); \
        //     b ^= a; \
        //     b -= rot(a, 14); \
        //     c ^= b; \
        //     c -= rot(b, 24); \
        // } \
        // ((void)0)
        private static uint rot(uint x, int k) {return (((x) << (k)) | ((x) >> (32 - (k))));}
        private static void mix(ref uint a, ref uint b, ref uint c)
        {
            a -= c; 
            a ^= rot(c, 4); 
            c += b; 
            b -= a; 
            b ^= rot(a, 6); 
            a += c; 
            c -= b; 
            c ^= rot(b, 8); 
            b += a; 
            a -= c; 
            a ^= rot(c, 16); 
            c += b; 
            b -= a; 
            b ^= rot(a, 19); 
            a += c; 
            c -= b; 
            c ^= rot(b, 4); 
            b += a; 
        }
        private static void final(ref uint a, ref uint b, ref uint c)
        {
            c ^= b; 
            c -= rot(b, 14); 
            a ^= c; 
            a -= rot(c, 11); 
            b ^= a; 
            b -= rot(a, 25); 
            c ^= b; 
            c -= rot(b, 16); 
            a ^= c; 
            a -= rot(c, 4); 
            b ^= a; 
            b -= rot(a, 14); 
            c ^= b; 
            c -= rot(b, 24); 
        }
        public static float hash_uint2(uint kx, uint ky)
        {
            uint a, b, c;
            a = b = c = 0xdeadbeef + (2 << 2) + 13;
            a += kx;
            b += ky;
            final(ref a, ref b, ref c);
            return (float)c / (float)0xFFFFFFFFu;
        }

        private static float hash_uint3(uint kx, uint ky, uint kz)
        {
            uint a, b, c;
            a = b = c = 0xdeadbeef + (3 << 2) + 13;
            a += kx;
            b += ky;
            c += kz;
            final(ref a, ref b, ref c);
            return (float)c / (float)0xFFFFFFFFu;
        }

        private static float hash_uint4(uint kx, uint ky, uint kz, uint kw)
        {
            uint a, b, c;
            a = b = c = 0xdeadbeef + (4 << 2) + 13;
            a += kx;
            b += ky;
            c += kz;
            mix(ref a, ref b, ref c);
            a += kw;
            final(ref a, ref b, ref c);
            return (float)c / (float)0xFFFFFFFFu;
        }

        public static float voronoi_2d(float x, float y, float randomness = 1.0f)
        {
                int ox = floor(x),
                    oy = floor(y);
                x -= ox;
                y -= oy;
                float min_dist = 8.0f;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        float px = hash_uint2((uint)(ox + i), (uint)(oy + j)) + i;
                        float py = hash_uint3((uint)(ox + i), (uint)(oy + j), 1) + j;
                        float dx = x - px;
                        float dy = y - py;
                        float dist = dx * dx + dy * dy;
                        if (dist < min_dist) min_dist = dist;
                    }
                }
                return MathHelper.Sqrt(min_dist);
        }

        public static float voronoi_3d(float x, float y, float z, float randomness = 1.0f)
        {
                int ox = floor(x),
                    oy = floor(y),
                    oz = floor(z);
                x -= ox;
                y -= oy;
                z -= oz;
                float min_dist = 12.0f;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int k = -1; k <= 1; k++)
                        {
                            float px = hash_uint3((uint)(ox + i), (uint)(oy + j), (uint)(oz + k)) * randomness + i;
                            float py = hash_uint4((uint)(ox + i), (uint)(oy + j), (uint)(oz + k), 1) * randomness + j;
                            float pz = hash_uint4((uint)(ox + i), (uint)(oy + j), (uint)(oz + k), 2) * randomness + k;
                            float dx = x - px;
                            float dy = y - py;
                            float dz = z - pz;
                            float dist = dx * dx + dy * dy + dz * dz;
                            if (dist < min_dist) min_dist = dist;
                        }
                    }
                }
                return MathHelper.Sqrt(min_dist);
        }
        #endregion
    }

}