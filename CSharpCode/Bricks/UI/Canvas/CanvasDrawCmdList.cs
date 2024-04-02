using EngineNS.Bricks.NodeGraph;
using EngineNS.Support;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.UI.Canvas
{
    public enum CanvasDrawRectType
    {
        Line,
        Fill,
    }

    public class TtCanvasDrawCmdList : AuxPtrType<EngineNS.Canvas.FCanvasDrawCmdList>
    {
        public TtCanvasDrawCmdList()
        {
            mCoreObject = EngineNS.Canvas.FCanvasDrawCmdList.CreateInstance();
        }

        public TtCanvasDrawCmdList(EngineNS.Canvas.FCanvasDrawCmdList self)
        {
            mCoreObject = self;
            Core_AddRef();
        }
        public void NewDrawCmd()
        {
            mCoreObject.NewDrawCmd();
        }
        public void PushMatrix(in Matrix matrix)
        {
            mCoreObject.PushMatrix(matrix);
        }
        public void PopMatrix()
        {
            mCoreObject.PopMatrix();
        }
        public void PushBrush(TtCanvasBrush brush)
        {
            mCoreObject.PushBrush(brush.mCoreObject);
        }
        public void PopBrush()
        {
            mCoreObject.PopBrush();
        }
        public void PushFont(Bricks.Font.TtFontSDF font)
        {
            mCoreObject.PushFont(font.mCoreObject);
        }
        public void PopFont()
        {
            mCoreObject.PopFont();
        }
        public void PushPathStyle(TtPathStyle pathStyle)
        {
            mCoreObject.PushPathStyle(pathStyle.mCoreObject);
        }
        public void PopPathStyle()
        {
            mCoreObject.PopPathStyle();
        }
        public void PushClip(in RectangleF rect)
        {
            mCoreObject.PushClip(rect);
        }
        public void PopClip()
        {
            mCoreObject.PopClip();
        }
        public void PushTransformIndex(in UInt16 index)
        {
            mCoreObject.PushTransformIndex(index);
        }
        public void PopTransformIndex()
        {
            mCoreObject.PopTransformIndex();
        }
        public void AddLine(in EngineNS.Vector2 start, in EngineNS.Vector2 end, float width, in EngineNS.Color color, ref EngineNS.Canvas.FSubDrawCmd outCmd)
        {
            mCoreObject.AddLine(in start, in end, width, in color, ref outCmd);
        }

        public void AddLineStrips(in EngineNS.Vector2 pPoints, uint num, float width, in EngineNS.Color color, bool loop, ref EngineNS.Canvas.FSubDrawCmd outCmd)
        {
            mCoreObject.AddLineStrips(in pPoints, num, width, in color, loop, ref outCmd);
        }

        public void AddImage(EngineNS.Canvas.ICanvasBrush image, float x, float y, float w, float h, in EngineNS.Color color, ref EngineNS.Canvas.FSubDrawCmd outCmd)
        {
            mCoreObject.AddImage(image, x, y, w, h, in color, ref outCmd);
        }
        public void AddRectFill(in RectangleF rect, in Vector4 cornerRadius, in EngineNS.Color color, ref EngineNS.Canvas.FSubDrawCmd outCmd)
        {
            mCoreObject.AddRectFill(rect, cornerRadius, color, ref outCmd);
        }
        public void AddRectFill(in EngineNS.Vector2 start, in EngineNS.Vector2 end, in Vector4 cornerRadius, in EngineNS.Color color, ref EngineNS.Canvas.FSubDrawCmd outCmd)
        {
            mCoreObject.AddRectFill(start, end, cornerRadius, color, ref outCmd);
        }
        public void AddRect(in RectangleF rect, in Thickness thickness, in Vector4 cornerRadius, in EngineNS.Color color, ref EngineNS.Canvas.FSubDrawCmd outCmd)
        {
            mCoreObject.AddRectLine(rect, thickness, cornerRadius, color, ref outCmd);
        }
        public void AddRect(in EngineNS.Vector2 start, in EngineNS.Vector2 end, in Thickness thickness, in Vector4 cornerRadius, in EngineNS.Color color, ref EngineNS.Canvas.FSubDrawCmd outCmd)
        {
            mCoreObject.AddRectLine(start, end, thickness, cornerRadius, color, ref outCmd);
        }
        public void AddRect(in EngineNS.Vector2 start, in EngineNS.Vector2 end, float width, in EngineNS.Color color,  CanvasDrawRectType DrawRectType, ref EngineNS.Canvas.FSubDrawCmd outCmd)
        {
            if (DrawRectType == CanvasDrawRectType.Line)
            {
                mCoreObject.AddRectLine(in start, in end, width, in color, ref outCmd);
            }
            else
            {
                mCoreObject.AddRectFill(in start, in end, in color, ref outCmd);
            }
        }
        public unsafe void AddText(string text, float x, float y, in EngineNS.Canvas.FDrawCmdInstanceData data)
        {
#if PWindow
            fixed (char* p = text)
            fixed (EngineNS.Canvas.FDrawCmdInstanceData* pData = &data)
            {
                mCoreObject.AddText((wchar_t*)p, text.Length, x, y, pData, new IBlobObject());
            }
#else
            fixed (char* p = text)
            fixed (EngineNS.Canvas.FDrawCmdInstanceData* pData = &data)
            {
                using (var buffer = BigStackBuffer.CreateInstance(text.Length * sizeof(wchar_t)))
                {
                    var numOfUtf32 = System.Text.Encoding.UTF32.GetBytes(p, text.Length, (byte*)buffer.GetBuffer(), text.Length * sizeof(wchar_t));
                    mCoreObject.AddText((wchar_t*)buffer.GetBuffer(), numOfUtf32, x, y, pData, new IBlobObject());
                }
            }
#endif
        }
        public unsafe void AddText(string text, float x, float y, in EngineNS.Canvas.FDrawCmdInstanceData data, UBlobObject outCmds)
        {
#if PWindow
            fixed (char* p = text)
            fixed (EngineNS.Canvas.FDrawCmdInstanceData* pData = &data)
            {
                mCoreObject.AddText((wchar_t*)p, text.Length, x, y, pData, outCmds.mCoreObject);
            }
#else
            fixed (char* p = text)
            fixed (EngineNS.Canvas.FDrawCmdInstanceData* pData = &data)
            {
                using (var buffer = BigStackBuffer.CreateInstance(text.Length * sizeof(wchar_t)))
                {
                    var numOfUtf32 = System.Text.Encoding.UTF32.GetBytes(p, text.Length, (byte*)buffer.GetBuffer(), text.Length * sizeof(wchar_t));
                    mCoreObject.AddText((wchar_t*)buffer.GetBuffer(), numOfUtf32, x, y, pData, outCmds.mCoreObject);
                }
            }
#endif
        }
    }
}
