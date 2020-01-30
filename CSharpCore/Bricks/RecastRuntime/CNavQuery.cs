using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.Support;

namespace EngineNS.Bricks.RecastRuntime
{
    public class CNavQuery : AuxCoreObject<CNavQuery.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        static bool[] AreaType = new bool[16];
        public static void UseAreaType(Byte value)
        {
            if (value > 16 && value < 1)
                return;

            AreaType[value] = true;
        }

        public static void UnUseAreaType(Byte value)
        {
            if (value > 16 && value < 1)
                return;

            AreaType[value] = false;
        }

        public CNavQuery(NativePointer self)
        {
            mCoreObject = self;
        }

        public ushort GetIncludeFlags()
        {
            return SDK_RcNavQuery_GetIncludeFlags(mCoreObject);
        }
        public void SetIncludeFlags(ushort flags)
        {
            SDK_RcNavQuery_SetIncludeFlags(mCoreObject, flags);
        }
        public ushort GetExcludeFlags()
        {
            return SDK_RcNavQuery_GetExcludeFlags(mCoreObject);
        }
        public void SetExcludeFlags(ushort flags)
        {
            SDK_RcNavQuery_SetExcludeFlags(mCoreObject, flags);
        }
        public bool GetHeight(Vector3 pos, Vector3 ext, ref float h)
        {
            unsafe
            {
                return SDK_RcNavQuery_GetHeight(mCoreObject, &pos, &ext, ref h);
            }
            
        }

        public bool FindStraightPath(Vector3 start, Vector3 end, Support.CBlobObject blob)
        {
            unsafe
            {
                return SDK_RcNavQuery_FindStraightPath(mCoreObject, &start, &end, blob.CoreObject);
            }
        }
        public Vector3[] FindStraightPath(Vector3 start, Vector3 end)
        {
            using (var blob = Support.CBlobProxy2.CreateBlobProxy())
            {
                unsafe
                {
                    if (SDK_RcNavQuery_FindStraightPath(mCoreObject, &start, &end, blob.CoreObject))
                    {
                        blob.BeginRead();
                        int count = (int)blob.DataLength / sizeof(Vector3);
                        Vector3[] result = new Vector3[count];
                        fixed (Vector3* p = &result[0])
                        {
                            blob.ReadPtr(p, sizeof(Vector3) * count);
                        }
                        return result;
                    }
                    return null;
                }
            }
        }
        public int FindStraightPath(Vector3 start, Vector3 end, Vector3[] points, ref int count)
        {
            using (var blob = Support.CBlobProxy2.CreateBlobProxy())
            {
                unsafe
                {
                    if (SDK_RcNavQuery_FindStraightPath(mCoreObject, &start, &end, blob.CoreObject))
                    {
                        blob.BeginRead();
                        count = (int)blob.DataLength / sizeof(Vector3);
                        if(points==null)
                        {
                            count = 0;
                            return (int)blob.DataLength / sizeof(Vector3);
                        }
                        if (points.Length < count)
                        {
                            fixed (Vector3* p = &points[0])
                            {
                                blob.ReadPtr(p, sizeof(Vector3) * points.Length);
                            }
                            count = points.Length;
                        }
                        else
                        {
                            fixed (Vector3* p = &points[0])
                            {
                                blob.ReadPtr(p, sizeof(Vector3) * count);
                            }
                        }
                    }
                    return (int)blob.DataLength / sizeof(Vector3);
                }
            } 
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static ushort SDK_RcNavQuery_GetIncludeFlags(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RcNavQuery_SetIncludeFlags(NativePointer self, ushort flags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static ushort SDK_RcNavQuery_GetExcludeFlags(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RcNavQuery_SetExcludeFlags(NativePointer self, ushort flags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_RcNavQuery_GetHeight(NativePointer self, Vector3* pos, Vector3* ext, ref float h);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_RcNavQuery_FindStraightPath(NativePointer self, Vector3* start, Vector3* end, Support.CBlobObject.NativePointer blob);
        #endregion
    }
}

namespace EngineNS.GamePlay.SceneGraph
{
    public partial class GSceneGraph
    {
        public Bricks.RecastRuntime.CNavQuery NavQuery = null;
    }
}

