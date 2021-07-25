using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public partial struct XndElement
    {
        public string Name
        {
            get
            {
                return GetName();
            }
        }
    }
}

namespace EngineNS.IO
{
    public class CXndHolder : AuxPtrType<XndHolder> , IDisposable
    {
        private CXndHolder()
        {
            mCoreObject = XndHolder.CreateInstance();
        }
        public override void Dispose()
        {
            mCoreObject.TryReleaseHolder();
            base.Dispose();
        }
        public XndAttribute NewAttribute(string name, UInt32 ver, UInt32 flags)
        {
            unsafe
            {
                var p = mCoreObject.NewAttribute(name, ver, flags);
                return new XndAttribute(p);
            }
        }
        public XndNode NewNode(string name, UInt32 ver, UInt32 flags)
        {
            unsafe
            {
                var p = mCoreObject.NewNode(name, ver, flags);
                return new XndNode(p);
            }
        }
        public static CXndHolder LoadXnd(string file)
        {
            var result = new CXndHolder();
            if (result.mCoreObject.LoadXnd(file) == false)
                return null;
            unsafe
            {
                result.mRootNode = new CXndNode(new XndNode(result.mCoreObject.GetRootNode()));
                result.mRootNode.Core_AddRef();
                return result;
            }
        }
        public CXndHolder(string name, UInt32 ver, UInt32 flags)
        {
            mCoreObject = XndHolder.CreateInstance();
            using (var ptr = NewNode(name, ver, flags))
            {
                mRootNode = new CXndNode(ptr);
                mRootNode.Core_AddRef();
                unsafe
                {
                    mCoreObject.SetRootNode(mRootNode.mCoreObject);
                }
            }   
        }
        public void SaveXnd(string file)
        {
            var path = IO.FileManager.GetParentPathName(file);
            IO.FileManager.SureDirectory(path);
            mCoreObject.SaveXnd(file);
        }
        CXndNode mRootNode;
        public CXndNode RootNode
        {
            get { return mRootNode; }
            set
            {
                mRootNode = value;
                unsafe
                {
                    mCoreObject.SetRootNode(value.mCoreObject);
                }
            }
        }
    }

    [UTest.UTest]
    public class UTest_XndTester
    {
        public unsafe void UnitTestEntrance()
        {
            var rn = RName.GetRName("UTest/t0.xnd");
            IO.FileManager.SureDirectory(RName.GetRName("UTest").Address);
            {
                var xnd = new CXndHolder("TestRoot", 1, 0);
                using (var attr = xnd.NewAttribute("Att0", 1, 0))
                {
                    attr.BeginWrite(100);
                    int a = 0;
                    attr.Write(a);
                    attr.Write(a);
                    attr.EndWrite();
                    xnd.RootNode.AddAttribute(attr);
                }

                using (var attr = xnd.NewAttribute("Att1", 1, 0))
                {
                    attr.BeginWrite(100);
                    int a = 0;
                    attr.Write(a);
                    attr.Write(a);
                    attr.EndWrite();
                    xnd.RootNode.AddAttribute(attr);
                }

                using (var cld = xnd.NewNode("Node0", 1, 0))
                {
                    xnd.RootNode.AddNode(cld);

                    using (var attr = xnd.NewAttribute("Att1", 1, 0))
                    {
                        attr.BeginWrite(100);
                        int a = 0;
                        attr.Write(a);
                        attr.Write(a);
                        attr.EndWrite();
                        cld.AddAttribute(attr);
                    }
                }

                xnd.SaveXnd(rn.Address);
            }
            {
                var xnd = CXndHolder.LoadXnd(rn.Address);
                for (uint i = 0; i < xnd.RootNode.NumOfAttribute; i++)
                {
                    var attr = xnd.RootNode.GetAttribute(i);
                    if (!attr.IsValidPointer)
                        continue;

                    if (attr.Name == "Att0")
                    {
                        attr.BeginRead();
                        int a = 1;
                        attr.Read(ref a);
                        a = 2;
                        attr.Read(ref a);
                        attr.EndRead();
                    }
                }
            }
        }
    }
}
