using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct Point3i
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int X;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Y;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int Z;

        public Point3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
