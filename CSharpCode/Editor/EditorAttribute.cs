using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class UCs2CppAttribute : Attribute
    {

    }
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    [Editor.UCs2Cpp]
    public partial class UTestCs2CppBuilder
    {
        public List<Matrix> tmp;
        public Matrix[] tmp1;
        static UTestCs2CppBuilder()
        {
            InitCallbacks();
        }
        [Editor.UCs2Cpp]
        public unsafe int Func0(float a, int* b)
        {
            return *b + (int)a;
        }
        [Editor.UCs2Cpp]
        public unsafe string Func1(int a)
        {
            return a.ToString();
        }
        public void UnitTestEntrance()
        {
            var str = Rtti.UTypeDesc.TypeStr(typeof(List<Matrix>));
            var type = Rtti.UTypeDesc.TypeOf(str);

            str = Rtti.UTypeDesc.TypeStr(typeof(Matrix[]));
            type = Rtti.UTypeDesc.TypeOf(str);
            var args = new object[] { 8, };
            var obj = System.Activator.CreateInstance(type.SystemType, args) as Array;
            unsafe
            {
                var pMatrix = (Matrix*)System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(obj, 0).ToPointer();
                pMatrix[0].M21 = 5;
            }

            EngineNS.UCs2CppBase.UnitTest();
        }
    }
}