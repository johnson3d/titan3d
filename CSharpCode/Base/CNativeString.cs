using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class TtNativeString
    {
        FNativeString mNativeString;
        public TtNativeString(string txt)
        {
            mNativeString = FNativeString.CreateInstance(txt);
        }
        public TtNativeString()
        {
            mNativeString = FNativeString.CreateInstance();
        }
        ~TtNativeString()
        {
            mNativeString.Dispose();
        }
        public void SetText(string txt)
        {
            mNativeString.SetText(txt);
        }
        public string GetText()
        {
            unsafe
            {
                return Rtti.UNativeCoreProvider.MarshalPtrAnsi(mNativeString.GetText());
            }
        }
        public unsafe sbyte* GetTextPointer()
        {
            return mNativeString.GetText();
        }
    }
}
