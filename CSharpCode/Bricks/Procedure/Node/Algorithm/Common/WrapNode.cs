using System.IO;

namespace EngineNS.Bricks.Procedure.Algorithm.Common
{
    public class WrapNode
    {
        public static float[] Unpack(UBufferConponent input)
        {
            int width = input.Width;
            float[] result = new float[width * width];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i + j * width] = input.GetPixel<Color4f>(i, j).Red;
                }
            } 
            return result;
        }

        public static UBufferConponent Pack(float[] values, int width)
        {
            var creator = UBufferCreator.CreateInstance<USuperBuffer<Vector4, FFloat4Operator>>(width, width, 1);
            var output = UBufferConponent.CreateInstance(creator);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    float value = values[i + j * width];
                    output.SetPixel(i, j, new Color4f(value, value, value));
                }
            }
            //output.Apply();
            return output;
        }

        public static UBufferConponent PackMask(int[] values, int width)
        {
            var creator = UBufferCreator.CreateInstance<USuperBuffer<Vector4, FFloat4Operator>>(width, width, 1);
            var output = UBufferConponent.CreateInstance(creator);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    float value = values[i + j * width];
                    output.SetPixel(i, j, new Color4f(value, value, value));
                }
            }
            //output.Apply();
            return output;
        }

        public static void SaveTexture2D(UBufferConponent toSave, string FileName, bool silent = true)
        { 
            
        }
    }
}