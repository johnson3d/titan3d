using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.IO
{
    public interface IConstructorStruct
    {
        void ToDefault();
    }
    public class StructCompress<T, W, R> 
        where T : unmanaged, IConstructorStruct
        where W : IO.Serializer.IWriter
        where R : IO.Serializer.IReader
    {
        static StructCompress()
        {
            TemplateObject.ToDefault();
        }
        private static T TemplateObject;
        public static StructCompress<T, W, R> Instance = new StructCompress<T, W, R>();
        public int Write(ref W writer, ref T obj, ref T template)
        {
            unsafe
            {
                T sub = new T();
                fixed (T* lh = &obj)
                fixed (T* rh = &template)
                {
                    CoreSDK.SDK_ByteArray_Sub((byte*)lh, (byte*)rh, (byte*)&sub, sizeof(T));
                    UInt16 size = (UInt16)CoreSDK.SDK_Compress_RLE((byte*)&sub, (UInt32)sizeof(T), null);
                    byte* buffer = stackalloc byte[size];
                    CoreSDK.SDK_Compress_RLE((byte*)&sub, (UInt32)sizeof(T), buffer);
                    writer.Write(size);
                    writer.WritePtr(buffer, size);
                    return size;
                }
            }
        }
        public bool Read(ref R reader, ref T obj, ref T template)
        {
            unsafe
            {
                T result = new T();
                fixed (T* lh = &obj)
                fixed (T* rh = &template)
                {
                    UInt16 size = 0;
                    reader.Read(out size);
                    if (size >= 2 * sizeof(T))
                        return false;

                    byte* buffer = stackalloc byte[size];
                    reader.ReadPtr(buffer, size);
                    
                    var toSize = CoreSDK.SDK_UnCompress_RLE(buffer, (UInt32)size, null);
                    if (toSize != sizeof(T))
                        return false;

                    CoreSDK.SDK_UnCompress_RLE(buffer, (UInt32)size, (byte*)&result);
                    CoreSDK.SDK_ByteArray_Add((byte*)&result, (byte*)rh, (byte*)lh, sizeof(T));

                    return true;
                }
            }
        }
        public int Write(ref W writer, ref T obj)
        {
            return Write(ref writer, ref obj, ref TemplateObject);
        }
        public bool Read(ref R reader, ref T obj)
        {
            return Read(ref reader, ref obj, ref TemplateObject);
        }
    }
}
