using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngienNS.Bricks.ImageDecoder
{
    public class UStbImageUtility
    {
        public static StbImageWriteSharp.ColorComponents ConvertColorComponent(StbImageSharp.ColorComponents src)
        {
            switch (src)
            {
                case StbImageSharp.ColorComponents.Grey:
                    return StbImageWriteSharp.ColorComponents.Grey;
                case StbImageSharp.ColorComponents.GreyAlpha:
                    return StbImageWriteSharp.ColorComponents.GreyAlpha;
                case StbImageSharp.ColorComponents.RedGreenBlue:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlue;
                case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
                case StbImageSharp.ColorComponents.Default:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
                default:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
            }

            //return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
        }
    }
}
