using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class BezierControl
    {
        [Rtti.Meta]
        public List<BezierPointBase> BezierPoints { get; set; } = new List<BezierPointBase>();
        float mMinX = 0.0f;
        [Rtti.Meta]
        public float MinX
        {
            get => mMinX;
            set
            {
                if (value == mMinX || value >= mMaxX)
                    return;

                if (IsScaleValue)
                {
                    var deltaX = (mMaxX - value) / (mMaxX - mMinX);
                    for (int i = 0; i < BezierPoints.Count; i++)
                    {
                        var pt = BezierPoints[i];
                        pt.Position = new Vector2((pt.Position.X - mMinX) * deltaX + value, pt.Position.Y);
                        pt.ControlPoint = new Vector2((pt.ControlPoint.X - mMinX) * deltaX + value, pt.ControlPoint.Y);
                    }
                }
                mMinX = value;
                DefaultControlPointExtersion = (mMaxX - mMinX) * 0.1f;
            }
        }
        float mMinY = 0.0f;
        [Rtti.Meta]
        public float MinY 
        {
            get => mMinY;
            set
            {
                if (value == mMinY || value >= mMaxY)
                    return;

                if (IsScaleValue)
                {
                    var deltaY = (mMaxY - value) / (mMaxY - mMinY);
                    for (int i = 0; i < BezierPoints.Count; i++)
                    {
                        var pt = BezierPoints[i];
                        pt.Position = new Vector2(pt.Position.X, (pt.Position.Y - mMinY) * deltaY + value);
                        pt.ControlPoint = new Vector2(pt.ControlPoint.X, (pt.ControlPoint.Y - mMinY) * deltaY + value);
                    }
                }
                mMinY = value;
            }
        }
        float mMaxX = 1.0f;
        [Rtti.Meta]
        public float MaxX
        {
            get => mMaxX;
            set
            {
                if (value == mMaxX || value <= mMinX)
                    return;

                if (IsScaleValue)
                {
                    var deltaX = (value - mMinX) / (mMaxX - mMinX);
                    for (int i = 0; i < BezierPoints.Count; i++)
                    {
                        var pt = BezierPoints[i];
                        pt.Position = new Vector2((pt.Position.X - mMinX) * deltaX + mMinX, pt.Position.Y);
                        pt.ControlPoint = new Vector2((pt.ControlPoint.X - mMinX) * deltaX + mMinX, pt.ControlPoint.Y);
                    }
                }
                mMaxX = value;
                DefaultControlPointExtersion = (mMaxX - mMinX) * 0.1f;
            }
        }
        float mMaxY = 1.0f;
        [Rtti.Meta]
        public float MaxY 
        {
            get => mMaxY;
            set
            {
                if (value == mMaxY || value <= mMinY)
                    return;

                if (IsScaleValue)
                {
                    var deltaY = (value - mMinY) / (mMaxY - mMinY);
                    for (int i = 0; i < BezierPoints.Count; i++)
                    {
                        var pt = BezierPoints[i];
                        pt.Position = new Vector2(pt.Position.X, (pt.Position.Y - mMinY) * deltaY + mMinY);
                        pt.ControlPoint = new Vector2(pt.ControlPoint.X, (pt.ControlPoint.Y - mMinY) * deltaY + mMinY);
                    }
                }
                mMaxY = value;
            }
        }
        [Rtti.Meta]
        public Vector2 MinSize { get; set; } = new Vector2(50, 50);
        bool mShowGrid = true;
        [Rtti.Meta]
        public bool ShowGrid 
        {
            get => mShowGrid;
            set => mShowGrid = value;
        }
        float mGridRowCount = 10;
        [Rtti.Meta]
        public float GridRowCount
        {
            get => mGridRowCount;
            set => mGridRowCount = value;
        }
        float mGridColumnCount = 15;
        [Rtti.Meta]
        public float GridColumnCount
        {
            get => mGridColumnCount;
            set => mGridColumnCount = value;
        }
        bool mLockLinkedControlPoint = true;
        [Rtti.Meta]
        public bool LockLinkedControlPoint 
        {
            get => mLockLinkedControlPoint;
            set => mLockLinkedControlPoint = value;
        }

        [Rtti.Meta]
        public float ControlPointRadius { get; set; } = 4.0f;
        [Rtti.Meta]
        public float MaxControlPointRadius { get; set; } = 4.0f;
        [Rtti.Meta]
        public uint ControlPointColor { get; set; } = 0xFFFFFF00;
        [Rtti.Meta]
        public float ControlPointThickness { get; set; } = 1.5f;

        [Rtti.Meta]
        public float PointRadius { get; set; } = 6.0f;
        [Rtti.Meta]
        public float MaxPointRadius { get; set; } = 6.0f;
        [Rtti.Meta]
        public uint PointColor { get; set; } = 0xFF00FF00;
        [Rtti.Meta]
        public uint PointDeleteColor { get; set; } = 0xFF0000FF;
        [Rtti.Meta]
        public uint PointFocusColor { get; set; } = 0xFF00FFFF;
        [Rtti.Meta]
        public uint BezierColor { get; set; } = 0xFFE0E0E0;
        [Rtti.Meta]
        public float BezierThickness { get; set; } = 2.0f;

        public float DefaultControlPointExtersion { get; set; } = 0.1f;
        public bool IsScaleValue { get; set; } = false;
        public bool ScalePointRadiusWithSize = true;
        public float DesireWidth = 300;

        public void Initialize(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            var sizeY = maxY - minY;
            BezierPoints.Add(new BezierPointBase(new Vector2(minX, 0.5f * sizeY + minY), new Vector2(minX + DefaultControlPointExtersion * sizeY, 0.5f * sizeY + minY)));
            BezierPoints.Add(new BezierPointBase(new Vector2(maxX, 0.5f * sizeY + minY), new Vector2(maxX - DefaultControlPointExtersion * sizeY, 0.5f * sizeY + minY)));
        }

        float GetPositionXInCanvas(float bezierPointX, float canvasSizeX, float canvasMinX)
        {
            return (bezierPointX - MinX) / (MaxX - MinX) * canvasSizeX + canvasMinX;
        }
        float GetPositionYInCanvas(float bezierPointY, float canvasSizeY, float canvasMinY)
        {
            return (1 - ((bezierPointY - MinY) / (MaxY - MinY))) * canvasSizeY + canvasMinY;
        }
        float GetPositionXFromCanvas(float canvasX, float canvasSizeX, float canvasMinX)
        {
            return (canvasX - canvasMinX) / canvasSizeX * (MaxX - MinX) + MinX;
        }
        float GetPositionYFromCanvas(float canvasY, float canvasSizeY, float canvasMinY)
        {
            return (1 - ((canvasY - canvasMinY) / canvasSizeY)) * (MaxY - MinY) + MinY;
        }

        int mHoverPointIdx = -1;
        bool mIsHoverControlPoint = false;
        bool mRemovingPoint = false;
        public unsafe void OnDraw()
        {
            UIProxy.CheckBox.DrawCheckBox("Lock linked control point", ref mLockLinkedControlPoint, false);
            ImGuiAPI.SameLine(0, -1);
            UIProxy.CustomButton.ToolButton("?", new Vector2(24));
            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
            {
                ImGuiAPI.SetTooltip("How to use\r\n" +
                    "Add point: Double click to create point in mouse position\r\n" +
                    "Remove point: Drag point outside to remove it");
            }
            var border = new Vector2(15.0f);
            var canvasP0 = ImGuiAPI.GetCursorScreenPos() + border;
            var canvasSize = ImGuiAPI.GetContentRegionAvail() - border * 2;
            if (canvasSize.X < MinSize.X) canvasSize.X = MinSize.X;
            if (canvasSize.Y < MinSize.Y) canvasSize.Y = MinSize.Y;

            ImGuiAPI.InvisibleButton("canvas", canvasSize + border * 2, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft | ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonRight);

            OnDrawCanvas(in canvasP0, in canvasSize);
        }
        public unsafe void OnDrawCanvas(in Vector2 canvasP0, in Vector2 canvasSize)
        {
            var canvasP1 = canvasP0 + canvasSize;

            var tempPointRadius = PointRadius;
            var tempCtPointRadius = ControlPointRadius;
            if (ScalePointRadiusWithSize)
            {
                var delta = canvasSize.X / DesireWidth;
                tempPointRadius = System.Math.Min(PointRadius * delta, MaxPointRadius);
                tempCtPointRadius = System.Math.Min(tempCtPointRadius * delta, MaxControlPointRadius);
            }

            //bool isHovered = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            bool isHovered = false;
            var msPt = ImGuiAPI.GetMousePos();
            int Border = 2;
            if (msPt.X > (canvasP0.X - Border) && msPt.X < canvasP0.X + canvasSize.X + Border &&
                msPt.Y > (canvasP0.Y - Border) && msPt.Y < canvasP0.Y + canvasSize.Y + Border)
            {
                isHovered = true;
            }
            bool isActive = true;// ImGuiAPI.IsItemActive();

            var io = ImGuiAPI.GetIO();
            var drawList = ImGuiAPI.GetWindowDrawList();
            drawList.AddRectFilled(in canvasP0, in canvasP1, UIProxy.StyleConfig.Instance.PanelBackground, 0.0f, ImDrawFlags_.ImDrawFlags_None);
            if(mShowGrid)
            {
                var gridDelta = canvasP1 - canvasP0;
                var columnDelta = gridDelta.X / mGridColumnCount;
                for(var x = canvasP0.X; x < canvasP1.X; x += columnDelta)
                {
                    var p1 = new Vector2(x, canvasP0.Y);
                    var p2 = new Vector2(x, canvasP1.Y);
                    drawList.AddLine(in p1, in p2, UIProxy.StyleConfig.Instance.GridColor, 1.0f);
                }
                var rowDelta = gridDelta.Y / mGridRowCount;
                for(var y = canvasP0.Y; y < canvasP1.Y; y += rowDelta)
                {
                    var p1 = new Vector2(canvasP0.X, y);
                    var p2 = new Vector2(canvasP1.X, y);
                    drawList.AddLine(in p1, in p2, UIProxy.StyleConfig.Instance.GridColor, 1.0f);
                }
            }
            drawList.AddRect(in canvasP0, in canvasP1, 0xFFFFFFFF, 0.0f, ImDrawFlags_.ImDrawFlags_None, 1.0f);
            var sizeX = MaxX - MinX;
            var sizeY = MaxY - MinY;
            var mousePosInCanvas = io.MousePos;
            var controlPtRangeSq = tempCtPointRadius * tempCtPointRadius;
            var ptRangeSq = tempPointRadius * tempPointRadius;
            bool hoverInPoint = false;

            var isdragging = isActive && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 3.0f);
            if (!isdragging)
            {
                if (mRemovingPoint)
                {
                    BezierPoints.RemoveAt(mHoverPointIdx);
                    BezierPoints.RemoveAt(mHoverPointIdx + (mHoverPointIdx % 2) - 1);
                    mRemovingPoint = false;
                }
                mHoverPointIdx = -1;
            }
            else
            {
                int xxx = 0;
            }

            for (int i=0; i<BezierPoints.Count; i+=2)
            {
                var pt0 = BezierPoints[i];
                var ctPt0Pos = new Vector2(GetPositionXInCanvas(pt0.ControlPoint.X, canvasSize.X, canvasP0.X), GetPositionYInCanvas(pt0.ControlPoint.Y, canvasSize.Y, canvasP0.Y));
                var pt0Pos = new Vector2(GetPositionXInCanvas(pt0.Position.X, canvasSize.X, canvasP0.X), GetPositionYInCanvas(pt0.Position.Y, canvasSize.Y, canvasP0.Y));
                var pt1 = BezierPoints[i + 1];
                var ctPt1Pos = new Vector2(GetPositionXInCanvas(pt1.ControlPoint.X, canvasSize.X, canvasP0.X), GetPositionYInCanvas(pt1.ControlPoint.Y, canvasSize.Y, canvasP0.Y));
                var pt1Pos = new Vector2(GetPositionXInCanvas(pt1.Position.X, canvasSize.X, canvasP0.X), GetPositionYInCanvas(pt1.Position.Y, canvasSize.Y, canvasP0.Y));

                var ctPt0Color = ControlPointColor;
                var pt0Color = PointColor;
                var ctPt1Color = ControlPointColor;
                var pt1Color = PointColor;
                if(isHovered && !isdragging)
                {
                    var ctPt0Offset = ctPt0Pos - mousePosInCanvas;
                    if (ctPt0Offset.LengthSquared() < controlPtRangeSq)
                    {
                        ctPt0Color = PointFocusColor;
                        hoverInPoint = true;
                        mHoverPointIdx = i;
                        mIsHoverControlPoint = true;
                    }
                    else if ((pt0Pos - mousePosInCanvas).LengthSquared() < ptRangeSq)
                    {
                        pt0Color = PointFocusColor;
                        hoverInPoint = true;
                        mHoverPointIdx = i;
                        mIsHoverControlPoint = false;
                    }
                    else if ((ctPt1Pos - mousePosInCanvas).LengthSquared() < controlPtRangeSq)
                    {
                        ctPt1Color = PointFocusColor;
                        hoverInPoint = true;
                        mHoverPointIdx = i + 1;
                        mIsHoverControlPoint = true;
                    }
                    else if ((pt1Pos - mousePosInCanvas).LengthSquared() < ptRangeSq)
                    {
                        pt1Color = PointFocusColor;
                        hoverInPoint = true;
                        mHoverPointIdx = i + 1;
                        mIsHoverControlPoint = false;
                    }
                }
                if(mRemovingPoint)
                {
                    if(mHoverPointIdx == i)
                    {
                        pt0Color = PointDeleteColor;
                    }
                    else if((mHoverPointIdx + ((mHoverPointIdx % 2 == 0) ? -1 : 1)) == i)
                    {
                        pt0Color = PointDeleteColor;
                    }
                }

                drawList.AddBezierCubic(in pt0Pos, in ctPt0Pos, in ctPt1Pos, in pt1Pos, BezierColor, BezierThickness, 0);

                drawList.AddLine(in pt0Pos, in ctPt0Pos, ctPt0Color, 1.0f);
                drawList.AddCircleFilled(in pt0Pos, tempPointRadius, pt0Color, 0);
                drawList.AddCircleFilled(in ctPt0Pos, tempCtPointRadius, ctPt0Color, 0);

                drawList.AddLine(in pt1Pos, in ctPt1Pos, ctPt1Color, 1.0f);
                drawList.AddCircleFilled(pt1Pos, tempPointRadius, pt1Color, 0);
                drawList.AddCircleFilled(ctPt1Pos, tempCtPointRadius, ctPt1Color, 0);
            }
            if(isdragging && mHoverPointIdx >= 0 && mHoverPointIdx < BezierPoints.Count)
            {
                var pt = BezierPoints[mHoverPointIdx];
                var mousePosInBezier = new Vector2(GetPositionXFromCanvas(mousePosInCanvas.X, canvasSize.X, canvasP0.X),
                                                   GetPositionYFromCanvas(mousePosInCanvas.Y, canvasSize.Y, canvasP0.Y));
                if (mIsHoverControlPoint)
                {
                    var cpDir = pt.ControlPoint - pt.Position;
                    var tempVal = mHoverPointIdx % 2;
                    if(tempVal == 0)
                    {
                        mousePosInBezier.X = System.Math.Max(mousePosInBezier.X, pt.Position.X);
                        pt.ControlPoint = mousePosInBezier;
                    }
                    else
                    {
                        mousePosInBezier.X = System.Math.Min(mousePosInBezier.X, pt.Position.X);
                        pt.ControlPoint = mousePosInBezier;
                    }
                    if (mLockLinkedControlPoint && mHoverPointIdx > 0 && mHoverPointIdx < BezierPoints.Count - 1)
                    {
                        var extPt = BezierPoints[mHoverPointIdx + (mHoverPointIdx % 2 == 0 ? -1 : 1)];
                        var extPosInCanvas = new Vector2(GetPositionXInCanvas(extPt.Position.X, canvasSize.X, canvasP0.X),
                                                         GetPositionYInCanvas(extPt.Position.Y, canvasSize.Y, canvasP0.Y));
                        var extCpPosInCanvas = new Vector2(GetPositionXInCanvas(extPt.ControlPoint.X, canvasSize.X, canvasP0.X),
                                                           GetPositionYInCanvas(extPt.ControlPoint.Y, canvasSize.Y, canvasP0.Y));

                        var extCpDir = extPt.ControlPoint - extPt.Position;
                        var rot = Quaternion.GetQuaternionUp(new Vector3(cpDir.X, 0.0f, cpDir.Y), new Vector3(extCpDir.X, 0.0f, extCpDir.Y));
                        var newCpDir = pt.ControlPoint - pt.Position;
                        var newExtCpDir = Quaternion.RotateVector3(rot, new Vector3(newCpDir.X, 0.0f, newCpDir.Y));
                        var newExtCpDirTemp = new Vector2(newExtCpDir.X, newExtCpDir.Z);
                        newExtCpDirTemp.Normalize();
                        var newCPPos = newExtCpDirTemp + extPt.Position;
                        var newCPPosInCanvas = new Vector2(GetPositionXInCanvas(newCPPos.X, canvasSize.X, canvasP0.X),
                                                           GetPositionYInCanvas(newCPPos.Y, canvasSize.Y, canvasP0.Y));
                        var dirInCanvas = newCPPosInCanvas - extPosInCanvas;
                        dirInCanvas.Normalize();
                        var finalCPPosInCanvas = dirInCanvas * (extCpPosInCanvas - extPosInCanvas).Length() + extPosInCanvas;
                        var newCp = new Vector2(GetPositionXFromCanvas(finalCPPosInCanvas.X, canvasSize.X, canvasP0.X),
                                                GetPositionYFromCanvas(finalCPPosInCanvas.Y, canvasSize.Y, canvasP0.Y));
                        if (tempVal == 0)
                            newCp.X = System.Math.Min(newCp.X, extPt.Position.X);
                        else
                            newCp.X = System.Math.Max(newCp.X, extPt.Position.X);
                        extPt.ControlPoint = newCp;
                    }
                }
                else
                {
                    var pos = mousePosInBezier;
                    if (mHoverPointIdx == 0)
                    {
                        //pos = new Vector2(System.Math.Max(System.Math.Min(pt.Position.X, MaxX), MinX),
                        //                  System.Math.Max(System.Math.Min(mousePosInBezier.Y, MaxY), MinY));
                        pos.X = MinX;
                        pos.Y = CoreDefine.Clamp(mousePosInBezier.Y, MinY, MaxY);
                    }
                    else if(mHoverPointIdx == BezierPoints.Count - 1)
                    {
                        //pos = new Vector2(System.Math.Max(System.Math.Min(pt.Position.X, MaxX), MinX),
                        //                  System.Math.Max(System.Math.Min(mousePosInBezier.Y, MaxY), MinY));
                        pos.X = MaxX;
                        pos.Y = CoreDefine.Clamp(mousePosInBezier.Y, MinY, MaxY);
                    }
                    else
                    {
                        pos = new Vector2(System.Math.Max(System.Math.Min(mousePosInBezier.X, MaxX), MinX),
                                          System.Math.Max(System.Math.Min(mousePosInBezier.Y, MaxY), MinY));
                        var posInCanvas = new Vector2(GetPositionXInCanvas(pos.X, canvasSize.X, canvasP0.X),
                                                      GetPositionYInCanvas(pos.Y, canvasSize.Y, canvasP0.Y));
                        mRemovingPoint = (posInCanvas - mousePosInCanvas).LengthSquared() > 400;
                    }
                    var offSet = pos - pt.Position;
                    pt.Position = pos;
                    pt.ControlPoint += offSet;
                    if (mHoverPointIdx > 0 && mHoverPointIdx < BezierPoints.Count - 1)
                    {
                        BezierPointBase extPt = BezierPoints[mHoverPointIdx + (mHoverPointIdx % 2 == 0 ? -1 : 1)];
                        extPt.Position = pos;
                        extPt.ControlPoint += offSet;
                    }
                }
            }
            else if (isHovered && !isdragging && !hoverInPoint)
            {
                if(ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    var mousePosInBezier = new Vector2(GetPositionXFromCanvas(mousePosInCanvas.X, canvasSize.X, canvasP0.X),
                                                       GetPositionYFromCanvas(mousePosInCanvas.Y, canvasSize.Y, canvasP0.Y));
                    for(int i=0; i<BezierPoints.Count; i++)
                    {
                        if(BezierPoints[i].Position.X > mousePosInBezier.X)
                        {
                            BezierPoints.Insert(i, new BezierPointBase(new Vector2(mousePosInBezier.X, mousePosInBezier.Y), new Vector2(mousePosInBezier.X + DefaultControlPointExtersion, mousePosInBezier.Y)));
                            BezierPoints.Insert(i, new BezierPointBase(new Vector2(mousePosInBezier.X, mousePosInBezier.Y), new Vector2(mousePosInBezier.X - DefaultControlPointExtersion, mousePosInBezier.Y)));
                            break;
                        }
                    }
                }
            }
        }
    }
}
