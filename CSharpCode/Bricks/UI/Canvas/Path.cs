using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Canvas
{
    public class TtPathStyle : AuxPtrType<EngineNS.Canvas.Path.FPathStyle>
    {
        static TtPathStyle mDefaultPathStyle = new TtPathStyle();
        public static TtPathStyle Default => mDefaultPathStyle;

        public TtPathStyle()
        {
            mCoreObject = EngineNS.Canvas.Path.FPathStyle.CreateInstance();
        }
        public float PathWidth
        {
            get
            {
                return mCoreObject.PathWidth;
            }
            set
            {
                mCoreObject.PathWidth = value;
            }
        }
        public float MiterLimit
        {
            get
            {
                return mCoreObject.MiterLimit;
            }
            set
            {
                mCoreObject.MiterLimit = value;
            }
        }
        public EngineNS.Canvas.EPathStrokeMode StrokeMode
        {
            get
            {
                return mCoreObject.StrokeMode;
            }
            set
            {
                mCoreObject.StrokeMode = value;
            }
        }
        public EngineNS.Canvas.EPathJoinMode JoinMode
        {
            get
            {
                return mCoreObject.JoinMode;
            }
            set
            {
                mCoreObject.JoinMode = value;
            }
        }
        public EngineNS.Canvas.EPathCapMode CapMode
        {
            get
            {
                return mCoreObject.CapMode;
            }
            set
            {
                mCoreObject.CapMode = value;
            }
        }
        public bool FillArea
        {
            get
            {
                return mCoreObject.FillArea;
            }
            set
            {
                mCoreObject.FillArea = value;
            }
        }
        public bool UvAlongPath
        {
            get
            {
                return mCoreObject.UvAlongPath;
            }
            set
            {
                mCoreObject.UvAlongPath = value;
            }
        }
        public EngineNS.Canvas.EPathPaintMode PaintMode
        {
            get
            {
                return mCoreObject.PaintMode;
            }
            set
            {
                mCoreObject.PaintMode = value;
            }
        }
        public float StrokeOffset
        {
            get
            {
                return mCoreObject.StrokeOffset;
            }
            set
            {
                mCoreObject.StrokeOffset = value;
            }
        }
        public bool StrokeAutoAlign
        {
            get
            {
                return mCoreObject.StrokeAutoAlign;
            }
            set
            {
                mCoreObject.StrokeAutoAlign = value;
            }
        }
        public void ResetStrokePattern()
        {
            mCoreObject.ResetStrokePattern();
        }
        public unsafe void PushStrokePattern(float[] pattern, int num = -1)
        {
            if (num < 0)
            {
                num = pattern.Length;
            }
            fixed (float* p = &pattern[0])
            {
                mCoreObject.PushStrokePattern(p, num);
            }
        }
        public void PushStrokePattern(Span<float> pattern)
        {
            mCoreObject.PushStrokePattern(ref pattern[0], pattern.Length);
        }
    }

    public class TtPath : AuxPtrType<EngineNS.Canvas.Path.FPath>
    {
        public TtPath()
        {
            mCoreObject = EngineNS.Canvas.Path.FPath.CreateInstance();
        }
        public void BeginPath()
        {
            mCoreObject.BeginPath();
        }
        public unsafe void PushCmd(EngineNS.Canvas.EPathCmdType CmdType, float* pArgs, uint NumOfArgs)
        {
            mCoreObject.PushCmd(CmdType, pArgs, NumOfArgs);
        }
        public unsafe void MoveTo(in Vector2 pos)
        {
            fixed(Vector2* pPos = &pos)
            {
                mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_MoveTo, (float*)pPos, 2);
            }
        }
        public unsafe void LineTo(in Vector2 pos)
        {
            fixed (Vector2* pPos = &pos)
            {
                mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_LineTo, (float*)pPos, 2);
            }
        }
        //if radius < half diameter(p1-p0) then radius = half diameter
        public unsafe void S_CCW_ArcTo(in Vector2 pos, float radius)
        {
            var arg = stackalloc float[5];
            arg[0] = pos.X;
            arg[1] = pos.Y;
            arg[2] = radius;
            arg[3] = radius;
            arg[4] = 0;
            mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_SCCWArcTo, (float*)arg, 5);
        }
        public unsafe void S_CW_ArcTo(in Vector2 pos, float radius)
        {
            var arg = stackalloc float[5];
            arg[0] = pos.X;
            arg[1] = pos.Y;
            arg[2] = radius;
            arg[3] = radius;
            arg[4] = 0;
            mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_SCWArcTo, (float*)arg, 5);
        }
        public unsafe void L_CCW_ArcTo(in Vector2 pos, float radius)
        {
            var arg = stackalloc float[5];
            arg[0] = pos.X;
            arg[1] = pos.Y;
            arg[2] = radius;
            arg[3] = radius;
            arg[4] = 0;
            mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_LCCWArcTo, (float*)arg, 5);
        }
        public unsafe void L_CW_ArcTo(in Vector2 pos, float radius)
        {
            var arg = stackalloc float[5];
            arg[0] = pos.X;
            arg[1] = pos.Y;
            arg[2] = radius;
            arg[3] = radius;
            arg[4] = 0;
            mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_LCWArcTo, (float*)arg, 5);
        }

        public unsafe void S_CCW_EllipseTo(in Vector2 pos, float radiusH, float radiusV, float rotation)
        {
            var arg = stackalloc float[5];
            arg[0] = pos.X;
            arg[1] = pos.Y;
            arg[2] = radiusH;
            arg[3] = radiusV;
            arg[4] = rotation;
            mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_SCCWArcTo, (float*)arg, 5);
        }
        public unsafe void S_CW_EllipseTo(in Vector2 pos, float radiusH, float radiusV, float rotation)
        {
            var arg = stackalloc float[5];
            arg[0] = pos.X;
            arg[1] = pos.Y;
            arg[2] = radiusH;
            arg[3] = radiusV;
            arg[4] = rotation;
            mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_SCWArcTo, (float*)arg, 5);
        }
        public unsafe void L_CCW_EllipseTo(in Vector2 pos, float radiusH, float radiusV, float rotation)
        {
            var arg = stackalloc float[5];
            arg[0] = pos.X;
            arg[1] = pos.Y;
            arg[2] = radiusH;
            arg[3] = radiusV;
            arg[4] = rotation;
            mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_LCCWArcTo, (float*)arg, 5);
        }
        public unsafe void L_CW_EllipseTo(in Vector2 pos, float radiusH, float radiusV, float rotation)
        {
            var arg = stackalloc float[5];
            arg[0] = pos.X;
            arg[1] = pos.Y;
            arg[2] = radiusH;
            arg[3] = radiusV;
            arg[4] = rotation;
            mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_LCWArcTo, (float*)arg, 5);
        }

        public unsafe void CubicTo(in Vector2 c0, in Vector2 c1, in Vector2 p1)
        {
            var arg = stackalloc Vector2[3];
            arg[0] = c0;
            arg[1] = c1;
            arg[2] = p1;
            mCoreObject.PushCmd(EngineNS.Canvas.EPathCmdType.Cmd_CubicTo, (float*)arg, 6);
        }
        public void Close()
        {
            mCoreObject.Close();
        }
        public void PushRect(in Vector2 Min, in Vector2 Max, float Round)
        {
            mCoreObject.PushRect(in Min, in Max, Round);
        }
        public void PushCircle(in Vector2 Center, float Radius)
        {
            mCoreObject.PushCircle(in Center, Radius);
        }
        public void EndPath(TtCanvasDrawCmdList DrawCmdList)
        {
            var outCmd = new EngineNS.Canvas.FSubDrawCmd();
            mCoreObject.EndPath(DrawCmdList.mCoreObject, ref outCmd);
        }
        public void EndPath(TtCanvasDrawCmdList DrawCmdList, ref EngineNS.Canvas.FSubDrawCmd outCmd)
        {
            mCoreObject.EndPath(DrawCmdList.mCoreObject, ref outCmd);
        }
    }
}
