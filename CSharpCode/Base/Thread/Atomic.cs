using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class TtAtomic_Int
    {
        private int Count;
        public int Value
        { 
            get => Count;
            set
            {
                Exchange(value);
            }
        }
        public int Exchange(int value)
        {
            return System.Threading.Interlocked.Exchange(ref Count, value);
        }
        public static TtAtomic_Int operator ++(TtAtomic_Int obj)
        {
            System.Threading.Interlocked.Increment(ref obj.Count);
            return obj;
        }
        public static TtAtomic_Int operator --(TtAtomic_Int obj)
        {
            System.Threading.Interlocked.Decrement(ref obj.Count);
            return obj;
        }
        public static TtAtomic_Int operator + (TtAtomic_Int obj, int v)
        {
            System.Threading.Interlocked.Add(ref obj.Count, v);
            return obj;
        }
        public static TtAtomic_Int operator -(TtAtomic_Int obj, int v)
        {
            System.Threading.Interlocked.Add(ref obj.Count, -v);
            return obj;
        }
    }
}
