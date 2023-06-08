using EngineNS.Bricks.CodeBuilder.MacrossNode;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.GpuDriven
{
    public class TtVisibilityCullingNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Graphics.Pipeline.Common.URenderGraphPin BoundingInOut = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("Bounding");
        public Graphics.Pipeline.Common.URenderGraphPin VisibilityOut = Graphics.Pipeline.Common.URenderGraphPin.CreateOutput("Visibility", false, EPixelFormat.PXF_UNKNOWN);
        public Graphics.Pipeline.Common.URenderGraphPin DrawArgumentOut = Graphics.Pipeline.Common.URenderGraphPin.CreateOutput("DrawArg", false, EPixelFormat.PXF_UNKNOWN);
    }
    //nanite: https://zhuanlan.zhihu.com/p/382687738
    public class TtVisibilityBuffer
    {
    }

    public struct FRasterTriangle_Rect
    {
        public Vector2i A;
        public Vector2i AB;
        public Vector2i AC;
        public Vector2i Min;
        public Vector2i Max;
        public float AB_AB;
        public float AC_AC;
        public float AB_AC;
        public float DenominatorU;
        public float DenominatorV;

        public void Setup(in Vector2i a, in Vector2i B, in Vector2i C, in Vector2i Size)
        {
            A = a;
            AB = B - A;
            AC = C - A;

            Min = Vector2i.Minimize(Vector2i.Minimize(in A, in B), in C);
            Max = Vector2i.Maximize(Vector2i.Maximize(in A, in B), in C);

            Min = Vector2i.Maximize(in Min, in Vector2i.Zero);
            Max = Vector2i.Minimize(in Max, in Size);

            AB_AB = (float)Vector2i.Dot(in AB, in AB);
            AC_AC = (float)Vector2i.Dot(in AC, in AC);
            AB_AC = (float)Vector2i.Dot(in AB, in AC);

            DenominatorU = ((AC_AC) * (AB_AB) - (AB_AC) * (AB_AC));
            DenominatorV = ((AB_AC) * (AB_AC) - (AB_AB) * (AC_AC));
        }
        public Vector2 GetUV(in Vector2i P)
        {
            Vector2i AP = P - A;
            //u = [（AP·AC）(AB·AB)- (AP·AB)(AB·AC)]/[(AC·AC)(AB·AB) - (AC·AB)(AB·AC)]
            //v = [（AP·AC）(AC·AB)- (AP·AB)(AC·AC)]/[(AB·AC)(AC·AB) - (AB·AB)(AC·AC)]
            float AP_AC = Vector2i.Dot(AP, AC);//dp1
            float AP_AB = Vector2i.Dot(AP, AB);//dp2
            //dp3 x 2
            //div
            float u = ((AP_AC) * (AB_AB) - (AP_AB) * (AB_AC)) / DenominatorU;
            float v = ((AP_AC) * (AB_AC) - (AP_AB) * (AC_AC)) / DenominatorV;
            return new Vector2(u, v);
        }
        public void Rasterize(StbImageSharp.ImageResult image, Color clr, ref Vector2i a, ref Vector2i B, ref Vector2i C)
        {
            var Size = new Vector2i(image.Width, image.Height);
            Setup(in a, in B, in C, in Size);
            for (int y = Min.Y; y <= Max.Y; y++)
            {
                for (int x = Min.X; x <= Max.X; x++)
                {
                    var P = new Vector2i(x, y);
                    var uv = GetUV(in P);
                    if (Vector2.Less(in uv, in Vector2.Zero).Any() || Vector2.Great(in uv, in Vector2.One).Any() ||
                        (uv.X + uv.Y > 1))
                    {
                        continue;
                    }
                    //draw pixel
                    image.SetPixel(x, y, clr);
                }
            }
        }
    }


    public struct FRasterTriangle_Scanline
    {
        public void Rasterize(StbImageSharp.ImageResult image, Color clr, ref Vector2i v1, ref Vector2i v2, ref Vector2i v3)
        {
            SortVertices(ref v1, ref v2, ref v3); //v1.Y <= v2.Y <= v3.Y

            if (v2.Y == v3.Y)
            {//Bottom triangle
                DrawBottomFlatTriangle(image, clr, v1, v2, v3);
            }
            else if (v1.Y == v2.Y)
            {//top triangle
                DrawTopFlatTriangle(image, clr, v1, v2, v3);
            }
            else
            {
                //Pappus Law
                var mid = new Vector2i((int)(v1.X + ((float)(v2.Y - v1.Y) / (float)(v3.Y - v1.Y)) * (float)(v3.X - v1.X)),
                                            v2.Y);
                DrawBottomFlatTriangle(image, clr, v1, v2, mid);
                DrawTopFlatTriangle(image, clr, v2, mid, v3);
            }
        }
        private void SortVertices(ref Vector2i v1, ref Vector2i v2, ref Vector2i v3)
        {
            if (v1.Y > v2.Y)
            {
                MathHelper.Swap(ref v1, ref v2);
            }
            if (v1.Y > v3.Y)
            {
                MathHelper.Swap(ref v1, ref v3);
            }
            if (v2.Y > v3.Y)
            {
                MathHelper.Swap(ref v2, ref v3);
            }
        }
        private void DrawTopFlatTriangle(StbImageSharp.ImageResult image, Color clr, in Vector2i v1, in Vector2i v2, in Vector2i v3)
        {
            float slope1 = (float)(v3.X - v1.X) / (float)(v3.Y - v1.Y);
            float slope2 = (float)(v3.X - v2.X) / (float)(v3.Y - v2.Y);

            float startX = v3.X;
            float endX = v3.X;

            for (int y = (int)v3.Y; y >= (int)v1.Y; y--)
            {
                for (int x = (int)startX; x <= (int)endX; x++)
                {
                    image.SetPixel(x, y, clr);
                }
                startX -= slope1;
                endX -= slope2;
            }
        }

        private void DrawBottomFlatTriangle(StbImageSharp.ImageResult image, Color clr, in Vector2i v1, in Vector2i v2, in Vector2i v3)
        {
            float slope1 = (float)(v2.X - v1.X) / (float)(v2.Y - v1.Y);
            float slope2 = (float)(v3.X - v1.X) / (float)(v3.Y - v1.Y);

            float startX = v1.X;
            float endX = v1.X;

            for (int y = (int)v1.Y; y <= (int)v2.Y; y++)
            {
                for (int x = (int)startX; x <= (int)endX; x++)
                {
                    image.SetPixel(x, y, clr);
                }
                startX += slope1;
                endX += slope2;
            }
        }
    }
}

namespace EngineNS.UTest
{
    [UTest]
    public class UTest_TtSoftRaster
    {
        bool IgnorTest = true;
        void TestBarycentric()
        {
            var A = new Vector2(100, 500);
            var B = new Vector2(150, 100);
            var C = new Vector2(700, 600);

            var P = new Vector2(300, 400);
            var uv = Vector2.Barycentric(in A, in B, in C, in P);
            var PP = A * (1.0f - uv.X - uv.Y) + C * uv.X + B * uv.Y;
            if (P == PP)
            {
                return;
            }
        }
        public void UnitTestEntrance()
        {
            TestBarycentric();

            if (IgnorTest)
                return;

            var A = new Vector2i(100, 500);
            var B = new Vector2i(150, 100);
            var C = new Vector2i(700, 600);
            {
                var image = StbImageSharp.ImageResult.CreateImage(1024, 1024, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                image.Clear(Color.Black);
                var tri = new Bricks.GpuDriven.FRasterTriangle_Rect();
                tri.Rasterize(image, Color.Red, ref A, ref B, ref C);
                using (var memStream = new System.IO.FileStream(RName.GetRName("utest/TestSoftRaster.png").Address, System.IO.FileMode.OpenOrCreate))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                }
            }
            {
                var image = StbImageSharp.ImageResult.CreateImage(1024, 1024, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                image.Clear(Color.Black);
                var tri = new Bricks.GpuDriven.FRasterTriangle_Scanline();
                tri.Rasterize(image, Color.Red, ref A, ref B, ref C);
                using (var memStream = new System.IO.FileStream(RName.GetRName("utest/TestSoftRaster2.png").Address, System.IO.FileMode.OpenOrCreate))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                }
            }
        }
    }
}