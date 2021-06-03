using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Base
{

    public class AnimatableObjectClassDesc : IAnimatableClassDesc
    {
        public Rtti.UTypeDesc ClassType
        {
            get { return Rtti.UTypeDesc.TypeOf(TypeStr.ToString()); }
        }
        private VNameString mTypeStr;
        public VNameString TypeStr { get => mTypeStr; set => mTypeStr = value; }

        private VNameString mName;
        public VNameString Name { get => mName; set => mName = value; }

        public List<IAnimatablePropertyDesc> Properties { get; set; }

        public AnimatableObjectClassDesc()
        {
            unsafe
            {
                mTypeStr = *VNameString.CreateInstance();
                mName = *VNameString.CreateInstance();
            }
        }
        public bool LoadXnd(CXndHolder xndHolder, XndNode parentNode)
        {
            using (var att = parentNode.TryGetAttribute("ClassTypeStr"))
            {
                if (!att.IsValidPointer)
                {
                    return false;
                }
                att.BeginRead();
                att.Read(ref mTypeStr);
                att.EndRead();
            }
            using (var att = parentNode.TryGetAttribute("Name"))
            {
                if (!att.IsValidPointer)
                {
                    return false;
                }
                att.BeginRead();
                att.Read(ref mName);
                att.EndRead();
            }
            uint propertiesCount = parentNode.GetNumOfNode();
            for (uint i = 0; i < propertiesCount; ++i)
            {
                var node = parentNode.GetNode(i);
                AnimatableObjectPropertyDesc desc = new AnimatableObjectPropertyDesc();
                desc.LoadXnd(xndHolder ,node);
                Properties.Add(desc);
            }
            return true;
        }

        public bool SaveXnd(IO.CXndHolder xndHolder, XndNode parentNode)
        {
            unsafe
            {
                using (var att = xndHolder.NewAttribute("ClassTypeStr", 1, 0))
                {
                    att.BeginWrite(10);
                    att.Write(mTypeStr);
                    att.EndWrite();
                    parentNode.AddAttribute(att.CppPointer);
                }
                using (var att = xndHolder.NewAttribute("Name", 1, 0))
                {
                    att.BeginWrite(10);
                    att.Write(mName);
                    att.EndWrite();
                    parentNode.AddAttribute(att.CppPointer);
                }
                for (int i = 0; i < Properties.Count; ++i)
                {
                    using (var node = xndHolder.NewNode("Property", 1, 0))
                    {
                        Properties[i].SaveXnd(xndHolder, node);
                        parentNode.AddNode(node.CppPointer);
                    }
                }
                    
            }

            return true;
        }
    }
    public class AnimatableObjectPropertyDesc : IAnimatablePropertyDesc
    {
        public Rtti.UTypeDesc ClassType
        {
            get { return Rtti.UTypeDesc.TypeOf(TypeStr.ToString()); }
        }
        private VNameString mTypeStr;
        public VNameString TypeStr { get => mTypeStr; set => mTypeStr = value; }

        private VNameString mName;
        public VNameString Name { get => mName; set => mName = value; }
        private int mCurveIndex = -1;
        public int CurveIndex { get => mCurveIndex; set => mCurveIndex = value; }

        public bool LoadXnd(CXndHolder xndHolder, XndNode parentNode)
        {
            using (var att = parentNode.TryGetAttribute("ClassTypeStr"))
            {
                if (!att.IsValidPointer)
                {
                    return false;
                }
                att.BeginRead();
                att.Read(ref mTypeStr);
                att.EndRead();
            }
            using (var att = parentNode.TryGetAttribute("Name"))
            {
                if (!att.IsValidPointer)
                {
                    return false;
                }
                att.BeginRead();
                att.Read(ref mName);
                att.EndRead();
            }
            using (var att = parentNode.TryGetAttribute("CurveIndex"))
            {
                if (!att.IsValidPointer)
                {
                    return false;
                }
                att.BeginRead();
                att.Read(ref mCurveIndex);
                att.EndRead();
            }
            return true;
        }

        public bool SaveXnd(IO.CXndHolder xndHolder, XndNode parentNode)
        {
            unsafe
            {
                using (var att = xndHolder.NewAttribute("ClassTypeStr", 1, 0))
                {
                    att.BeginWrite(10);
                    att.Write(mTypeStr);
                    att.EndWrite();
                    parentNode.AddAttribute(att.CppPointer);
                }
                using (var att = xndHolder.NewAttribute("Name", 1, 0))
                {
                    att.BeginWrite(10);
                    att.Write(mName);
                    att.EndWrite();
                    parentNode.AddAttribute(att.CppPointer);
                }
                using (var att = xndHolder.NewAttribute("CurveIndex", 1, 0))
                {
                    att.BeginWrite(10);
                    att.Write(mCurveIndex);
                    att.EndWrite();
                    parentNode.AddAttribute(att.CppPointer);
                }
            }

            return true;
        }
    }
}
