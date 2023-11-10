using System;
using System.Collections.Generic;
using System.Text;

using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;
using StbImageSharp;
using System.Diagnostics;

namespace EngineNS
{
    public struct PSBR
    {
        public static unsafe Vector3 GetMSERGB(StbImageSharp.ImageResult Img1, StbImageSharp.ImageResult Img2)
        {
            Debug.Assert(Img1.Width == Img2.Width && Img1.Height == Img2.Height && Img1.Comp == StbImageSharp.ColorComponents.RedGreenBlue);


            float ER = 0;
            float EG = 0;
            float EB = 0;

            int PixelSize = 3;
            int LineSize = 3 * Img1.Width;
            LineSize = (int)MathHelper.Roundup((uint)LineSize, 4);


            fixed (byte* p1 = &Img1.Data[0])
            {
                fixed (byte* p2 = &Img2.Data[0])
                {
                    for (int i = 0; i < Img1.Width; i++)
                    {
                        for (int j = 0; j < Img1.Width; j++)
                        {
                            var rgbValue1 = new Vector3();
                            float v = (float)p1[LineSize * i + j * PixelSize] / 255.0f;
                            rgbValue1.X = v;

                            v = (float)p1[LineSize * i + j * PixelSize + 1] / 255.0f;
                            rgbValue1.Y = v;

                            v = (float)p1[LineSize * i + j * PixelSize + 2] / 255.0f;
                            rgbValue1.Z = v;

                            var rgbValue2 = new Vector3();
                            v = (float)p2[LineSize * i + j * PixelSize] / 255.0f;
                            rgbValue2.X = v;

                            v = (float)p2[LineSize * i + j * PixelSize + 1] / 255.0f;
                            rgbValue2.Y = v;

                            v = (float)p2[LineSize * i + j * PixelSize + 2] / 256.0f;
                            rgbValue2.Z = v;

                            ER = ER + (rgbValue1.X - rgbValue2.X) * (rgbValue1.X - rgbValue2.X);
                            EG = EG + (rgbValue1.Y - rgbValue2.Y) * (rgbValue1.Y - rgbValue2.Y);
                            EB = EB + (rgbValue1.Z - rgbValue2.Z) * (rgbValue1.Z - rgbValue2.Z);
                        }
                    }
                }
            }

            float wh = (float)(Img1.Width * Img1.Height);
            ER = ER / wh;
            EG = EG / wh;
            EB = EB / wh;

            return new Vector3(ER, EG, EB);
        }

        public static Vector3 GetMSEValue(Vector3[] Value1, Vector3[] Value2, out float MaxValue)
        {
            Debug.Assert(Value1.Length == Value2.Length);

            MaxValue = 0.0f;
            float EX = 0;
            float EY = 0;
            float EZ = 0;


            for (int i = 0; i < Value1.Length; i++)
            {
               
                var V1 = Value1[i];
                var V2 = Value2[i];


                EX = EX + (V1.X - V2.X) * (V1.X - V2.X);
                EY = EY + (V1.Y - V2.Y) * (V1.Y - V2.Y);
                EZ = EZ + (V1.Z - V2.Z) * (V1.Z - V2.Z);

                MaxValue = Math.Max(MaxValue, V1.X);
                MaxValue = Math.Max(MaxValue, V1.Y);
                MaxValue = Math.Max(MaxValue, V1.Z);

                MaxValue = Math.Max(MaxValue, V2.X);
                MaxValue = Math.Max(MaxValue, V2.Y);
                MaxValue = Math.Max(MaxValue, V2.Z);

            }

            float lenh = (float)Value1.Length * 0.5f;
            float wh = lenh * lenh;// TODO
            EX = EX / wh;
            EY = EY / wh;
            EZ = EZ / wh;

            return new Vector3(EX, EY, EZ);
        }

        public static float GetImagePSNR(StbImageSharp.ImageResult Img1, StbImageSharp.ImageResult Img2)
        {
            Vector3 MSERGB = GetMSERGB(Img1, Img2);
            float MSEValue = (MSERGB.X + MSERGB.Y + MSERGB.Z) / 3.0f;
            float Maxv = 1.0f;
            float v = 10.0f * (float)Math.Log10(Maxv / MSEValue);
            return v;
        }

        public static float GetPSNRValue(Vector3[] Value1, Vector3[] Value2)
        {
            float Maxv = 0.0f;
            Vector3 MSERGB = GetMSEValue(Value1, Value2, out Maxv);
            float MSEValue = (MSERGB.X + MSERGB.Y + MSERGB.Z) / 3.0f;
            float v = 10.0f * (float)Math.Log10(Maxv / MSEValue);
            return v;
        }
    }
}
