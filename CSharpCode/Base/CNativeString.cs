using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class CNativeString
    {
        FNativeString mNativeString;
        public CNativeString(string txt)
        {
            mNativeString = FNativeString.CreateInstance(txt);
        }
        public CNativeString()
        {
            mNativeString = FNativeString.CreateInstance();
        }
        ~CNativeString()
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
