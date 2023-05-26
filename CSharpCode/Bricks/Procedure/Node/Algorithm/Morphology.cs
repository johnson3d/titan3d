using System.Collections;
using System.Collections.Generic;

namespace EngineNS.Bricks.Procedure.Algorithm
{
    public class Morphology
    {
        public UBufferConponent Input;
        public int iteration = 1;
        public float LerpValue = 0.0f;
        public string SavePath;
        public void DilateGrey()
        {
            int width = Input.Width;
            int count = width * width;
            float[] grey = new float[count];
            float[] result = new float[count];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    grey[i + j * width] = Input.GetPixel<Color4f>(i, j).Red;
                    result[i + j * width] = grey[i + j * width];
                }
            }

            int[] dx = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            int loops = iteration;
            while (loops-- > 0)
            {
                Profiler.Log.WriteInfoSimple("iteration");
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int current = i + j * width;
                        float max = 0;
                        for (int n = 0; n <= 8; n++)
                        {
                            if (i + dx[n] > -1 && i + dx[n] < width && j + dy[n] > -1 && j + dy[n] < width)
                            {
                                int target = i + dx[n] + (j + dy[n]) * width;
                                max = max < grey[target] ? grey[target] : max;
                            }
                        }
                        result[current] = MathHelper.Lerp(grey[current], max, LerpValue);
                    }
                }
                for (int i = 0; i < count; i++) grey[i] = result[i];
            }

            var creator = UBufferCreator.CreateInstance<USuperBuffer<Vector4, FFloat4Operator>>(width, width, 1);
            var Output = UBufferConponent.CreateInstance(creator);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Output.SetPixel(i, j, new Color4f(result[i + j * width], result[i + j * width], result[i + j * width]));
                }
            }
            //Output.Apply();
            //PCGNode.WrapNode.SaveTexture2D(Output, SavePath);
        }
    }
}

