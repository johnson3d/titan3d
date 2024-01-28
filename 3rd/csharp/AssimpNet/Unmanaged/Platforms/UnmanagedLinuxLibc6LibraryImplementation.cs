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
    internal sealed class UnmanagedLinuxLibc6LibraryImplementation : UnmanagedLibraryImplementation
    {
        public override String DllExtension
        {
            get
            {
                return ".so";
            }
        }

        public override String DllPrefix
        {
            get
            {
                return "lib";
            }
        }

        public UnmanagedLinuxLibc6LibraryImplementation(String defaultLibName, Type[] unmanagedFunctionDelegateTypes)
            : base(defaultLibName, unmanagedFunctionDelegateTypes)
        {
        }

        protected override IntPtr NativeLoadLibrary(String path)
        {
            IntPtr libraryHandle = dlopen(path, RTLD_NOW);

            if(libraryHandle == IntPtr.Zero &&  ThrowOnLoadFailure)
            {
                IntPtr errPtr = dlerror();
                String msg = Marshal.PtrToStringAnsi(errPtr);
                if(!String.IsNullOrEmpty(msg))
                    throw new AssimpException(String.Format("Error loading unmanaged library from path: {0}\n\n{1}", path, msg));
                else
                    throw new AssimpException(String.Format("Error loading unmanaged library from path: {0}", path));
            }

            return libraryHandle;
        }

        protected override IntPtr NativeGetProcAddress(IntPtr handle, String functionName)
        {
            return dlsym(handle, functionName);
        }

        protected override void NativeFreeLibrary(IntPtr handle)
        {
            dlclose(handle);
        }

        #region Native Methods

        [DllImport("libc.so.6")]
        private static extern IntPtr dlopen(String fileName, int flags);

        [DllImport("libc.so.6")]
        private static extern IntPtr dlsym(IntPtr handle, String functionName);

        [DllImport("libc.so.6")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libc.so.6")]
        private static extern IntPtr dlerror();

        private const int RTLD_NOW = 2;

        #endregion
    }
}
