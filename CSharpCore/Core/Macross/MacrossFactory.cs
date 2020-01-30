using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Macross
{
    public class MacrossFactory
    {
        protected static MacrossFactory smInstance = null;// new MacrossFactory();
        public static MacrossFactory Instance
        {
            get
            {
                if (smInstance == null)
                {
                    CEngine.Instance.MacrossDataManager.RefreshMacrossCollector();
                }
                return smInstance;
            }
        }
        public MacrossFactory()
        {
            smInstance = this;
        }
        protected static Dictionary<RName, MacrossDesc> Describes = new Dictionary<RName, MacrossDesc>(new RName.EqualityComparer());
        public static Dictionary<RName, Type> RegTypes = new Dictionary<RName, Type>(new RName.EqualityComparer());
        public static MacrossDesc SetVersion(RName name, int ver)
        {
            MacrossDesc desc;
            if (Describes.TryGetValue(name, out desc) == false)
            {
                desc = new MacrossDesc();
                Describes.Add(name, desc);
            }
            desc.Version = ver;
            return desc;
        }
        public MacrossDesc GetDesc(RName name)
        {
            MacrossDesc desc;
            if (Describes.TryGetValue(name, out desc) == false)
            {
                return null;
            }
            return desc;
        }
        public virtual object CreateMacrossObject(RName name, EngineNS.Rtti.VAssembly assembly)
        {
            return null;
        }
        public virtual int GetVersion(RName name)
        {
            return 0;
        }
        public object CreateMacrossObject(RName name, out MacrossDesc desc)
        {
            desc = GetDesc(name);
            //string dllName = "";
            //switch(EngineNS.CIPlatform.Instance.PlatformType)
            //{
            //    case EPlatformType.PLATFORM_WIN:
            //        dllName = "MacrossScript.dll";
            //        break;
            //    case EPlatformType.PLATFORM_DROID:
            //        dllName = "MacrossScript.Android.dll";
            //        break;
            //    default:
            //        throw new InvalidOperationException();
            //}
            var assembly = EngineNS.Rtti.RttiHelper.GetAnalyseAssembly(CIPlatform.Instance.CSType, "Game");
            var ret = CreateMacrossObject(name, assembly);
            if (desc == null)
            {
                SetVersion(name, GetVersion(name));
                desc = GetDesc(name);
            }
            return ret;
        }
        public virtual RName GetMacrossRName(Type macrossType)
        {
            return RName.EmptyName;
        }
        public virtual System.Type GetMacrossType(RName macrossType)
        {
            MacrossDesc desc;
            return CreateMacrossObject(macrossType, out desc).GetType();
        }
    }
}
