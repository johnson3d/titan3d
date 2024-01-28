/*
* Copyright (c) 2012-2020 AssimpNet - Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Assimp.Unmanaged
{
    internal abstract class UnmanagedLibraryImplementation : IDisposable
    {
        private String m_defaultLibName;
        private Type[] m_unmanagedFunctionDelegateTypes;
        private Dictionary<String, Delegate> m_nameToUnmanagedFunction;
        private IntPtr m_libraryHandle;
        private bool m_isDisposed;
        private bool m_throwOnLoadFailure;

        public bool IsLibraryLoaded
        {
            get
            {
                return m_libraryHandle != IntPtr.Zero;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return m_isDisposed;
            }
        }

        public String DefaultLibraryName
        {
            get
            {
                return m_defaultLibName;
            }
        }

        public bool ThrowOnLoadFailure
        {
            get
            {
                return m_throwOnLoadFailure;
            }
            set
            {
                m_throwOnLoadFailure = value;
            }
        }

        public abstract String DllExtension { get; }

        public virtual String DllPrefix { get { return String.Empty; } }

        public UnmanagedLibraryImplementation(String defaultLibName, Type[] unmanagedFunctionDelegateTypes)
        {
            m_defaultLibName = DllPrefix + Path.ChangeExtension(defaultLibName, DllExtension);

            m_unmanagedFunctionDelegateTypes = unmanagedFunctionDelegateTypes;

            m_nameToUnmanagedFunction = new Dictionary<String, Delegate>();
            m_isDisposed = false;
            m_libraryHandle = IntPtr.Zero;

            m_throwOnLoadFailure = true;
        }

        ~UnmanagedLibraryImplementation()
        {
            Dispose(false);
        }

        public T GetFunction<T>(String functionName) where T : class
        {
            if(String.IsNullOrEmpty(functionName))
                return null;

            Delegate function;
            if(!m_nameToUnmanagedFunction.TryGetValue(functionName, out function))
                return null;

            Object obj = (Object) function;

            return (T) obj;
        }

        public bool LoadLibrary(String path)
        {
            FreeLibrary(true);

            m_libraryHandle = NativeLoadLibrary(path);

            if(m_libraryHandle != IntPtr.Zero)
                LoadFunctions();

            return m_libraryHandle != IntPtr.Zero;
        }

        public bool FreeLibrary()
        {
            return FreeLibrary(true);
        }

        private bool FreeLibrary(bool clearFunctions)
        {
            if(m_libraryHandle != IntPtr.Zero)
            {
                NativeFreeLibrary(m_libraryHandle);
                m_libraryHandle = IntPtr.Zero;

                if(clearFunctions)
                    m_nameToUnmanagedFunction.Clear();

                return true;
            }

            return false;
        }

        private void LoadFunctions()
        {
            foreach(Type funcType in m_unmanagedFunctionDelegateTypes)
            {
                String funcName = GetUnmanagedName(funcType);
                if(String.IsNullOrEmpty(funcName))
                {
                    System.Diagnostics.Debug.Assert(false, String.Format("No UnmanagedFunctionNameAttribute on {0} type.", funcType.AssemblyQualifiedName));
                    continue;
                }

                IntPtr procAddr = NativeGetProcAddress(m_libraryHandle, funcName);
                if(procAddr == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.Assert(false, String.Format("No unmanaged function found for {0} type.", funcType.AssemblyQualifiedName));
                    continue;
                }

                Delegate function;
                if(!m_nameToUnmanagedFunction.TryGetValue(funcName, out function))
                {
                    function = PlatformHelper.GetDelegateForFunctionPointer(procAddr, funcType);
                    m_nameToUnmanagedFunction.Add(funcName, function);
                }
            }
        }

        private String GetUnmanagedName(Type funcType)
        {
            object[] attributes = PlatformHelper.GetCustomAttributes(funcType, typeof(UnmanagedFunctionNameAttribute), false);
            foreach(object attr in attributes)
            {
                if(attr is UnmanagedFunctionNameAttribute)
                    return (attr as UnmanagedFunctionNameAttribute).UnmanagedFunctionName;
            }

            return null;
        }

        protected abstract IntPtr NativeLoadLibrary(String path);
        protected abstract void NativeFreeLibrary(IntPtr handle);
        protected abstract IntPtr NativeGetProcAddress(IntPtr handle, String functionName);

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(!m_isDisposed)
            {
                FreeLibrary(isDisposing);

                m_isDisposed = true;
            }
        }
    }
}
