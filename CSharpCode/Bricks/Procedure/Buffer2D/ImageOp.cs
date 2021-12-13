using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure.Buffer2D
{
    public class ImageOp
    {
        public Image mResultImage;
    }
    public class ImageLoader : ImageOp
    {
        public virtual bool Process(RName rn, Image.EImageComponent comps)
        {
            using (var stream = System.IO.File.OpenRead(rn.Address))
            {
                var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                if (image != null)
                {

                }
            }   
            return true;
        }
    }
    public class Monocular : ImageOp
    {
        public virtual bool Process(Image left, Image.EImageComponent comps)
        {
            return true;   
        }
    }
    public class Binocular : ImageOp
    {
        public virtual bool Process(Image left, Image right, Image.EImageComponent comps)
        {
            return true;
        }
    }

    public class SmoothGaussion : Monocular
    {
        static float[,] BlurMatrix = 
        {
            { 0.0947416f, 0.118318f, 0.0947416f },
            { 0.118318f, 0.147761f, 0.118318f },
            { 0.0947416f, 0.118318f, 0.0947416f }
        };
        public override bool Process(Image left, Image.EImageComponent comps)
        {
            mResultImage = new Image();
            mResultImage.Initialize(left.Width, left.Height, 0, comps);
            float[,] pixels = new float[3, 3];
            for (int i = 0; i < mResultImage.Height; i++)
            {
                for (int j = 0; j < mResultImage.Width; j++)
                {
                    if ((comps & Image.EImageComponent.X) != 0)
                    {
                        var curComp = left.CompX;

                        var center = curComp.GetPixel(j, i);
                        pixels[1, 1] = center;

                        var v = curComp.GetPixel(j - 1, i - 1);
                        {//line1
                            if (v != float.MaxValue)
                                pixels[0, 0] = v;
                            else
                                pixels[0, 0] = center;

                            v = curComp.GetPixel(j, i - 1);
                            if (v != float.MaxValue)
                                pixels[0, 1] = v;
                            else
                                pixels[0, 1] = center;

                            v = curComp.GetPixel(j + 1, i - 1);
                            if (v != float.MaxValue)
                                pixels[0, 2] = v;
                            else
                                pixels[0, 2] = center;
                        }

                        {//line2
                            v = curComp.GetPixel(j - 1, i);
                            if (v != float.MaxValue)
                                pixels[1, 0] = v;
                            else
                                pixels[1, 0] = center;

                            v = curComp.GetPixel(j + 1, i);
                            if (v != float.MaxValue)
                                pixels[1, 2] = v;
                            else
                                pixels[1, 2] = center;
                        }

                        {//line3
                            v = curComp.GetPixel(j - 1, i + 1);
                            if (v != float.MaxValue)
                                pixels[2, 0] = v;
                            else
                                pixels[2, 0] = center;

                            v = curComp.GetPixel(j, i + 1);
                            if (v != float.MaxValue)
                                pixels[2, 1] = v;
                            else
                                pixels[2, 1] = center;

                            v = curComp.GetPixel(j + 1, i + 1);
                            if (v != float.MaxValue)
                                pixels[2, 2] = v;
                            else
                                pixels[2, 2] = center;
                        }

                        float value = pixels[0, 0] * BlurMatrix[0, 0] + pixels[0, 1] * BlurMatrix[0, 1] + pixels[0, 2] * BlurMatrix[0, 2]
                            + pixels[1, 0] * BlurMatrix[1, 0] + pixels[1, 1] * BlurMatrix[1, 1] + pixels[1, 2] * BlurMatrix[1, 2]
                            + pixels[2, 0] * BlurMatrix[2, 0] + pixels[2, 1] * BlurMatrix[2, 1] + pixels[2, 2] * BlurMatrix[2, 2];
                        mResultImage.CompX.SetPixel(i, j, value);
                    }
                }
            }
            return true;
        }
    }
    public class NoisePerlin : Monocular
    {
        int mOctaves = 3;
        public int Octaves 
        {
            get => mOctaves;
            set
            {
                mOctaves = value;
                UpdatePerlin();
            }
        }
        float mFreq = 3.5f;
        public float Freq 
        {
            get => mFreq;
            set
            {
                mFreq = value;
                UpdatePerlin();
            }
        }
        float mAmptitude = 10.0f;
        public float Amptitude 
        {
            get => mAmptitude;
            set
            {
                mAmptitude = value;
                UpdatePerlin();
            }
        }
        int mSeed = (int)Support.Time.GetTickCount();
        public int Seed 
        { 
            get => mSeed;
            set
            {
                mSeed = value;
                UpdatePerlin();
            }
        }
        protected Support.CPerlin mPerlin;
        protected void UpdatePerlin()
        {
            mPerlin = new Support.CPerlin(Octaves, Freq, Amptitude, Seed);
        }
        public DVector3 StartPosition = DVector3.Zero;
        public override bool Process(Image left, Image.EImageComponent comps)
        {
            mResultImage = new Image();
            mResultImage.Initialize(left.Width, left.Height, 0, Image.EImageComponent.X);
            for (int i = 0; i < left.Height; i++)
            {
                for (int j = 0; j < left.Width; j++)
                {
                    var value = (float)mPerlin.mCoreObject.Get(StartPosition.X + j, StartPosition.Z + i);
                    if (mResultImage.CompX != null && (comps & Image.EImageComponent.X) != 0)
                        mResultImage.CompX.SetPixel(i, j, value);
                }
            }
            return true;
        }
    }
    public class PixelAdd : Binocular
    {
        public override bool Process(Image left, Image right, Image.EImageComponent comps)
        {
            if (left.Width != right.Width || left.Height != right.Height)
                return false;

            var result = left.Clone();
            mResultImage = result;
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (left.CompX != null && right.CompX != null && (comps & Image.EImageComponent.X) != 0)
                        result.CompX.SetPixel(i, j, left.CompX.GetPixel(i, j) + right.CompX.GetPixel(i, j));
                    if (left.CompY != null && right.CompY != null && (comps & Image.EImageComponent.Y) != 0)
                        result.CompY.SetPixel(i, j, left.CompY.GetPixel(i, j) + right.CompX.GetPixel(i, j));
                    if (left.CompZ != null && right.CompZ != null && (comps & Image.EImageComponent.Z) != 0)
                        result.CompZ.SetPixel(i, j, left.CompZ.GetPixel(i, j) + right.CompZ.GetPixel(i, j));
                    if (left.CompW != null && right.CompW != null && (comps & Image.EImageComponent.W) != 0)
                        result.CompW.SetPixel(i, j, left.CompW.GetPixel(i, j) + right.CompW.GetPixel(i, j));
                }
            }
            return true;
        }
    }
    /*
     0 1 2
     3 c 4
     5 6 7
     */
    public class CalcNormal : Monocular
    {
        public float GridSize;
        public float HeightRange;
        public override bool Process(Image left, Image.EImageComponent comps)
        {
            var heightFiels = left.GetComponent(comps);
            if (heightFiels == null)
                return false;

            mResultImage = new Image();
            mResultImage.Initialize(left.Width, left.Height, 0, Image.EImageComponent.X | Image.EImageComponent.Y | Image.EImageComponent.Z);
            float minHeight, maxHeight;
            heightFiels.GetRange(out minHeight, out maxHeight);
            float range = HeightRange;// maxHeight - minHeight;
            for (int i = 1; i < heightFiels.Height - 1; i++)
            {
                for (int j = 1; j < heightFiels.Width - 1; j++)
                {
                    //float h1 = heightFiels.GetPixel(j, i);
                    //float h2 = heightFiels.GetPixel(j + 1, i);
                    //float h3 = heightFiels.GetPixel(j, i + 1);
                    //float h4 = heightFiels.GetPixel(j + 1, i + 1);

                    //var V1 = Vector3.Normalize(new Vector3(1.0f, 0.0f, h2 - h1));//正方形边
                    //var V2 = Vector3.Normalize(new Vector3(1.0f, 0.0f, h4 - h3));//正方形边
                    //var V3 = Vector3.Normalize(new Vector3(1.0f, -1.0f, h4 - h1));//斜边
                    //var V4 = Vector3.Normalize(new Vector3(-1.0f, -1.0f, h3 - h2));//斜边

                    //var Normal1 = Vector3.Cross(in V4, in V1);
                    //var Normal2 = Vector3.Cross(in V3, in V1);
                    //var Normal3 = Vector3.Cross(in V3, in V2);
                    //var Normal4 = Vector3.Cross(in V4, in V2);

                    //var n = Vector3.Normalize(Normal1 + Normal2 + Normal3 + Normal4);

                    //{
                    //    var hc = (heightFiels.GetPixel(j, i) - minHeight);
                    //    var h0 = (heightFiels.GetPixel(j - 1, i + 1) - minHeight);
                    //    var h1 = (heightFiels.GetPixel(j, i + 1) - minHeight);
                    //    var h2 = (heightFiels.GetPixel(j + 1, i + 1) - minHeight);
                    //    var h3 = (heightFiels.GetPixel(j - 1, i) - minHeight);
                    //    var h4 = (heightFiels.GetPixel(j + 1, i) - minHeight);
                    //    var h5 = (heightFiels.GetPixel(j - 1, i - 1) - minHeight);
                    //    var h6 = (heightFiels.GetPixel(j, i - 1) - minHeight);
                    //    var h7 = (heightFiels.GetPixel(j + 1, i - 1) - minHeight);

                    //    hc /= range;
                    //    h0 /= range;
                    //    h1 /= range;
                    //    h2 /= range;
                    //    h3 /= range;
                    //    h4 /= range;
                    //    h5 /= range;
                    //    h6 /= range;
                    //    h7 /= range;

                    //    var vc = new Vector3(0, hc, 0);
                    //    var v0 = new Vector3(-1, h0, 1);
                    //    var v1 = new Vector3(0, h1, 1);
                    //    var v2 = new Vector3(1, h2, 1);
                    //    var v3 = new Vector3(-1, h3, 0);
                    //    var v4 = new Vector3(1, h4, 0);
                    //    var v5 = new Vector3(-1, h5, -1);
                    //    var v6 = new Vector3(0, h6, -1);
                    //    var v7 = new Vector3(1, h7, -1);

                    //    var n1 = Vector3.Cross(vc - v1, v0 - v1);
                    //    var n2 = Vector3.Cross(v2 - v1, vc - v1);
                    //    var n3 = Vector3.Cross(vc - v4, v2 - v4);
                    //    var n4 = Vector3.Cross(v7 - v4, vc - v4);
                    //    var n5 = Vector3.Cross(vc - v6, v7 - v6);
                    //    var n6 = Vector3.Cross(v5 - v6, vc - v6);
                    //    var n7 = Vector3.Cross(vc - v3, v5 - v3);
                    //    var n8 = Vector3.Cross(v0 - v3, vc - v3);

                    //    n1.Normalize();
                    //    n2.Normalize();
                    //    n3.Normalize();
                    //    n4.Normalize();
                    //    n5.Normalize();
                    //    n6.Normalize();
                    //    n7.Normalize();
                    //    n8.Normalize();

                    //    var n = n1 + n2 + n3 + n4 + n6 + n6 + n7 + n8;
                    //    n /= 8.0f;
                    //    n.Normalize();
                    //}

                    float altInfo = heightFiels.GetPixel(j, i);
                    float v_du = heightFiels.GetPixel(j + 1, i);
                    float v_dv = heightFiels.GetPixel(j, i + 1);

                    var A = new Vector3(GridSize, (v_du - altInfo), 0);
                    var B = new Vector3(0, (v_dv - altInfo), -GridSize);

                    var n = Vector3.Cross(A, B);
                    n = Vector3.Normalize(n);

                    mResultImage.CompX.SetPixel(i, j, n.X);
                    mResultImage.CompY.SetPixel(i, j, n.Y);
                    mResultImage.CompZ.SetPixel(i, j, n.Z);
                }
            }

            return true;
        }
    }

    public class UTerrainGen : Monocular
    {
        public float Amptitude1 = 100.0f;
        public float Freq1 = 0.002f;
        public int Octaves1 = 3;
        public float Amptitude2 = 1.5f;
        public float Freq2 = 0.8f;
        public int Octaves2 = 3;
        public int SmoothNum = 3;
        public float GridStep = 1.0f;
        public Bricks.Procedure.Buffer2D.NoisePerlin Perlin1 = new Bricks.Procedure.Buffer2D.NoisePerlin();
        public Bricks.Procedure.Buffer2D.NoisePerlin Perlin2 = new Bricks.Procedure.Buffer2D.NoisePerlin();
        public void SetStartPosition(in DVector3 pos)
        {
            Perlin1.StartPosition = pos;
            Perlin2.StartPosition = pos;
        }

        public Image mResultNormalImage;
        public void InitPerlin()
        {
            Perlin1.Amptitude = Amptitude1;
            Perlin1.Freq = Freq1;
            Perlin1.Octaves = Octaves1;

            Perlin2.Amptitude = Amptitude2;
            Perlin2.Freq = Freq2;
            Perlin2.Octaves = Octaves2;
        }
        public override bool Process(Image left, Image.EImageComponent comps)
        {
            var flatImage = new Image();
            flatImage.Initialize(left.Width, left.Height, 0, Image.EImageComponent.X);

            Perlin1.Process(flatImage, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);
            Perlin2.Process(flatImage, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);

            var addOp = new Bricks.Procedure.Buffer2D.PixelAdd();
            addOp.Process(Perlin1.mResultImage, Perlin2.mResultImage, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);

            Bricks.Procedure.Buffer2D.Image curImg = addOp.mResultImage;
            for (int i = 0; i < SmoothNum; i++)
            {
                var tmp = new Bricks.Procedure.Buffer2D.SmoothGaussion();
                tmp.Process(curImg, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);
                curImg = tmp.mResultImage;
            }

            var finalAddOp = new Bricks.Procedure.Buffer2D.PixelAdd();
            finalAddOp.Process(curImg, left, Image.EImageComponent.X);

            mResultImage = finalAddOp.mResultImage;

            var calcNorm = new Bricks.Procedure.Buffer2D.CalcNormal();
            //calcNorm.HeightRange = (y1 + y2) * 2.0f;
            calcNorm.GridSize = GridStep;
            calcNorm.Process(mResultImage, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);
            mResultNormalImage = calcNorm.mResultImage;
            return true;
        }
    }
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    public class UTest_Procedure
    {
        public static void GenarateTerrain(int size, float y1, float freq1, float y2, float freq2, int smooth, float gridStep, out Bricks.Procedure.Buffer2D.Image heightMap, out Bricks.Procedure.Buffer2D.Image normalMap)
        {
            var pcdTerrain = new Bricks.Procedure.Buffer2D.UTerrainGen();
            pcdTerrain.SmoothNum = smooth;
            pcdTerrain.Amptitude1 = y1;
            pcdTerrain.Freq1 = freq1;
            pcdTerrain.Amptitude2 = y2;
            pcdTerrain.Freq2 = freq2;
            pcdTerrain.GridStep = gridStep;
            pcdTerrain.InitPerlin();

            var oriImage = new Bricks.Procedure.Buffer2D.Image();
            oriImage.Initialize(size, size, 0, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);
            pcdTerrain.Process(oriImage, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);

            heightMap = pcdTerrain.mResultImage;
            normalMap = pcdTerrain.mResultNormalImage;

            //var oriImage = new Bricks.Procedure.Buffer2D.Image();
            //oriImage.Initialize(size, size, 0, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);
            //var perlin = new Bricks.Procedure.Buffer2D.NoisePerlin();
            //perlin.Amptitude = y1;
            //perlin.Freq = freq1;
            //perlin.Octaves = 3;
            //perlin.Process(oriImage, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);

            //var perlin2 = new Bricks.Procedure.Buffer2D.NoisePerlin();
            //perlin2.Amptitude = y2;
            //perlin2.Freq = freq2;
            //perlin2.Octaves = 3;
            //perlin2.Process(oriImage, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);

            //var addOp = new Bricks.Procedure.Buffer2D.PixelAdd();
            //addOp.Process(perlin.mResultImage, perlin2.mResultImage, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);

            //Bricks.Procedure.Buffer2D.Image curImg = addOp.mResultImage;
            //for (int i = 0; i < smooth; i++)
            //{
            //    var tmp = new Bricks.Procedure.Buffer2D.SmoothGaussion();
            //    tmp.Process(curImg, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);
            //    curImg = tmp.mResultImage;
            //}

            //var calcNorm = new Bricks.Procedure.Buffer2D.CalcNormal();
            ////calcNorm.HeightRange = (y1 + y2) * 2.0f;
            //calcNorm.GridSize = gridStep;
            //calcNorm.Process(curImg, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);

            //heightMap = curImg;
            //normalMap = calcNorm.mResultImage;
        }
        public void UnitTestEntrance()
        {

        }
    }
}