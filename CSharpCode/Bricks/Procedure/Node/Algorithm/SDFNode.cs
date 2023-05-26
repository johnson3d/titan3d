
namespace EngineNS.Bricks.Procedure.Algorithm
{

    public class SDFNode
    {
        private const string SDFPath = "SDF";

        /// <summary>
        /// JFA Method
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static float[] CalculateSDF_Approx(int[] mask, int width)
        {
            return null;

            //int count = width * width;
            //ComputeShader ComputeSDF = Resources.Load(SDFPath) as ComputeShader;
            //ComputeBuffer MaskBuffer = new ComputeBuffer(count, sizeof(int));
            //MaskBuffer.SetData(mask);
            //int InitMaskKernel = ComputeSDF.FindKernel("InitMask");
            //int JumpFloodKernel = ComputeSDF.FindKernel("JumpFlood");
            //int DistanceTransformKernel = ComputeSDF.FindKernel("DT");
            //RenderTexture Source = new RenderTexture(width, width, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            //Source.enableRandomWrite = true;
            //Source.Create();
            //RenderTexture Result = new RenderTexture(width, width, 0,RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            //Result.enableRandomWrite = true;
            //Result.Create();
            //int stepCount = (int) Mathf.Log(width, 2);
            //int groupX = Mathf.CeilToInt(width / 8.0f);
            //// Compute Target
            //{
            //    ComputeSDF.SetInt("Width", width);
            //    ComputeSDF.SetInt("Height", width);
            //    ComputeSDF.SetBuffer(InitMaskKernel, "MaskBuffer", MaskBuffer);
            //    ComputeSDF.SetTexture(InitMaskKernel, "Source", Source);
            //    ComputeSDF.SetTexture(InitMaskKernel, "Result", Result);
            //    ComputeSDF.Dispatch(InitMaskKernel, groupX, groupX, 1);
            //    // SDF
            //    for (int i = 0; i < stepCount; i++)
            //    {
            //        int step = (int) Mathf.Pow(2, stepCount - i - 1);
            //        ComputeSDF.SetInt("Step", step);
            //        ComputeSDF.SetTexture(JumpFloodKernel, "Source", Source);
            //        ComputeSDF.SetTexture(JumpFloodKernel, "Result", Result);
            //        ComputeSDF.Dispatch(JumpFloodKernel, groupX, groupX, 1);
            //        Graphics.Blit(Result, Source);
            //    }
            //    // DT
            //    {
            //        ComputeSDF.SetTexture(DistanceTransformKernel, "Source", Source);
            //        ComputeSDF.SetTexture(DistanceTransformKernel, "Result", Result);
            //        ComputeSDF.Dispatch(DistanceTransformKernel, groupX, groupX, 1);
            //    }
            //}
            //MaskBuffer.Dispose();
            //Source.DiscardContents();
            //Result.DiscardContents();
            //Texture2D tex = new Texture2D(Result.width, Result.width, TextureFormat.RGB24, false);
            //RenderTexture.active = Result;
            //tex.ReadPixels(new Rect(0, 0, Result.width, Result.height), 0, 0);
            //tex.Apply();
            //return PCGNode.WrapNode.Unpack(tex);
        }

        private static int edt_f(int x, int i, int gi)
        {
            return (x - i) * (x - i) + gi * gi;
            // return Mathf.Abs(x - i) + gi;
        }

        private static int edt_sep(int i, int u, int gi, int gu)
        {
            return MathHelper.FloorToInt(
                (u * u - i * i + gu * gu - gi * gi) / (2 * (u - i))
            );
        }
        /// <summary>
        /// Meijster Method
        /// </summary>
        /// <param name="b"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static float[] CalculateSDF_Exact(int[] b, int m, int n, bool exp_value = true)
        {
            int[] g = new int[m * n];
            int inf = m + n;
            // first phase
            for (int x = 0; x < m; x++)
            {
                // scan 1
                if (b[x] > 0) g[x] = 0;
                else g[x] = inf;
                for (int y = 1; y < n; y++)
                {
                    if (b[x + y * m] > 0) g[x + y * m] = 0;
                    else g[x + y * m] = 1 + g[x + (y - 1) * m];
                }
                // scan 2
                for (int y = n - 2; y >= 0; y--)
                {
                    if (g[x + (y + 1) * m] < g[x + y * m]) g[x + y * m] = 1 + g[x + (y + 1) * m]; 
                }
            }

            float[] dt = new float[m * n];
            // second phase
            int[] s = new int[m];
            int[] t = new int[m];
            int q = 0, w;
            for (int y = 0; y < n; y++)
            {
                q = 0;
                s[0] = 0;
                t[0] = 0;
                // scan 3
                for (int u = 1; u < m; u++)
                {
                    while (q >= 0 && edt_f(t[q], s[q], g[s[q] + y * m]) > edt_f(t[q], u, g[u + y * m]))
                    {
                        q--;
                    }
                    if (q < 0)
                    {
                        q = 0;
                        s[0] = u;
                    }
                    else
                    {
                        w = 1 + edt_sep(s[q], u, g[s[q] + y * m], g[u + y * m]);
                        if (w < m)
                        {
                            q++;
                            s[q] = u;
                            t[q] = w;
                        }
                    }
                }
                // scan 4
                for (int u = m - 1; u >= 0; u--)
                {
                    float d2 = edt_f(u, s[q], g[s[q] + y * m]);
                    if (exp_value) dt[u + y * m] = 1 - MathHelper.Exp(-MathHelper.Sqrt(d2) * 0.01f);
                    else dt[u + y * m] = MathHelper.Sqrt(d2);
                    if (u == t[q]) q--;
                }
            }
            return dt;
        }
    }
}