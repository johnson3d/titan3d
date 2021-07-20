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
        public const float ScaleEpsilon = 0.000001f;
        public bool HasScale
        {
            get
            {
                return Vector3.Equals(in mScale, in Vector3.One) == false;
            }
        }
        public bool IsIdentity
        {
            get
            {
                if (Vector3.Equals(in mScale, in Vector3.One) == false)
                    return false;
                if (Vector3.Equals(in mPosition, in Vector3.Zero) == false)
                    return false;
                if (Quaternion.Equals(in mQuat, in Quaternion.mIdentity) == false)
                    return false;
                return true;
            }
        }
        public Vector3 mPosition;
        public Vector3 mScale;//为了Hierarchical计算方便，我们设定mScale在Transform中只影响本节点而不传递，如果需要整体放缩，在Node上新增一个ScaleMatrix
        public Quaternion mQuat;

        public Vector3 Position { get => mPosition; set => mPosition = value; }
        public Vector3 Scale { get => mScale; set => mScale = value; }
        public Quaternion Quat { get => mQuat; set => mQuat = value; }
    }
}
