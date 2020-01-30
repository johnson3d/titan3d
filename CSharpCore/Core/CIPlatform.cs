using System;
using System.Collections.Generic;
using System.Text;
//using System.Management;

namespace EngineNS
{
    [Flags]
    public enum EPlatformType : UInt32
    {
        PLATFORM_WIN = (1 << 0),
        PLATFORM_DROID = (1 << 1),
        PLATFORM_IOS = (1 << 2)
    }
    public partial class CIPlatform : AuxCoreObject<CIPlatform.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public static CIPlatform Instance = new CIPlatform();
        protected EPlatformType mPlatformType;
        public EPlatformType PlatformType
        {
            get { return mPlatformType; }
        }
        internal static Profiler.TimeScope.EProfileFlag mProfileType = Profiler.TimeScope.EProfileFlag.FlagsAll;
        protected EngineNS.ECSType mCSType = ECSType.Client;
        public EngineNS.ECSType CSType
        {
            get { return mCSType; }
        }

        public enum enPlayMode
        {
            Game,
            Editor,
            PlayerInEditor,
            Cook,
        }
        public enPlayMode PlayMode
        {
            get;
            set;
        } = enPlayMode.Game;

        public static bool DebugMode
        {
            get;
            set;
        } = false;

        partial void ReadFileFromAssets(string file, ref List<byte> result);

        public void ReadAllBytes(string file, ref List<byte> result)
        {
            ReadFileFromAssets(file, ref result);
        }

        public bool IsServer = false;

        public void LogCpuData()
        {
            
        }
    }
    public class BrickDescriptor
    {
        public virtual string UnitName
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        public void TAssert(bool condition, string info)
        {
            if (condition == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, $"Test {UnitName}", info);
            }
        }
        public void TInfo(string info)
        {
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, $"Test {UnitName}", info);
        }
        public virtual async System.Threading.Tasks.Task DoTest()
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
    }
}
