using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public class TtXndNode : AuxPtrType<XndNode>
    {
        public TtXndHolder Holder { get; private set; }
        public TtXndNode(TtXndHolder holder, XndNode ptr)
        {
            Holder = holder;
            mCoreObject = ptr;
        }
        public string Name
        {
            get
            {
                return mCoreObject.NativeSuper.GetName();
            }
            set
            {
                var super = mCoreObject.NativeSuper;
                super.SetName(value);
            }
        }
        public UInt32 Version
        {
            get
            {
                return mCoreObject.NativeSuper.mVersion;
            }
            set
            {
                var super = mCoreObject.NativeSuper;
                super.mVersion = value;
            }
        }
        public UInt32 Flags
        {
            get
            {
                return mCoreObject.NativeSuper.mFlags;
            }
            set
            {
                var super = mCoreObject.NativeSuper;
                super.mFlags = value;
            }
        }
        public UInt32 NumOfAttribute
        {
            get
            {
                return mCoreObject.GetNumOfAttribute();
            }
        }
        public UInt32 NumOfNode
        {
            get
            {
                return mCoreObject.GetNumOfNode();
            }
        }
        public XndAttribute GetAttribute(UInt32 index)
        {
            unsafe
            {
                return new XndAttribute(mCoreObject.GetAttribute(index));
            }
        }
        public XndNode GetNodePtr(UInt32 index)
        {
            unsafe
            {
                return new XndNode(mCoreObject.GetNode(index));
            }
        }
        public TtXndNode GetNode(UInt32 index)
        {
            var result = new TtXndNode(Holder, GetNodePtr(index));
            result.Core_AddRef();
            return result;
        }
        public unsafe XndAttribute TryGetAttribute(string name)
        {
            return new XndAttribute(mCoreObject.TryGetAttribute(name));
        }
        public unsafe XndAttribute GetOrAddAttribute(string name, uint ver, uint flags, bool bCheckName = true)
        {
            return new XndAttribute(mCoreObject.GetOrAddAttribute(name, ver, flags, bCheckName));
        }
        public unsafe XndNode TryGetChildNode(string name)
        {
            return new XndNode(mCoreObject.TryGetChildNode(name));
        }
        public unsafe XndNode GetOrAddNode(string name, uint ver, uint flags, bool bCheckName)
        {
            return new XndNode(mCoreObject.GetOrAddNode(name, ver, flags, bCheckName));
        }
        public void AddAttributePtr(XndAttribute attr)
        {
            unsafe
            {
                mCoreObject.AddAttribute(attr);
            }
        }
        public void AddNodePtr(XndNode node)
        {
            unsafe
            {
                mCoreObject.AddNode(node);
            }
        }
        public void AddAttribute(XndAttribute attr)
        {
            unsafe
            {
                mCoreObject.AddAttribute(attr);
            }
        }
        public void AddNode(XndNode node)
        {
            unsafe
            {
                mCoreObject.AddNode(node);
            }
        }
    }
}
