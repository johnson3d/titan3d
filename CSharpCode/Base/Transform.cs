using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [TypeConverter]
    public struct Transform
    {
        public static Transform Identity;
        static Transform()
        {
            Identity.InitData();
        }
        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                Transform result;
                int cur = 0;
                var posStr = GetMatchPair(text, ref cur, '(', ')');
                if (posStr == null)
                    return false;
                var segs = posStr.Split(',');
                result.mPosition = new Vector3(System.Convert.ToSingle(segs[0]),
                    System.Convert.ToSingle(segs[1]),
                    System.Convert.ToSingle(segs[2]));
                var scaleStr = GetMatchPair(text, ref cur, '(', ')');
                if (scaleStr == null)
                    return false;
                segs = scaleStr.Split(',');
                result.mScale = new Vector3(System.Convert.ToSingle(segs[0]),
                    System.Convert.ToSingle(segs[1]),
                    System.Convert.ToSingle(segs[2]));
                var quatStr = GetMatchPair(text, ref cur, '(', ')');
                if (quatStr == null)
                    return false;
                segs = quatStr.Split(',');
                result.mQuat = new Quaternion(System.Convert.ToSingle(segs[0]),
                    System.Convert.ToSingle(segs[1]),
                    System.Convert.ToSingle(segs[2]),
                    System.Convert.ToSingle(segs[3]));

                obj = result;
                return true;
            }
        }

        public override string ToString()
        {
            return $"[Position({mPosition.ToString()}),Scale({mScale.ToString()}),Quat({mQuat.ToString()})]";
        }
        public void InitData()
        {
            mPosition = new Vector3(0);
            mScale = new Vector3(1);
            mQuat = Quaternion.mIdentity;
        }
        public static Transform CreateTransform(Vector3 pos, Vector3 scale , Quaternion quat)
        {
            Transform tmp;
            tmp.mPosition = pos;
            tmp.mScale = scale;
            tmp.mQuat = quat;
            return tmp;
        }
        public Vector3 mPosition;
        public Vector3 mScale;
        public Quaternion mQuat;

        public Vector3 Position { get => mPosition; set => mPosition = value; }
        public Vector3 Scale { get => mScale; set => mScale = value; }
        public Quaternion Quat { get => mQuat; set => mQuat = value; }
    }
}
