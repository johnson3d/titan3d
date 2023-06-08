using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    #region byte n
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Byte2
    {
        public byte X;
        public byte Y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Byte3
    {
        public byte X;
        public byte Y;
        public byte Z;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Byte4
    {
        public byte X;
        public byte Y;
        public byte Z;
        public byte W;
        public byte R { get => X; set => X = value; }
        public byte G { get => Y; set => Y = value; }
        public byte B { get => Z; set => Z = value; }
        public byte A { get => W; set => W = value; }
    }
    #endregion

    #region bool n
    public struct Bool2
    {
        public bool X;
        public bool Y;
        public bool All()
        {
            return X && Y;
        }
        public bool Any()
        {
            return X || Y;
        }
    }
    public struct Bool3
    {
        public bool X;
        public bool Y;
        public bool Z;
        public Bool3(bool x, bool y, bool z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public bool All()
        {
            return X && Y && Z;
        }
        public bool Any()
        {
            return X || Y || Z;
        }
    }
    public struct Bool4
    {
        public bool X;
        public bool Y;
        public bool Z;
        public bool W;
        public bool All()
        {
            return X && Y && Z && W;
        }
        public bool Any()
        {
            return X || Y || Z || W;
        }
    }
    #endregion
}
