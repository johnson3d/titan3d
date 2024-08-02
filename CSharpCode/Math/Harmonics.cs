using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    //https://blog.csdn.net/tiao_god/article/details/111240808
    public struct ThreeBandSHVector
    {
        public Vector4 V0;
        public Vector4 V1;
        public float V2;

        //public ThreeBandSHVector()
        //{
        //    V0 = new Vector4();
        //    V1 = new Vector4();
        //    V2 = 0f;
        //}

        public static ThreeBandSHVector operator +(in ThreeBandSHVector left, in ThreeBandSHVector right)
        {
            ThreeBandSHVector Result = new ThreeBandSHVector();

            Result.V0 = left.V0 + right.V0;
            Result.V1 = left.V1 + right.V1;
            Result.V2 = left.V2 + right.V2;

            return Result;
        }
    }

    public struct ThreeBandSHVectorRGB
    {
        public ThreeBandSHVector R;
        public ThreeBandSHVector B;
        public ThreeBandSHVector G;

        //public ThreeBandSHVectorRGB() 
        //{
        //    R = new ThreeBandSHVector();
        //    G = new ThreeBandSHVector();
        //    B = new ThreeBandSHVector();
        //}

        public static ThreeBandSHVectorRGB operator +(in ThreeBandSHVectorRGB left, in ThreeBandSHVectorRGB right)
        {
            ThreeBandSHVectorRGB Result = new ThreeBandSHVectorRGB();
            Result.R = left.R + right.R;
            Result.G = left.G + right.G;
            Result.R = left.B + right.B;

            return Result;
        }
    }
    class Harmonics
    {
        public ThreeBandSHVectorRGB Coefs;

        public Harmonics()
        {
            Coefs = new ThreeBandSHVectorRGB();

            // Postion
            //Shadow
            //...
        }

        public ThreeBandSHVector SHBasisFunction3(in Vector3 InputVector) //Direction
        {
            InputVector.Normalize();

            ThreeBandSHVector Result = new ThreeBandSHVector();

            Result.V0.Z = 0.282095f;

            Result.V0.Y = -0.488603f * InputVector.Y;
            Result.V0.Z = 0.488603f * InputVector.Z;
            Result.V0.W = -0.488603f * InputVector.X;

            Vector3 VectorSquared = InputVector * InputVector;
            Result.V1.X = 1.092548f * InputVector.X * InputVector.Y;
            Result.V1.Y = -1.092548f * InputVector.Y * InputVector.Z;
            Result.V1.Z = 0.315392f * (3.0f * VectorSquared.Z - 1.0f);
            //Result.V1.Z = 0.315392f * (-VectorSquared.X - VectorSquared.Y + 2.0f * VectorSquared.Z);
            Result.V1.W = -1.092548f * InputVector.X * InputVector.Z;
            Result.V2 = 0.546274f * (VectorSquared.X - VectorSquared.Y);

            return Result;
        }

        public ThreeBandSHVectorRGB MulSH3(in ThreeBandSHVector SHVector, in Color4b color)
        {
            ThreeBandSHVectorRGB Result = new ThreeBandSHVectorRGB();

            Result.R.V0 = SHVector.V0 * color.R;
            Result.R.V1 = SHVector.V1 * color.R;
            Result.R.V2 = SHVector.V2 * color.R;
            Result.G.V0 = SHVector.V0 * color.G;
            Result.G.V1 = SHVector.V1 * color.G;
            Result.G.V2 = SHVector.V2 * color.G;
            Result.B.V0 = SHVector.V0 * color.B;
            Result.B.V1 = SHVector.V1 * color.B;
            Result.B.V2 = SHVector.V2 * color.B;
            return Result;
        }

        public ThreeBandSHVector CalcDiffuseTransferSH3(in Vector3 Normal, float Exponent)
        {
            ThreeBandSHVector Result = SHBasisFunction3(in Normal);


            float L0 = 2.0f * (float)Math.PI / (1.0f + 1.0f * Exponent);
            float L1 = 2.0f * (float)Math.PI / (2.0f + 1.0f * Exponent);
            float L2 = Exponent * 2.0f * (float)Math.PI / (3.0f + 4.0f * Exponent + Exponent * Exponent);
            float L3 = (Exponent - 1.0f) * 2.0f * (float)Math.PI / (8.0f + 6.0f * Exponent + Exponent * Exponent);


            Result.V0.X = Result.V0.X * L0;
            Result.V0.Y = Result.V0.Y * L1;

            Result.V0.Z = Result.V0.Z * L1;

            Result.V0.W = Result.V0.W * L1;


            Result.V1 = Result.V1 * L2;
            Result.V2 = Result.V2 * L2;

            return Result;
        }
        public float DotSH(in ThreeBandSHVector A, in ThreeBandSHVector B)
        {
            float Result = Vector4.Dot(in A.V0, in B.V0);
            Result = Result + Vector4.Dot(in A.V1, in B.V1);
            Result = Result + A.V2 * B.V2;
            return Result;
        }

        public Vector3 DotSH3(in ThreeBandSHVectorRGB A, in ThreeBandSHVector B) //Return Color
        {
            Vector3 color = new Vector3();
            color.X = DotSH(A.R, B);
            color.Y = DotSH(A.G, B);
            color.Z = DotSH(A.B, B);
            return color;
        }

        public Vector4 UniformSampleSphere(in Vector2 E)//E == UV for test
        {
            float Phi = 2f * (float)Math.PI * E.X;
            float CosTheta = 1.0f - 2.0f * E.Y;
            float SinTheta = (float)Math.Sqrt(1 - CosTheta * CosTheta);

            Vector3 H =new Vector3();
            H.X = SinTheta * (float)Math.Cos(Phi);
            H.Y = SinTheta * (float)Math.Sin(Phi);
            H.Z = CosTheta;

            float PDF = 1.0f / (4f * (float)Math.PI);

            return new Vector4(H.X, H.Y, H.Z, PDF);
        }

        public void GenerateData(in Vector3 Direction, in Color4b PixelColor, float Weight)
        {
            ThreeBandSHVector SHVector = SHBasisFunction3(in Direction);
            ThreeBandSHVectorRGB SHVectorRGB = MulSH3(in SHVector, in PixelColor);
            Coefs = Coefs + SHVectorRGB;
        }

        // Test for IBL(cubemap)
        public void GenerateDatas(Vector3[] Normals)
        {
            float UniformPDF = 1.0f / (2.0f * (float)Math.PI);
            float SampleWeight = 1.0f / (UniformPDF * Normals.Length);
            for (int i = 0; i < Normals.Length; i++)
            {
                GenerateData(Normals[i], Color4b.FromRgb(255, 255, 255), SampleWeight); //Color -> Compute Color 
            }
        }

        public unsafe void GenerateDatas(Vector3* Normals, int count)
        {
            float UniformPDF = 1.0f / (2.0f * (float)Math.PI);
            float SampleWeight = 1.0f / (UniformPDF * count);
            for (int i = 0; i < count; i++)
            {
                GenerateData(Normals[i], Color4b.FromRgb(255, 255, 255), SampleWeight); //Color -> Compute Color 
            }
        }

        public Vector3 GetColor(in Vector3 Direction)
        {
            ThreeBandSHVector SHVector = CalcDiffuseTransferSH3(in Direction, 1);

            Vector3 color = DotSH3(in Coefs, in SHVector);
            return color;
        }

    }
}
