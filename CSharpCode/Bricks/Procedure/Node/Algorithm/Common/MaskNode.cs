namespace EngineNS.Bricks.Procedure.Algorithm.Common
{
    public class MaskNode
    {
        public static int[] ToMask(float[] values, float threshold = 0.1f, bool reverse = false)
        {
            int count = values.Length;
            int[] mask = new int[count];
            if (!reverse)
            for (int i = 0; i < count; i++) mask[i] = values[i] > threshold ? 1 : 0;
            else
            for (int i = 0; i < count; i++) mask[i] = values[i] > threshold ? 0 : 1;
            return mask;
        }

        public static float[] FromMask(int[] mask)
        {
            int count = mask.Length;
            float[] values = new float[count];
            for (int i = 0; i < count; i++) values[i] = mask[i];
            return values;
        }

        public static int[] UnionMask(int[] maskA, int[] maskB)
        {
            int count = maskA.Length;
            int[] union = new int[count];
            for (int i = 0; i < count; i++) union[i] = (maskA[i] > 0 && maskB[i] > 0) ? 1 : 0; 
            return union;
        }

        public static int[] SubMask(int[] maskA, int[] maskB)
        {
            int count = maskA.Length;
            int[] subSet = new int[count];
            for (int i = 0; i < count; i++) subSet[i] = (maskA[i] > 0 && maskB[i] <= 0) ? 1 : 0;
            return subSet;
        }

        public static int[] RevMask(int[] mask)
        {
            int count = mask.Length;
            int[] reverse = new int[count];
            for (int i = 0; i < count; i++) reverse[i] = mask[i] > 0 ? 0 : 1;
            return reverse;
        }

        public static int[] EmptyMask(int width)
        {
            int[] empty = new int[width * width];
            return empty;
        }
        


    }
}