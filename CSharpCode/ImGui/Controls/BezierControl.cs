using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class BezierControl
    {
        [Rtti.Meta("")]
        public List<BezierPointBase> BezierPoints { get; set; } = new List<BezierPointBase>();
        float mMinX = 0.0f;
        [Rtti.Meta("")]
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
                DefaultControlPointExtension = (mMaxX - mMinX) * 0.1f;
                ResetView();
            }
        }
        float mMinY = 0.0f;
        [Rtti.Meta("")]
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
                ResetView();
            }
        }
        float mMaxX = 1.0f;
        [Rtti.Meta("")]
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
                DefaultControlPointExtension = (mMaxX - mMinX) * 0.1f;
                ResetView();
            }
        }
        float mMaxY = 1.0f;
        [Rtti.Meta("")]
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
                ResetView();
            }
        }
        [Rtti.Meta("")]
        public Vector2 MinSize { get; set; } = new Vector2(50, 50);
        bool mShowGrid = true;
        [Rtti.Meta("")]
        public bool ShowGrid 
        {
            get => mShowGrid;
            set => mShowGrid = value;
        }
        float mGridRowCount = 10;
        [Rtti.Meta("")]
        public float GridRowCount
        {
            get => mGridRowCount;
            set => mGridRowCount = value;
        }
        float mGridColumnCount = 15;
        [Rtti.Meta("")]
        public float GridColumnCount
        {
            get => mGridColumnCount;
            set => mGridColumnCount = value;
        }
        bool mLockLinkedControlPoint = true;
        [Rtti.Meta("")]
        public bool LockLinkedControlPoint 
        {
            get => mLockLinkedControlPoint;
            set => mLockLinkedControlPoint = value;
        }

        [Rtti.Meta("")]
        public float ControlPointRadius { get; set; } = 4.0f;
        [Rtti.Meta("")]
        public float MaxControlPointRadius { get; set; } = 4.0f;
        [Rtti.Meta("")]
        public uint ControlPointColor { get; set; } = 0xFFFFFF00;
        [Rtti.Meta("")]
        public float ControlPointThickness { get; set; } = 1.5f;

        [Rtti.Meta("")]
        public float PointRadius { get; set; } = 6.0f;
        [Rtti.Meta("")]
        public float MaxPointRadius { get; set; } = 6.0f;
        [Rtti.Meta("")]
        public uint PointColor { get; set; } = 0xFF00FF00;
        [Rtti.Meta("")]
        public uint PointDeleteColor { get; set; } = 0xFF0000FF;
        [Rtti.Meta("")]
        public uint PointFocusColor { get; set; } = 0xFF00FFFF;
        [Rtti.Meta("")]
        public uint BezierColor { get; set; } = 0xFFE0E0E0;
        [Rtti.Meta("")]
        public float BezierThickness { get; set; } = 2.0f;

        public float DefaultControlPointExtension { get; set; } = 0.1f;
        public bool IsScaleValue { get; set; } = false;
        public bool ScalePointRadiusWithSize = true;
        public float DesireWidth = 300;

        public Action<string, Action, Action> HistoryRecorder { get; set; }

        bool mViewInitialized;
        float mViewMinX;
        float mViewMaxX;
        float mViewMinY;
        float mViewMaxY;
        int mSelectedPointIdx = -1;
        bool mSelectedControlPoint;
        bool mCurveDragActive;
        List<BezierPointBase> mCurveDragBefore;
        List<BezierPointBase> mNumericEditBefore;

        enum ESelectedValue
        {
            PositionX,
            PositionY,
            ControlX,
            ControlY,
        }

        public void Initialize(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            var sizeY = maxY - minY;
            BezierPoints.Add(new BezierPointBase(new Vector2(minX, 0.5f * sizeY + minY), new Vector2(minX + DefaultControlPointExtension * sizeY, 0.5f * sizeY + minY)));
            BezierPoints.Add(new BezierPointBase(new Vector2(maxX, 0.5f * sizeY + minY), new Vector2(maxX - DefaultControlPointExtension * sizeY, 0.5f * sizeY + minY)));
        }

        void EnsureViewRange()
        {
            if (mViewInitialized)
                return;
            mViewMinX = MinX;
            mViewMaxX = MaxX;
            mViewMinY = MinY;
            mViewMaxY = MaxY;
            mViewInitialized = true;
        }

        public void ResetView()
        {
            mViewInitialized = false;
        }

        float GetPositionXInCanvas(float bezierPointX, float canvasSizeX, float canvasMinX)
        {
            return (bezierPointX - mViewMinX) / (mViewMaxX - mViewMinX) * canvasSizeX + canvasMinX;
        }
        float GetPositionYInCanvas(float bezierPointY, float canvasSizeY, float canvasMinY)
        {
            return (1 - ((bezierPointY - mViewMinY) / (mViewMaxY - mViewMinY))) * canvasSizeY + canvasMinY;
        }
        float GetPositionXFromCanvas(float canvasX, float canvasSizeX, float canvasMinX)
        {
            return (canvasX - canvasMinX) / canvasSizeX * (mViewMaxX - mViewMinX) + mViewMinX;
        }
        float GetPositionYFromCanvas(float canvasY, float canvasSizeY, float canvasMinY)
        {
            return (1 - ((canvasY - canvasMinY) / canvasSizeY)) * (mViewMaxY - mViewMinY) + mViewMinY;
        }

        static List<BezierPointBase> ClonePoints(List<BezierPointBase> source)
        {
            var result = new List<BezierPointBase>();
            if (source == null)
                return result;
            for (int i = 0; i < source.Count; i++)
            {
                var point = source[i];
                result.Add(point == null ? null : new BezierPointBase(point.Position, point.ControlPoint));
            }
            return result;
        }

        static bool PointsEqual(List<BezierPointBase> left, List<BezierPointBase> right)
        {
            if (left == null || right == null || left.Count != right.Count)
                return false;
            for (int i = 0; i < left.Count; i++)
            {
                if (left[i] == null || right[i] == null)
                {
                    if (left[i] != right[i])
                        return false;
                    continue;
                }
                if (left[i].Position != right[i].Position || left[i].ControlPoint != right[i].ControlPoint)
                    return false;
            }
            return true;
        }

        void RestorePoints(List<BezierPointBase> snapshot)
        {
            BezierPoints ??= new List<BezierPointBase>();
            BezierPoints.Clear();
            BezierPoints.AddRange(ClonePoints(snapshot));
            if (mSelectedPointIdx >= BezierPoints.Count)
                mSelectedPointIdx = -1;
        }

        void CommitCurveEdit(string name, List<BezierPointBase> before)
        {
            var after = ClonePoints(BezierPoints);
            if (before == null || PointsEqual(before, after))
                return;
            var undoSnapshot = ClonePoints(before);
            var redoSnapshot = ClonePoints(after);
            HistoryRecorder?.Invoke(name,
                () => RestorePoints(redoSnapshot),
                () => RestorePoints(undoSnapshot));
        }

        void SetAnchorPosition(int index, in Vector2 requestedPosition)
        {
            if (index < 0 || index >= BezierPoints.Count)
                return;
            var position = requestedPosition;
            position.Y = MathHelper.Clamp(position.Y, MinY, MaxY);
            if (index == 0)
                position.X = MinX;
            else if (index == BezierPoints.Count - 1)
                position.X = MaxX;
            else
            {
                int leftCopy = (index & 1) == 1 ? index : index - 1;
                int rightCopy = leftCopy + 1;
                position.X = MathHelper.Clamp(position.X,
                    BezierPoints[leftCopy - 1].Position.X,
                    BezierPoints[rightCopy + 1].Position.X);
                var leftPoint = BezierPoints[leftCopy];
                var rightPoint = BezierPoints[rightCopy];
                var leftOffset = position - leftPoint.Position;
                var rightOffset = position - rightPoint.Position;
                leftPoint.Position = position;
                leftPoint.ControlPoint += leftOffset;
                rightPoint.Position = position;
                rightPoint.ControlPoint += rightOffset;
                return;
            }
            var point = BezierPoints[index];
            var offset = position - point.Position;
            point.Position = position;
            point.ControlPoint += offset;
        }

        void SetControlPosition(int index, in Vector2 requestedPosition)
        {
            if (index < 0 || index >= BezierPoints.Count)
                return;
            var position = requestedPosition;
            var point = BezierPoints[index];
            if ((index & 1) == 0)
            {
                float upper = index + 1 < BezierPoints.Count ?
                    BezierPoints[index + 1].ControlPoint.X : point.Position.X;
                position.X = MathHelper.Clamp(position.X, point.Position.X,
                    Math.Max(point.Position.X, upper));
            }
            else
            {
                float lower = index > 0 ? BezierPoints[index - 1].ControlPoint.X : point.Position.X;
                position.X = MathHelper.Clamp(position.X,
                    Math.Min(lower, point.Position.X), point.Position.X);
            }
            point.ControlPoint = position;
        }

        void ApplySelectedValue(ESelectedValue field, float value)
        {
            if (!float.IsFinite(value) || mSelectedPointIdx < 0 || mSelectedPointIdx >= BezierPoints.Count)
                return;
            var point = BezierPoints[mSelectedPointIdx];
            switch (field)
            {
                case ESelectedValue.PositionX:
                    SetAnchorPosition(mSelectedPointIdx, new Vector2(value, point.Position.Y));
                    break;
                case ESelectedValue.PositionY:
                    SetAnchorPosition(mSelectedPointIdx, new Vector2(point.Position.X, value));
                    break;
                case ESelectedValue.ControlX:
                    SetControlPosition(mSelectedPointIdx, new Vector2(value, point.ControlPoint.Y));
                    break;
                case ESelectedValue.ControlY:
                    SetControlPosition(mSelectedPointIdx, new Vector2(point.ControlPoint.X, value));
                    break;
            }
        }

        unsafe void DrawSelectedValue(string label, ESelectedValue field, float value)
        {
            var before = ClonePoints(BezierPoints);
            ImGuiAPI.SetNextItemWidth(120.0f);
            if (ImGuiAPI.InputFloat(label, ref value, 0.0f, 0.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None))
            {
                if (mNumericEditBefore == null)
                    mNumericEditBefore = before;
                ApplySelectedValue(field, value);
            }
            if (ImGuiAPI.IsItemActivated())
                mNumericEditBefore = before;
            if (ImGuiAPI.IsItemDeactivatedAfterEdit())
            {
                CommitCurveEdit("Edit Bezier Value", mNumericEditBefore ?? before);
                mNumericEditBefore = null;
            }
        }

        unsafe void DrawSelectedPointEditor()
        {
            if (mSelectedPointIdx < 0 || mSelectedPointIdx >= BezierPoints.Count)
                return;
            var point = BezierPoints[mSelectedPointIdx];
            string id = GetHashCode().ToString();
            DrawSelectedValue($"{TtLocalization.Tr("X")}##BezierX_{id}", ESelectedValue.PositionX, point.Position.X);
            DrawSelectedValue($"{TtLocalization.Tr("Y")}##BezierY_{id}", ESelectedValue.PositionY, point.Position.Y);
            DrawSelectedValue($"{TtLocalization.Tr("Handle X")}##BezierControlX_{id}", ESelectedValue.ControlX, point.ControlPoint.X);
            DrawSelectedValue($"{TtLocalization.Tr("Handle Y")}##BezierControlY_{id}", ESelectedValue.ControlY, point.ControlPoint.Y);
        }

        void HandleViewNavigation(bool isHovered, in Vector2 canvasP0, in Vector2 canvasSize)
        {
            if (!isHovered || canvasSize.X <= 0.0f || canvasSize.Y <= 0.0f)
                return;
            var io = ImGuiAPI.GetIO();
            if (io.MouseWheel != 0.0f)
            {
                float xRange = mViewMaxX - mViewMinX;
                float yRange = mViewMaxY - mViewMinY;
                float xRatio = MathHelper.Clamp((io.MousePos.X - canvasP0.X) / canvasSize.X, 0.0f, 1.0f);
                float yRatio = MathHelper.Clamp(1.0f - (io.MousePos.Y - canvasP0.Y) / canvasSize.Y, 0.0f, 1.0f);
                float mouseX = mViewMinX + xRange * xRatio;
                float mouseY = mViewMinY + yRange * yRatio;
                float scale = (float)Math.Pow(0.85, io.MouseWheel);
                float sourceXRange = Math.Max(1e-5f, MaxX - MinX);
                float sourceYRange = Math.Max(1e-5f, MaxY - MinY);
                float minXRange = sourceXRange * 1e-4f;
                float minYRange = sourceYRange * 1e-4f;
                float newXRange = MathHelper.Clamp(xRange * scale, minXRange, sourceXRange * 1e4f);
                float newYRange = MathHelper.Clamp(yRange * scale, minYRange, sourceYRange * 1e4f);
                mViewMinX = mouseX - newXRange * xRatio;
                mViewMaxX = mViewMinX + newXRange;
                mViewMinY = mouseY - newYRange * yRatio;
                mViewMaxY = mViewMinY + newYRange;
            }
            if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Middle, 0.0f))
            {
                var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Middle, 0.0f);
                ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Middle);
                float xRange = mViewMaxX - mViewMinX;
                float yRange = mViewMaxY - mViewMinY;
                float offsetX = delta.X / canvasSize.X * xRange;
                float offsetY = delta.Y / canvasSize.Y * yRange;
                mViewMinX -= offsetX;
                mViewMaxX -= offsetX;
                mViewMinY += offsetY;
                mViewMaxY += offsetY;
            }
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
                ImGuiAPI.SetTooltip(TtLocalization.Tr("How to use\r\n") +
                    TtLocalization.Tr("Add point: Double click to create point in mouse position\r\n") +
                    TtLocalization.Tr("Remove point: Drag point outside to remove it\r\n") +
                    TtLocalization.Tr("Pan view: Drag with middle mouse button\r\n") +
                    TtLocalization.Tr("Zoom view: Use the mouse wheel\r\n") +
                    TtLocalization.Tr("Precise edit: Right click a point or handle"));
            }
            DrawSelectedPointEditor();
            var border = new Vector2(15.0f);
            var canvasP0 = ImGuiAPI.GetCursorScreenPos() + border;
            var canvasSize = ImGuiAPI.GetContentRegionAvail() - border * 2;
            if (canvasSize.X < MinSize.X) canvasSize.X = MinSize.X;
            if (canvasSize.Y < MinSize.Y) canvasSize.Y = MinSize.Y;

            ImGuiAPI.InvisibleButton(TtLocalization.Tr("canvas"), canvasSize + border * 2, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft | ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonRight | ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonMiddle);

            OnDrawCanvas(in canvasP0, in canvasSize);
        }
        public unsafe void OnDrawCanvas(in Vector2 canvasP0, in Vector2 canvasSize)
        {
            if (canvasSize.X <= 0.0f || canvasSize.Y <= 0.0f)
                return;
            EnsureViewRange();
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
            int border = 2;
            if (msPt.X > (canvasP0.X - border) && msPt.X < canvasP0.X + canvasSize.X + border &&
                msPt.Y > (canvasP0.Y - border) && msPt.Y < canvasP0.Y + canvasSize.Y + border)
            {
                isHovered = true;
            }
            bool isActive = true;// ImGuiAPI.IsItemActive();

            BezierPoints ??= new List<BezierPointBase>();
            HandleViewNavigation(isHovered, in canvasP0, in canvasSize);
            var io = ImGuiAPI.GetIO();
            var drawList = ImGuiAPI.GetWindowDrawList();
            drawList.AddRectFilled(in canvasP0, in canvasP1, UIProxy.StyleConfig.Instance.PanelBackground, 0.0f, ImDrawFlags_.ImDrawFlags_None);
            if(mShowGrid)
            {
                var columnDelta = canvasSize.X / mGridColumnCount;
                for(var x = canvasP0.X; x < canvasP1.X; x += columnDelta)
                {
                    var p1 = new Vector2(x, canvasP0.Y);
                    var p2 = new Vector2(x, canvasP1.Y);
                    drawList.AddLine(in p1, in p2, UIProxy.StyleConfig.Instance.GridColor, 1.0f);
                }
                var rowDelta = canvasSize.Y / mGridRowCount;
                for(var y = canvasP0.Y; y < canvasP1.Y; y += rowDelta)
                {
                    var p1 = new Vector2(canvasP0.X, y);
                    var p2 = new Vector2(canvasP1.X, y);
                    drawList.AddLine(in p1, in p2, UIProxy.StyleConfig.Instance.GridColor, 1.0f);
                }
            }
            drawList.AddRect(in canvasP0, in canvasP1, 0xFFFFFFFF, 0.0f, ImDrawFlags_.ImDrawFlags_None, 1.0f);
            var mousePosInCanvas = io.MousePos;
            var controlPtRangeSq = tempCtPointRadius * tempCtPointRadius;
            var ptRangeSq = tempPointRadius * tempPointRadius;
            bool hoverInPoint = false;

            var isdragging = isActive && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 3.0f);
            if (isdragging && !mCurveDragActive && mHoverPointIdx >= 0 && mHoverPointIdx < BezierPoints.Count)
            {
                mCurveDragBefore = ClonePoints(BezierPoints);
                mCurveDragActive = true;
                mSelectedPointIdx = mHoverPointIdx;
                mSelectedControlPoint = mIsHoverControlPoint;
            }
            if (!isdragging)
            {
                if (mCurveDragActive)
                {
                    if (mRemovingPoint && mHoverPointIdx > 0 && mHoverPointIdx < BezierPoints.Count - 1)
                    {
                        int leftCopy = (mHoverPointIdx & 1) == 1 ? mHoverPointIdx : mHoverPointIdx - 1;
                        BezierPoints.RemoveAt(leftCopy + 1);
                        BezierPoints.RemoveAt(leftCopy);
                        mSelectedPointIdx = -1;
                    }
                    CommitCurveEdit(mRemovingPoint ? "Remove Bezier Point" : "Move Bezier Point", mCurveDragBefore);
                    mCurveDragBefore = null;
                    mCurveDragActive = false;
                }
                mRemovingPoint = false;
                mHoverPointIdx = -1;
            }

            for (int i = 0; i + 1 < BezierPoints.Count; i += 2)
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
                if (mSelectedPointIdx == i)
                {
                    if (mSelectedControlPoint)
                        ctPt0Color = PointFocusColor;
                    else
                        pt0Color = PointFocusColor;
                }
                else if (mSelectedPointIdx == i + 1)
                {
                    if (mSelectedControlPoint)
                        ctPt1Color = PointFocusColor;
                    else
                        pt1Color = PointFocusColor;
                }
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

            string valuePopupName = $"BezierValueEditor##{GetHashCode()}";
            if (isHovered && !isdragging && hoverInPoint)
            {
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
                {
                    mSelectedPointIdx = mHoverPointIdx;
                    mSelectedControlPoint = mIsHoverControlPoint;
                }
                if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false))
                {
                    mSelectedPointIdx = mHoverPointIdx;
                    mSelectedControlPoint = mIsHoverControlPoint;
                    ImGuiAPI.OpenPopup(valuePopupName, ImGuiPopupFlags_.ImGuiPopupFlags_None);
                }
            }

            if(isdragging && mHoverPointIdx >= 0 && mHoverPointIdx < BezierPoints.Count)
            {
                var pt = BezierPoints[mHoverPointIdx];
                var mousePosInBezier = new Vector2(GetPositionXFromCanvas(mousePosInCanvas.X, canvasSize.X, canvasP0.X),
                                                   GetPositionYFromCanvas(mousePosInCanvas.Y, canvasSize.Y, canvasP0.Y));
                if (mIsHoverControlPoint)
                {
                    SetControlPosition(mHoverPointIdx, in mousePosInBezier);
                    if (mLockLinkedControlPoint && mHoverPointIdx > 0 && mHoverPointIdx < BezierPoints.Count - 1)
                    {
                        int linkedIndex = mHoverPointIdx + ((mHoverPointIdx & 1) == 0 ? -1 : 1);
                        var linkedPoint = BezierPoints[linkedIndex];
                        var anchorInCanvas = new Vector2(GetPositionXInCanvas(pt.Position.X, canvasSize.X, canvasP0.X),
                            GetPositionYInCanvas(pt.Position.Y, canvasSize.Y, canvasP0.Y));
                        var controlInCanvas = new Vector2(GetPositionXInCanvas(pt.ControlPoint.X, canvasSize.X, canvasP0.X),
                            GetPositionYInCanvas(pt.ControlPoint.Y, canvasSize.Y, canvasP0.Y));
                        var linkedControlInCanvas = new Vector2(GetPositionXInCanvas(linkedPoint.ControlPoint.X, canvasSize.X, canvasP0.X),
                            GetPositionYInCanvas(linkedPoint.ControlPoint.Y, canvasSize.Y, canvasP0.Y));
                        var oppositeDirection = anchorInCanvas - controlInCanvas;
                        float directionLength = oppositeDirection.Length();
                        if (directionLength > 1e-5f)
                        {
                            oppositeDirection /= directionLength;
                            float linkedLength = (linkedControlInCanvas - anchorInCanvas).Length();
                            var newLinkedControlInCanvas = anchorInCanvas + oppositeDirection * linkedLength;
                            var newLinkedControl = new Vector2(
                                GetPositionXFromCanvas(newLinkedControlInCanvas.X, canvasSize.X, canvasP0.X),
                                GetPositionYFromCanvas(newLinkedControlInCanvas.Y, canvasSize.Y, canvasP0.Y));
                            SetControlPosition(linkedIndex, in newLinkedControl);
                        }
                    }
                }
                else
                {
                    var position = mousePosInBezier;
                    if (mHoverPointIdx > 0 && mHoverPointIdx < BezierPoints.Count - 1)
                    {
                        position.X = MathHelper.Clamp(position.X, MinX, MaxX);
                        position.Y = MathHelper.Clamp(position.Y, MinY, MaxY);
                        var positionInCanvas = new Vector2(GetPositionXInCanvas(position.X, canvasSize.X, canvasP0.X),
                            GetPositionYInCanvas(position.Y, canvasSize.Y, canvasP0.Y));
                        mRemovingPoint = (positionInCanvas - mousePosInCanvas).LengthSquared() > 400.0f;
                    }
                    SetAnchorPosition(mHoverPointIdx, in position);
                }
            }
            else if (isHovered && !isdragging && !hoverInPoint)
            {
                if(ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    var mousePosInBezier = new Vector2(GetPositionXFromCanvas(mousePosInCanvas.X, canvasSize.X, canvasP0.X),
                                                       GetPositionYFromCanvas(mousePosInCanvas.Y, canvasSize.Y, canvasP0.Y));
                    mousePosInBezier.X = MathHelper.Clamp(mousePosInBezier.X, MinX, MaxX);
                    mousePosInBezier.Y = MathHelper.Clamp(mousePosInBezier.Y, MinY, MaxY);
                    for(int i = 1; i < BezierPoints.Count; i++)
                    {
                        if(BezierPoints[i].Position.X > mousePosInBezier.X)
                        {
                            var before = ClonePoints(BezierPoints);
                            BezierPoints.Insert(i, new BezierPointBase(mousePosInBezier,
                                new Vector2(mousePosInBezier.X + DefaultControlPointExtension, mousePosInBezier.Y)));
                            BezierPoints.Insert(i, new BezierPointBase(mousePosInBezier,
                                new Vector2(mousePosInBezier.X - DefaultControlPointExtension, mousePosInBezier.Y)));
                            mSelectedPointIdx = i;
                            mSelectedControlPoint = false;
                            CommitCurveEdit("Add Bezier Point", before);
                            break;
                        }
                    }
                }
            }

            if (ImGuiAPI.BeginPopup(valuePopupName, ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize))
            {
                DrawSelectedPointEditor();
                ImGuiAPI.EndPopup();
            }
        }
    }
}
