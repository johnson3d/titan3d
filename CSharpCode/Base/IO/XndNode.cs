using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public class TtXndNode : AuxPtrType<XndNode>
    {
        public TtXndNode(XndNode ptr)
        {
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
            var result = new TtXndNode(GetNodePtr(index));
            result.Core_AddRef();
            return result;
        }
        public XndAttribute TryGetAttribute(string name)
        {
            unsafe
            {
                return new XndAttribute(mCoreObject.TryGetAttribute(name));
            }
        }
        public XndNode TryGetChildNode(string name)
        {
            unsafe
            {
                return new XndNode(mCoreObject.TryGetChildNode(name));
            }
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
