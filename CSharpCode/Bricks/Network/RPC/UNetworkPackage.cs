using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Network.RPC
{
    public struct UMemWriter : IO.ICoreWriter, IDisposable
    {
        public static UMemWriter CreateInstance()
        {
            UMemWriter result = new UMemWriter();
            result.Writer = MemStreamWriter.CreateInstance();
            return result;
        }
        public object Tag;
        public MemStreamWriter Writer;
        public IO.EIOType IOType { get => IO.EIOType.Network; }
        public ulong GetPosition()
        {
            return Writer.Tell();
        }
        public void Seek(ulong pos)
        {
            Writer.Seek(pos);
        }
        public unsafe void WritePtr(void* p, int length)
        {
            Writer.Write(p, (uint)length);
        }
        public void Dispose()
        {
            Writer.Dispose();
        }
        public void SurePkgHeader()
        {
            System.Diagnostics.Debug.Assert(Writer.Tell() < ushort.MaxValue);
            unsafe
            {
                ((FPkgHeader*)Writer.GetDataPointer())->PackageSize = (ushort)Writer.Tell();
            }
        }
    }
    public struct UMemReader : IO.ICoreReader, IDisposable
    {
        public unsafe static UMemReader CreateInstance(byte* ptr, ulong len)
        {
            UMemReader result = new UMemReader();
            result.Reader = MemStreamReader.CreateInstance();
            result.Reader.ProxyPointer(ptr, len);
            return result;
        }
        public MemStreamReader Reader;
        public IO.EIOType IOType { get => IO.EIOType.Network; }
        public ulong GetPosition()
        {
            return Reader.Tell();
        }
        public void Seek(ulong pos)
        {
            Reader.Seek(pos);
        }
        public unsafe void ReadPtr(void* p, int length)
        {
            Reader.Read(p, (uint)length);
        }
        public void Dispose()
        {
            Reader.Dispose();
        }
    }
}
