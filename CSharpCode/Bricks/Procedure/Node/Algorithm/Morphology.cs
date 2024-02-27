using System.Collections;
using System.Collections.Generic;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("Morphology", "Float1\\Morphology", UPgcGraph.PgcEditorKeyword)]
    public class TtMorphology : Node.UAnyTypeMonocular
    {
        [Rtti.Meta]
        public int Step { get; set; } = 1;
        [Rtti.Meta]
        public float LerpValue { get; set; } = 1.0f;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var Input = graph.BufferCache.FindBuffer(SrcPin);
            var Output = graph.BufferCache.FindBuffer(ResultPin);
            if (Input.BufferCreator.ElementType != Rtti.UTypeDescGetter<float>.TypeDesc)
                return false;

            int width = Input.Width;
            int height = Input.Height;
            int count = width * height;
            float minValue = float.MaxValue;
            var prevStep = Output.Clone();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var src = Input.GetPixel<float>(i, j);
                    prevStep.SetPixel(i, j, src);
                    Output.SetPixel(i, j, src);
                    minValue = MathHelper.Min(minValue, src);
                }
            }

            int[] dx = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };

            int loops = Step;
            while (loops-- > 0)
            {
                //Profiler.Log.WriteInfoSimple("iteration");
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        float max = minValue;
                        for (int n = 0; n <= 8; n++)
                        {
                            if (i + dx[n] > -1 && i + dx[n] < width && j + dy[n] > -1 && j + dy[n] < height)
                            {
                                var v = prevStep.GetPixel<float>(i + dx[n], j + dy[n]);
                                max = MathHelper.Max(v, max);
                            }
                        }
                        var fv = MathHelper.Lerp(Output.GetPixel<float>(i, j), max, LerpValue);
                        Output.SetPixel(i, j, fv);
                    }
                }
                if (loops > 0)
                {
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            var src = Output.GetPixel<float>(i, j);
                            prevStep.SetPixel(i, j, src);
                        }
                    }
                }
            }

            return true;
        }
    }
}

