using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngienNS.Bricks.ImageDecoder
{

    public enum UImageType
    {
        PNG = 0,
        HDR,
        EXR,
    }


    public class UImageDecoder
    {
        public EngineNS.IImageDecoder mInner;

        public UImageDecoder(string fileName)
        {
            Initialize(fileName);
        }

        void Initialize(string fileName)
        {
            mInner = EngineNS.IImageDecoder.MatchDecoder(fileName);
        }

        
    }
}
