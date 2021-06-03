using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    class Bezier3D
    {
        public Vector3 a0;
        public Vector3 a1;
        public Vector3 a2;
        public Vector3 a3;

        public Bezier3D()
        {
        }

        public Bezier3D(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Set(p1, p2, p3, p4);
        }

        public void Set(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            a0 = p1;
            a1 = p2;
            a2 = p3;
            a3 = p4;
        }

        public Vector3 GetValue(float t)
        {
            return (float)Math.Pow(1-t, 3) * a0 + 3 * a1 * t * (float)Math.Pow((1-t), 2) +
                3 * a2 * (1 - t) * (float)Math.Pow(t, 2) + (float)Math.Pow(t, 3) * a3;
        }
    }
}
