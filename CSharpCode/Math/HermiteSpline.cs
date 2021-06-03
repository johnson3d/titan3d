using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    class HermiteSpline
    {
        public Vector3 a0;
        public Vector3 a1;
        public Vector3 a2;
        public Vector3 a3;

        public HermiteSpline()
        {
        }

        public HermiteSpline(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Set(p1, p2, p3, p4);
        }

        public void Set(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            a0 = p2;
            a1 = (p3 - p1) * 0.5f;
            a2 = (p3 * 4.0f - p2 * 5.0f + p1 * 2.0f - p4) * 0.5f;
            a3 = (p2 * 3.0f - p3 * 3.0f - p1 + p4) * 0.5f;
        }

        public Vector3 GetValue(float t)
        {
            return a0 + a1 * t + a2 * (t * t) + a3 * (t * t * t);
        }
    }
}
