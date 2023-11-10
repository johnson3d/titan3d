using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;

namespace EngineNS.Algorithm
{
    public class RamerDouglasPeucker
    {
        public static int ThresholdDistance = 10;
        public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            return (ProjectPointLine(point, lineStart, lineEnd) - point).Length();
        }

        public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 relativePoint = point - lineStart;
            Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
            float distance = Vector3.Dot(relativePoint, lineDirection);
            return lineStart + (lineDirection * distance);
        }

        public static void FindRedundanceData(Vector3[] Points, int StartPointIndex, int EndPointIndex, ref bool[] NeedDeleteTab)
        {
            if (StartPointIndex + 1 >= EndPointIndex)
            {
                return;
            }

            Vector3 StartPoint = Points[StartPointIndex];
            Vector3 EndPoint = Points[EndPointIndex];

            int SelectIndex = -1;
            float MaxDistance = -1;

            for (int i = StartPointIndex + 1; i <=  EndPointIndex - 1; i ++)
            {
                var p = Points[i];
                float dis = DistancePointLine(p, StartPoint, EndPoint);
                if (dis > ThresholdDistance)
                {
                    if (MaxDistance == -1 || dis > MaxDistance)
                    {
                        SelectIndex = i;
                        MaxDistance = dis;
                    }
                }
            }


            if (SelectIndex == -1)
            {
                for (int i = StartPointIndex + 1; i <= EndPointIndex - 1; i++)
                {
                    if (NeedDeleteTab[i])
                    {
                        Debug.Assert(false);
                    }
                    NeedDeleteTab[i] = true;
                }

                return;
            }

            FindRedundanceData(Points, StartPointIndex, SelectIndex, ref NeedDeleteTab);
            FindRedundanceData(Points, SelectIndex + 1, EndPointIndex, ref NeedDeleteTab);
        }

        public static Vector3[] DealCurePoints(Vector3[] Points)
        {

            bool[] NeedDeleteTab = new bool[Points.Length];

            for (int i = 0; i < Points.Length; i++)
            {
                NeedDeleteTab[i] = false;
            }

            if (Points.Length > 2)
            {
                FindRedundanceData(Points, 0, Points.Length - 1, ref NeedDeleteTab);
            }

            List<Vector3> NewDatas = new List<Vector3>();
            for (int i = 0; i < Points.Length; i++)
            {
                if (NeedDeleteTab[i] == false)
                {
                    NewDatas.Add(Points[i]);
                }
            }

            return NewDatas.ToArray();
        }
    }
}
