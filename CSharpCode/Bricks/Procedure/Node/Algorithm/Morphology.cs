using System.Collections;
using System.Collections.Generic;

namespace EngineNS.Bricks.Procedure.Node
{
    public class TtMorphology : Node.UAnyTypeMonocular
    {
        public int iteration = 1;
        public float LerpValue = 1.0f;
        public string SavePath;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var Input = graph.BufferCache.FindBuffer(SrcPin);
            var Output = graph.BufferCache.FindBuffer(ResultPin);
            if (Input.BufferCreator.BufferType != Rtti.UTypeDescGetter<float>.TypeDesc)
                return false;

            int width = Input.Width;
            int height = Input.Height;
            int count = width * height;
            var grey = (float*)Input.GetSuperPixelAddress(0, 0, 0); //new float[count];
            var result = (float*)Output.GetSuperPixelAddress(0, 0, 0);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
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
                //Profiler.Log.WriteInfoSimple("iteration");
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int current = i + j * width;
                        float max = 0;
                        for (int n = 0; n <= 8; n++)
                        {
                            if (i + dx[n] > -1 && i + dx[n] < width && j + dy[n] > -1 && j + dy[n] < height)
                            {
                                int target = i + dx[n] + (j + dy[n]) * width;
                                max = max < grey[target] ? grey[target] : max;
                            }
                        }
                        result[current] = MathHelper.Lerp(grey[current], max, LerpValue);
                    }
                }
            }

            return true;
            //Output.Apply();
            //PCGNode.WrapNode.SaveTexture2D(Output, SavePath);
        }
    }
}

