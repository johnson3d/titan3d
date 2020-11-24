using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace CSharpCode
{
    public class UVAnim
    {
        public Vector2 Size { get; set; } = new Vector2(50, 50);
        public UInt32 Color { get; set; } = 0xFFFFFFFF;
        public void OnDraw(ref ImDrawList_PtrType cmdlist, ref Vector2 rectMin, ref Vector2 rectMax)
        {
            cmdlist.AddRectFilled(ref rectMin, ref rectMax, Color, 0, ImDrawCornerFlags_.ImDrawCornerFlags_All);
        }
    }
}
