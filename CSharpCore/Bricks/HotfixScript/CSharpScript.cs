using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.HotfixScript
{
    public class CSharpScript
    {
        ILRuntime.Runtime.Enviorment.AppDomain mDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        public ILRuntime.Runtime.Enviorment.AppDomain Domain
        {
            get { return mDomain; }
        }
        public bool LoadAssembly(RName dll, RName pdb)
        {
            var dataDll = LoadAllFile(dll);
            if (dataDll == null)
                return false;
            var dataPdb = LoadAllFile(pdb);
            if (dataPdb == null)
                return false;

            using (System.IO.MemoryStream fs = new System.IO.MemoryStream(dataDll))
            {
                using (System.IO.MemoryStream p = new System.IO.MemoryStream(dataPdb))
                {
                    mDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
                }
            }

            return true;
        }
        private byte[] LoadAllFile(RName dll)
        {
            try
            {
                var dllFile = new System.IO.FileStream(dll.Address, System.IO.FileMode.Open);

                var binReader = new System.IO.BinaryReader(dllFile);
                var bBuffer = new byte[dllFile.Length];
                binReader.Read(bBuffer, 0, (int)dllFile.Length);
                binReader.Close();
                dllFile.Close();
                return bBuffer;
            }
            catch(Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null;
            }
        }
        private object ILInvoke(out bool succesed, string klass, string method, object host, params object[] args)
        {
            var type = mDomain.GetType(klass);
            if (type == null)
            {
                succesed = false;
                return null;
            }
            var mi = type.GetMethod(method, args!=null? args.Length : 0);
            if (mi == null)
            {
                succesed = false;
                return null;
            }

            succesed = true;
            return mDomain.Invoke(mi, host, args);
            //return mDomain.Invoke(klass, method, host, args);
        }
        public object Invoke(out bool succesed, string klass, string method, object host, params object[] args)
        {
            var ret = ILInvoke(out succesed, klass, method, host, args);
            if (succesed)
            {
                return ret;
            }
            var type = Rtti.RttiHelper.GetTypeFromTypeFullName(klass);
            if (type == null)
            {
                succesed = false;
                return null;
            }
            var mi = type.GetMethod(method);
            if (mi == null)
            {
                succesed = false;
                return null;
            }

            succesed = true;
            return mi.Invoke(host, args);
        }
        public object CreateInstance(string klass)
        {
            var obj = mDomain.Instantiate(klass);
            if (obj != null)
                return obj;
            var type = Rtti.RttiHelper.GetTypeFromTypeFullName(klass);
            if (type == null)
                return null;
            return System.Activator.CreateInstance(type);
        }
        public ILRuntime.CLR.TypeSystem.IType GetType(string klass)
        {
            ILRuntime.CLR.TypeSystem.IType type;
            if (mDomain.LoadedTypes.TryGetValue(klass, out type) == false)
                return null;
            
            return type;
        }
    }

    public class CSharpScriptHelper : BrickDescriptor
    {
        public override async System.Threading.Tasks.Task DoTest()
        {
            var csScript = new CSharpScript();
            if(csScript.LoadAssembly(RName.GetRName("EditorCommon.dll"), RName.GetRName("EditorCommon.pdb")))
            {
                bool succesed;
                var ret = csScript.Invoke(out succesed, "EditorCommon.Program", "TestCSScript1", null,
                            new object[]
                            {
                                (int)1,
                                (float)2.0f
                            });
                if (succesed)
                    this.TInfo(ret.ToString());
            }

            await base.DoTest();
        }
    }
}
