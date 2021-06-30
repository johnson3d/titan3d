using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure.Buffer2D
{
    public class ImageOp
    {
        public Image mResultImage;
        public Image GetResultImage(Image value)
        {
            if (mResultImage == null || mResultImage.Width != value.Width || mResultImage.Height != value.Height)
            {
                mResultImage = value.Clone();
            }
            return mResultImage;
        }
    }
    public class Monocular : ImageOp
    {
        public virtual void Process(Image left)
        {
            
        }
    }
    public class Binocular : ImageOp
    {
        public virtual void Process(Image left, Image right)
        {
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
        public override void Process(Image left)
        {
            var result = GetResultImage(left);
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    result.Pixels[i, j] = mPerlin.mCoreObject.Get(j, i);
                }
            }
        }
    }
    public class PixelAdd : Binocular
    {
        public override void Process(Image left, Image right)
        {
            if (left.Width != right.Width || left.Height != right.Height)
                return;

            var result = GetResultImage(left);
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    result.Pixels[i, j] = left.Pixels[i, j] + right.Pixels[i, j];
                }
            }
        }
    }
}
