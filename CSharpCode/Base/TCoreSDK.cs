using EngineNS.Rtti;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EngineNS
{
    public unsafe partial struct CoreSDK
    {
        public const string CoreModule = "Core.Window.dll";

        public static void CopyString2Ansi(BigStackBuffer buffer, string srcStr)
        {
            unsafe
            {
                //var p = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(srcStr);
                //SDK_StrCpy(buffer.GetBuffer(), p.ToPointer(), (uint)buffer.GetSize());
                //System.Runtime.InteropServices.Marshal.FreeHGlobal(p);

                var len = Encoding.ASCII.GetByteCount(srcStr);
                if (len == 0)
                    return;
                if (buffer.GetSize() + 16 < len)
                {
                    buffer.Resize(buffer.GetSize() + len);
                }

                var utf8Buffer = new Span<byte>(buffer.GetBuffer(), len);
                Encoding.ASCII.GetBytes(srcStr, utf8Buffer);
            }
        }
        public static void CopyString2Utf8(BigStackBuffer buffer, string srcStr)
        {
            if (srcStr == null)
                return;
            unsafe
            {
                var len = Encoding.UTF8.GetByteCount(srcStr);
                if (len == 0)
                    return;
                if (buffer.GetSize() + 16 < len)
                {
                    buffer.Resize(buffer.GetSize() + len);
                }

                var utf8Buffer = new Span<byte>(buffer.GetBuffer(), len);
                Encoding.UTF8.GetBytes(srcStr, utf8Buffer);
                //var tt = Encoding.UTF8.GetString((byte*)buffer.GetBuffer(), len);
            }
        }
        public static void CopyString2Utf16(BigStackBuffer buffer, string srcStr)
        {
            unsafe
            {
                var len = srcStr.Length;
                if (len == 0)
                    return;
                if (buffer.GetSize() + 16 < len)
                {
                    buffer.Resize(buffer.GetSize() + len);
                }
                fixed(char* p = srcStr)
                {
                    CoreSDK.MemoryCopy(buffer.GetBuffer(), p, (uint)len);
                }
            }
        }
        public static void CopyString2Utf32(BigStackBuffer buffer, string srcStr)
        {
            unsafe
            {
                var len = Encoding.UTF32.GetByteCount(srcStr);
                if (len == 0)
                    return;
                if (buffer.GetSize() + 16 < len)
                {
                    buffer.Resize(buffer.GetSize() + len);
                }

                var utf8Buffer = new Span<byte>(buffer.GetBuffer(), len);
                Encoding.UTF32.GetBytes(srcStr, utf8Buffer);
                //var tt = Encoding.UTF8.GetString((byte*)buffer.GetBuffer(), len);
            }
        }
        public static void IUnknown_Release(IntPtr unk)
        {
            IUnknown_Release(unk.ToPointer());
        }
        public static void DisposeObject<T>(ref T obj) where T : class, IDisposable
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }
        public unsafe static bool IsNullPointer<T>(T* ptr) where T : unmanaged
        {
            return ptr == (T*)0;
        }
        public static int GetShaderVarTypeSize(NxRHI.EShaderVarType type)
        {
            switch (type)
            {
                case NxRHI.EShaderVarType.SVT_Float:
                    return 4;
                case NxRHI.EShaderVarType.SVT_Int:
                    return 4;
                default:
                    return -1;
            }
        }
    }
    public unsafe partial struct BigStackBuffer : IDisposable
    {
        public string AsTextAnsi()
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)GetBuffer());
        }
        public string AsTextUtf8()
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringUTF8((IntPtr)GetBuffer());
        }
        public string AsTextUtf16()
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringUni((IntPtr)GetBuffer());
        }
        //public string AsTextUtf32()
        //{
        //    return System.Runtime.InteropServices.Marshal.PtrToStringUni((IntPtr)GetBuffer());
        //}
        public void SetTextAnsi(string txt)
        {
            CoreSDK.CopyString2Ansi(this, txt);
        }
        public void SetTextUtf8(string txt)
        {
            CoreSDK.CopyString2Utf8(this, txt);
        }
        public void SetTextUtf16(string txt)
        {
            CoreSDK.CopyString2Utf16(this, txt);
        }
        //public void SetTextUtf32(string txt)
        //{
        //    CoreSDK.CopyString2Utf32(this, txt);
        //}
    }

    public class TtGlobalConfigVar
    {
        protected TtGlobalConfigVar()
        {
            Handle = uint.MaxValue;
        }
        uint Handle;
        public string Name
        {
            get
            {
                return FGlobalConfig.GetInstance().GetName(Handle);
            }
        }
        public int ValueI32
        {
            get
            {
                return FGlobalConfig.GetInstance().GetConfigValueI32(Handle);
            }
        }
        public uint ValueUI32
        {
            get
            {
                return FGlobalConfig.GetInstance().GetConfigValueUI32(Handle);
            }
        }
        public float ValueF32
        {
            get
            {
                return FGlobalConfig.GetInstance().GetConfigValueF32(Handle);
            }
        }
        protected void OnValueChanged()
        {

        }
        public void SetValue(int value)
        {
            FGlobalConfig.GetInstance().SetConfigValueI32(Handle, value);
            OnValueChanged();
        }
        public void SetValue(uint value)
        {
            FGlobalConfig.GetInstance().SetConfigValueUI32(Handle, value);
            OnValueChanged();
        }
        public void SetValue(float value)
        {
            FGlobalConfig.GetInstance().SetConfigValueF32(Handle, value);
            OnValueChanged();
        }
        public static TtGlobalConfigVar CreateConfigVar(UTypeDesc type, string name, int value)
        {
            var result = Rtti.UTypeDescManager.CreateInstance(type, null) as TtGlobalConfigVar;
            result.Handle = FGlobalConfig.GetInstance().SetConfigValueI32(name, value);
            result.OnValueChanged();
            return result;
        }
        public static TtGlobalConfigVar CreateConfigVar(UTypeDesc type, string name, uint value)
        {
            var result = Rtti.UTypeDescManager.CreateInstance(type, null) as TtGlobalConfigVar;
            result.Handle = FGlobalConfig.GetInstance().SetConfigValueUI32(name, value);
            result.OnValueChanged();
            return result;
        }
        public static TtGlobalConfigVar CreateConfigVar(UTypeDesc type, string name, float value)
        {
            var result = Rtti.UTypeDescManager.CreateInstance(type, null) as TtGlobalConfigVar;
            result.Handle = FGlobalConfig.GetInstance().SetConfigValueF32(name, value);
            result.OnValueChanged();
            return result;
        }
        public static T CreateConfigVar<T>(string name, int value) where T : TtGlobalConfigVar, new()
        {
            var result = new T();
            result.Handle = FGlobalConfig.GetInstance().SetConfigValueI32(name, value);
            return result;
        }
        public static T CreateConfigVar<T>(string name, uint value) where T : TtGlobalConfigVar, new()
        {
            var result = new T();
            result.Handle = FGlobalConfig.GetInstance().SetConfigValueUI32(name, value);
            result.OnValueChanged();
            return result;
        }
        public static T CreateConfigVar<T>(string name, float value) where T : TtGlobalConfigVar, new()
        {
            var result = new T();
            result.Handle = FGlobalConfig.GetInstance().SetConfigValueF32(name, value);
            result.OnValueChanged();
            return result;
        }
    }

    public class TtGlobalConfig : IO.BaseSerializer
    {
        public TtGlobalConfig()
        {
            ConfigType = Rtti.UTypeDesc.TypeOf(typeof(TtGlobalConfigVar));
        }
        [Rtti.Meta]
        public string Name { get; set; }
        [Rtti.Meta]
        public string Value { get; set; }
        [Rtti.Meta]
        public NxRHI.EShaderVarType ValueType { get; set; } = NxRHI.EShaderVarType.SVT_Int;
        [Rtti.Meta]
        public Rtti.UTypeDesc ConfigType { get; set; }
        public TtGlobalConfigVar SetToGlobalConfig()
        {
            switch (ValueType)
            {
                case NxRHI.EShaderVarType.SVT_Int:
                    return TtGlobalConfigVar.CreateConfigVar(ConfigType, Name, System.Convert.ToInt32(Value));
                case NxRHI.EShaderVarType.SVT_Float:
                    return TtGlobalConfigVar.CreateConfigVar(ConfigType, Name, System.Convert.ToSingle(Value));
            }
            return null;
        }
    }
}

public unsafe partial struct VNameString
{
    public static bool operator ==(VNameString lh, VNameString rh)
    {
        return lh.m_Index == rh.Index;
    }
    public static bool operator !=(VNameString lh, VNameString rh)
    {
        return lh.m_Index != rh.Index;
    }
    public static VNameString FromString(string name)
    {
        var result = new VNameString();
        result.Index = VNameString.GetIndexFromString(name);
        return result;
    }
    public string Text
    {
        get
        {
            return c_str();
        }
    }
    public override int GetHashCode()
    {
        return m_Index;
    }
    public override bool Equals([NotNullWhen(true)] object obj)
    {
        var nameStr = (VNameString)obj;
        return this == nameStr;
    }
    public bool StartWith(string name)
    {
        return c_str().StartsWith(name);
    }
}

