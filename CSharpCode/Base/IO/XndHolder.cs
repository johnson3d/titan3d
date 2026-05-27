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
    public class TtXndHolder : AuxPtrType<XndHolder> , IDisposable
    {
        private TtXndHolder()
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
        public static unsafe TtXndHolder LoadXnd(string file)
        {
            if (file == null)
                return null;
            //var t1 = Support.TtTime.HighPrecision_GetTickCount();
            var result = new TtXndHolder();
            using (var f2m = TtRes2Memory.CreateFromFile(file))
            {
                if (f2m == null)
                    return null;

                if (false == result.mCoreObject.LoadXnd(f2m.mCoreObject))
                    return null;

                result.mRootNode = new TtXndNode(result, new XndNode(result.mCoreObject.GetRootNode()));
                result.mRootNode.Core_AddRef();
                //var t2 = Support.TtTime.HighPrecision_GetTickCount();
                //Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Info, $"LoadXnd {file} cost {(t2 - t1) / 1000} ms");
                return result;
            }   
        }
        public TtXndHolder(string name, UInt32 ver, UInt32 flags)
        {
            mCoreObject = XndHolder.CreateInstance();
            using (var ptr = NewNode(name, ver, flags))
            {
                mRootNode = new TtXndNode(this, ptr);
                mRootNode.Core_AddRef();
                unsafe
                {
                    mCoreObject.SetRootNode(mRootNode.mCoreObject);
                }
            }   
        }
        public void SaveXnd(string file)
        {
            var path = IO.TtFileManager.GetParentPathName(file);
            IO.TtFileManager.SureDirectory(path);
            TtRes2Memory.OnBeforeWriteFile(file);
            mCoreObject.SaveXnd(file);
            TtRes2Memory.OnAfterWriteFile(file);
        }
        public void SaveXndWithoutHead(TtMemWriter writer)
        {
            mCoreObject.SaveXndWithoutHead(writer.Writer.NativeSuper);
        }
        TtXndNode mRootNode;
        public TtXndNode RootNode
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
        public int ResRefCount
        {
            get
            {
                if (mCoreObject.GetResource().IsValidPointer == false)
                    return -2;
                return mCoreObject.GetResource().GetRefCount();
            }
        }
    }

    [UnitTest.TtTest]
    public class UTest_XndTester
    {
        public unsafe void UnitTestEntrance()
        {
            var rn = RName.GetRName("UTest/t0.xnd");
            IO.TtFileManager.SureDirectory(RName.GetRName("UTest").Address);
            {
                var xnd = new TtXndHolder("TestRoot", 1, 0);
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
                var xnd = TtXndHolder.LoadXnd(rn.Address);
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
