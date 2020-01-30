using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Controls.Containers
{
   
    public class CanvasSlotOperator : ISlotOperator
    {
        enum enCanvasAnchors
        {
            Unknow = -1,
            Center = 0,
            LeftTop = 1,
            RightTop = 2,
            RightBottom = 3,
            LeftBottom = 4,

            Left = 5,
            Top = 6,
            Right = 7,
            Bottom = 8,

            Count = 9,
        }
        enCanvasAnchors mSelectedAnchorHandleType = enCanvasAnchors.Unknow;

        enum enTransformOPHandleType
        {
            Unknow = -1,
            LeftTop = 0,
            Top = 1,
            RightTop = 2,
            Right = 3,
            RightBottom = 4,
            Bottom = 5,
            LeftBottom = 6,
            Left = 7,
            Count = 8,
        }
        enTransformOPHandleType mSelectedHandleType = enTransformOPHandleType.Unknow;

       
        class AnchorsHandleData
        {
            public EngineNS.UISystem.Controls.Image Show;
            public EngineNS.Vector2 StartMin;
            public EngineNS.Vector2 StartMax;
            public EngineNS.RectangleF SrcRect;
        }
       
        AnchorsHandleData[] mCanvasAnchorsHandles = new AnchorsHandleData[(int)enCanvasAnchors.Count];

       
        class TransformHandleData
        {
            public EngineNS.UISystem.Controls.Image Show;
            public EngineNS.RectangleF SrcRect;
        }
        TransformHandleData[] mTransformHandles = new TransformHandleData[(int)enTransformOPHandleType.Count];

        bool mInitialized = false;
        // 初始化
        public async Task Init(EngineNS.CRenderContext rc)
        {
            await InitCanvasAnchorsHandles(rc);
            await InitTransformOperationAssist(rc);
            mInitialized = true;
        }
        async Task InitCanvasAnchorsHandles(EngineNS.CRenderContext rc)
        {
            string[] pinNames = new string[]{ "uieditor/uva_op_center.uvanim",
                                              "uieditor/uva_op_topleft.uvanim",
                                              "uieditor/uva_op_topright.uvanim",
                                              "uieditor/uva_op_bottomright.uvanim",
                                              "uieditor/uva_op_bottomleft.uvanim",
                                              "uieditor/uva_op_left.uvanim",
                                              "uieditor/uva_op_top.uvanim",
                                              "uieditor/uva_op_right.uvanim",
                                              "uieditor/uva_op_bottom.uvanim"};

            for (int i = 0; i < (int)enCanvasAnchors.Count; i++)
            {
                var imgInit = new EngineNS.UISystem.Controls.ImageInitializer();
                imgInit.ImageBrush.ImageName = EngineNS.RName.GetRName(pinNames[i], EngineNS.RName.enRNameType.Editor);
                imgInit.ImageBrush.TileMode = EngineNS.UISystem.Brush.enTileMode.None;
                var img = new EngineNS.UISystem.Controls.Image();
                await img.Initialize(rc, imgInit);
                img.Visibility = EngineNS.UISystem.Visibility.Collapsed;

                var rect = new EngineNS.RectangleF(0, 0, 16, 16);
                var uvAnim = await EngineNS.CEngine.Instance.UVAnimManager.GetUVAnimAsync(rc, imgInit.ImageBrush.ImageName);
                if (uvAnim != null && uvAnim.Frames.Count > 0)
                {
                    await uvAnim.WaitTextureValid(rc);
                    var width = uvAnim.PixelWidth * uvAnim.Frames[0].SizeU;
                    var height = uvAnim.PixelHeight * uvAnim.Frames[0].SizeV;
                    rect = new EngineNS.RectangleF(0, 0, width, height);
                }

                mCanvasAnchorsHandles[i] = new AnchorsHandleData()
                {
                    Show = img,
                    SrcRect = rect,
                };
            }
        }
        async Task InitTransformOperationAssist(EngineNS.CRenderContext rc)
        {
            for (int i = 0; i < (int)enTransformOPHandleType.Count; i++)
            {
                var imgInit = new EngineNS.UISystem.Controls.ImageInitializer();
                imgInit.ImageBrush.ImageName = EngineNS.RName.GetRName("uieditor/uva_op_pin.uvanim", EngineNS.RName.enRNameType.Editor);
                imgInit.ImageBrush.TileMode = EngineNS.UISystem.Brush.enTileMode.None;
                var img = new EngineNS.UISystem.Controls.Image();
                await img.Initialize(rc, imgInit);
                img.Visibility = EngineNS.UISystem.Visibility.Collapsed;

                var rect = new EngineNS.RectangleF(0, 0, 16, 16);
                var uvAnim = await EngineNS.CEngine.Instance.UVAnimManager.GetUVAnimAsync(rc, imgInit.ImageBrush.ImageName);
                if (uvAnim != null && uvAnim.Frames.Count > 0)
                {
                    await uvAnim.WaitTextureValid(rc);
                    var width = uvAnim.PixelWidth * uvAnim.Frames[0].SizeU;
                    var height = uvAnim.PixelHeight * uvAnim.Frames[0].SizeV;
                    rect = new EngineNS.RectangleF(0, 0, width, height);
                }

                mTransformHandles[i] = new TransformHandleData()
                {
                    Show = img,
                    SrcRect = rect,
                };
            }
        }

        // 更新显示
        public void UpdateShow(EngineNS.UISystem.Editor.SelectedData operationUIData, ref EngineNS.RectangleF windowDesignSize, ref EngineNS.RectangleF windowDrawSize)
        {
            if (!mInitialized)
                return;
            UpdateCanvasAnchorsHandlesShow(operationUIData, ref windowDesignSize, ref windowDrawSize);
            UpdateTransformOperationAssistShow(operationUIData);
        }
        void UpdateCanvasAnchorsHandlesShow(EngineNS.UISystem.Editor.SelectedData operationUIData, ref EngineNS.RectangleF windowDesignSize, ref EngineNS.RectangleF windowDrawSize)
        {
            foreach (var handle in mCanvasAnchorsHandles)
            {
                if (handle == null)
                    continue;
                handle.Show.Visibility = EngineNS.UISystem.Visibility.Collapsed;
            }
            if(operationUIData != null)
            {
                var anchorOp = operationUIData.UI.Slot as EngineNS.UISystem.Controls.Containers.CanvasSlot;
                if (anchorOp == null)
                    return;
                var canvas = operationUIData.UI.Parent as EngineNS.UISystem.Controls.Containers.CanvasPanel;
                if (canvas == null)
                    return;

                if (anchorOp.Maximum.Equals(anchorOp.Minimum))
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Center].Show.Visibility = EngineNS.UISystem.Visibility.Visible;
                else
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Center].Show.Visibility = EngineNS.UISystem.Visibility.Collapsed;
                if (anchorOp.Maximum.X == anchorOp.Minimum.X)
                {
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Top].Show.Visibility = EngineNS.UISystem.Visibility.Visible;
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Bottom].Show.Visibility = EngineNS.UISystem.Visibility.Visible;
                }
                else
                {
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Top].Show.Visibility = EngineNS.UISystem.Visibility.Collapsed;
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Bottom].Show.Visibility = EngineNS.UISystem.Visibility.Collapsed;
                }
                if (anchorOp.Maximum.Y == anchorOp.Minimum.Y)
                {
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Left].Show.Visibility = EngineNS.UISystem.Visibility.Visible;
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Right].Show.Visibility = EngineNS.UISystem.Visibility.Visible;
                }
                else
                {
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Left].Show.Visibility = EngineNS.UISystem.Visibility.Collapsed;
                    mCanvasAnchorsHandles[(int)enCanvasAnchors.Right].Show.Visibility = EngineNS.UISystem.Visibility.Collapsed;
                }
                mCanvasAnchorsHandles[(int)enCanvasAnchors.LeftTop].Show.Visibility = EngineNS.UISystem.Visibility.Visible;
                mCanvasAnchorsHandles[(int)enCanvasAnchors.RightTop].Show.Visibility = EngineNS.UISystem.Visibility.Visible;
                mCanvasAnchorsHandles[(int)enCanvasAnchors.RightBottom].Show.Visibility = EngineNS.UISystem.Visibility.Visible;
                mCanvasAnchorsHandles[(int)enCanvasAnchors.LeftBottom].Show.Visibility = EngineNS.UISystem.Visibility.Visible;

                var canvasRect = canvas.DesignRect;
                var leftDelta = canvasRect.X / windowDesignSize.Width;
                var topDelta = canvasRect.Y / windowDesignSize.Height;
                var widthDelta = canvasRect.Width / windowDesignSize.Width;
                var heightDelta = canvasRect.Height / windowDesignSize.Height;
                canvasRect = new EngineNS.RectangleF(windowDrawSize.Width * leftDelta + windowDrawSize.Left,
                                                     windowDrawSize.Height * topDelta + windowDrawSize.Top,
                                                     windowDrawSize.Width * widthDelta,
                                                     windowDrawSize.Height * heightDelta);

                var showRect = new EngineNS.RectangleF();

                showRect.X = anchorOp.Minimum.X * canvasRect.Width + canvasRect.Left;
                showRect.Y = anchorOp.Minimum.Y * canvasRect.Height + canvasRect.Top;
                showRect.Width = (anchorOp.Maximum.X - anchorOp.Minimum.X) * canvasRect.Width;
                showRect.Height = (anchorOp.Maximum.Y - anchorOp.Minimum.Y) * canvasRect.Height;

                for (int i = 0; i < (int)enCanvasAnchors.Count; i++)
                {
                    var handle = mCanvasAnchorsHandles[i];
                    if (handle == null || handle.Show.Visibility == EngineNS.UISystem.Visibility.Collapsed)
                        continue;

                    var handleRect = handle.SrcRect;
                    var drawWidth = handleRect.Width;
                    var drawHeight = handleRect.Height;
                    var newRect = new EngineNS.RectangleF(0, 0, drawWidth, drawHeight);
                    switch ((enCanvasAnchors)i)
                    {
                        case enCanvasAnchors.Center:
                            newRect.X = showRect.X - drawWidth * 0.5f;
                            newRect.Y = showRect.Y - drawHeight * 0.5f;
                            break;
                        case enCanvasAnchors.Top:
                            newRect.X = showRect.X + showRect.Width * 0.5f - drawWidth * 0.5f;
                            newRect.Y = showRect.Y - drawHeight;
                            break;
                        case enCanvasAnchors.RightTop:
                            newRect.X = showRect.X + showRect.Width;
                            newRect.Y = showRect.Y - drawHeight;
                            break;
                        case enCanvasAnchors.Right:
                            newRect.X = showRect.X + showRect.Width;
                            newRect.Y = showRect.Y + showRect.Height * 0.5f - drawHeight * 0.5f;
                            break;
                        case enCanvasAnchors.RightBottom:
                            newRect.X = showRect.X + showRect.Width;
                            newRect.Y = showRect.Y + showRect.Height;
                            break;
                        case enCanvasAnchors.Bottom:
                            newRect.X = showRect.X + showRect.Width * 0.5f - drawWidth * 0.5f;
                            newRect.Y = showRect.Y + showRect.Height;
                            break;
                        case enCanvasAnchors.LeftBottom:
                            newRect.X = showRect.X - drawWidth;
                            newRect.Y = showRect.Y + showRect.Height;
                            break;
                        case enCanvasAnchors.Left:
                            newRect.X = showRect.X - drawWidth;
                            newRect.Y = showRect.Y + showRect.Height * 0.5f - drawHeight * 0.5f;
                            break;
                        case enCanvasAnchors.LeftTop:
                            newRect.X = showRect.X - drawWidth;
                            newRect.Y = showRect.Y - drawHeight;
                            break;
                    }
                    handle.Show.SetDesignRect(ref newRect, true);
                }
            }
        }
        void UpdateTransformOperationAssistShow(EngineNS.UISystem.Editor.SelectedData operationUIData)
        {
            if (operationUIData != null)
            {
                var selectRect = operationUIData.ShowRect.DesignRect;

                for (int i = 0; i < (int)enTransformOPHandleType.Count; i++)
                {
                    var handle = mTransformHandles[i];
                    if (handle == null)
                        continue;
                    handle.Show.Visibility = EngineNS.UISystem.Visibility.Visible;
                    var handleRect = handle.SrcRect;
                    var drawWidth = handleRect.Width;
                    var drawHeight = handleRect.Height;
                    var newRect = new EngineNS.RectangleF(0, 0, drawWidth, drawHeight);
                    switch ((enTransformOPHandleType)i)
                    {
                        case enTransformOPHandleType.LeftTop:
                            newRect.X = selectRect.X - drawWidth;
                            newRect.Y = selectRect.Y - drawHeight;
                            break;
                        case enTransformOPHandleType.Top:
                            newRect.X = selectRect.X + selectRect.Width * 0.5f - drawWidth * 0.5f;
                            newRect.Y = selectRect.Y - drawHeight;
                            break;
                        case enTransformOPHandleType.RightTop:
                            newRect.X = selectRect.X + selectRect.Width;
                            newRect.Y = selectRect.Y - drawHeight;
                            break;
                        case enTransformOPHandleType.Right:
                            newRect.X = selectRect.X + selectRect.Width;
                            newRect.Y = selectRect.Y + selectRect.Height * 0.5f - drawHeight * 0.5f;
                            break;
                        case enTransformOPHandleType.RightBottom:
                            newRect.X = selectRect.X + selectRect.Width;
                            newRect.Y = selectRect.Y + selectRect.Height;
                            break;
                        case enTransformOPHandleType.Bottom:
                            newRect.X = selectRect.X + selectRect.Width * 0.5f - drawWidth * 0.5f;
                            newRect.Y = selectRect.Y + selectRect.Height;
                            break;
                        case enTransformOPHandleType.LeftBottom:
                            newRect.X = selectRect.X - drawWidth;
                            newRect.Y = selectRect.Y + selectRect.Height;
                            break;
                        case enTransformOPHandleType.Left:
                            newRect.X = selectRect.X - drawWidth;
                            newRect.Y = selectRect.Y + selectRect.Height * 0.5f - drawHeight * 0.5f;
                            break;
                    }
                    handle.Show.SetDesignRect(ref newRect, true);
                }
            }
            else
            {
                for (int i = 0; i < (int)enTransformOPHandleType.Count; i++)
                {
                    if (mTransformHandles[i] == null)
                        continue;
                    mTransformHandles[i].Show.Visibility = EngineNS.UISystem.Visibility.Collapsed;
                }
            }
        }


        // 选中操作点
        public void ProcessSelect(EngineNS.UISystem.Editor.SelectedData operationUIData, ref EngineNS.PointF mouseInViewport)
        {
            if (!mInitialized)
                return;
            ProcessSelectAnchorsHandle(operationUIData, ref mouseInViewport);
            ProcessSelectTransformHandle(ref mouseInViewport);
        }
        void ProcessSelectAnchorsHandle(EngineNS.UISystem.Editor.SelectedData operationUIData, ref EngineNS.PointF mouseInViewport)
        {
            if (operationUIData == null || mSelectedAnchorHandleType == enCanvasAnchors.Unknow)
                return;
            var anchorOp = operationUIData.UI.Slot as EngineNS.UISystem.Controls.Containers.CanvasSlot;
            if (anchorOp == null)
                return;

            mCanvasAnchorsHandles[(int)mSelectedAnchorHandleType].StartMin = anchorOp.Minimum;
            mCanvasAnchorsHandles[(int)mSelectedAnchorHandleType].StartMax = anchorOp.Maximum;
        }
        void ProcessSelectTransformHandle(ref EngineNS.PointF mouseInViewport)
        {
            mSelectedHandleType = enTransformOPHandleType.Unknow;
            for (int i = 0; i < (int)enTransformOPHandleType.Count; i++)
            {
                var handle = mTransformHandles[i];
                if (handle == null)
                    continue;
                if (handle.Show.Visibility != EngineNS.UISystem.Visibility.Visible)
                    continue;
                if (handle.Show.DesignRect.Contains(ref mouseInViewport))
                {
                    mSelectedHandleType = (enTransformOPHandleType)i;
                }
            }
        }

        // 鼠标指向
        public bool ProcessMousePointAt(ref EngineNS.PointF mouseInViewport, Action<EngineNS.UISystem.Editor.enCursors> setCursorAction)
        {
            if (!mInitialized)
                return false;
            var retVal = ProcessMousePointAtTransformHandles(ref mouseInViewport, setCursorAction);
            if (retVal == true)
                return retVal;
            return ProcessMousePointAtAnchorsHandle(ref mouseInViewport, setCursorAction);
        }
        bool ProcessMousePointAtAnchorsHandle(ref EngineNS.PointF mouseInViewport, Action<EngineNS.UISystem.Editor.enCursors> setCursorAction)
        {
            mSelectedAnchorHandleType = enCanvasAnchors.Unknow;
            for (int i = 0; i < mCanvasAnchorsHandles.Length; i++)
            {
                var data = mCanvasAnchorsHandles[i];
                if (data == null)
                    continue;
                if (data.Show.Visibility != EngineNS.UISystem.Visibility.Visible)
                    continue;
                if (data.Show.DesignRect.Contains(ref mouseInViewport))
                {
                    mSelectedAnchorHandleType = (enCanvasAnchors)i;
                    switch ((enCanvasAnchors)i)
                    {
                        case enCanvasAnchors.Center:
                            setCursorAction?.Invoke(Editor.enCursors.SizeAll);
                            //this.Cursor = System.Windows.Input.Cursors.SizeAll;
                            break;
                        case enCanvasAnchors.Left:
                            setCursorAction?.Invoke(Editor.enCursors.SizeWE);
                            //this.Cursor = System.Windows.Input.Cursors.SizeWE;
                            break;
                        case enCanvasAnchors.Top:
                            setCursorAction?.Invoke(Editor.enCursors.SizeNS);
                            //this.Cursor = System.Windows.Input.Cursors.SizeNS;
                            break;
                        case enCanvasAnchors.Right:
                            setCursorAction?.Invoke(Editor.enCursors.SizeWE);
                            //this.Cursor = System.Windows.Input.Cursors.SizeWE;
                            break;
                        case enCanvasAnchors.Bottom:
                            setCursorAction?.Invoke(Editor.enCursors.SizeNS);
                            //this.Cursor = System.Windows.Input.Cursors.SizeNS;
                            break;
                        case enCanvasAnchors.LeftTop:
                            setCursorAction?.Invoke(Editor.enCursors.SizeNWSE);
                            //this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                            break;
                        case enCanvasAnchors.RightTop:
                            setCursorAction?.Invoke(Editor.enCursors.SizeNESW);
                            //this.Cursor = System.Windows.Input.Cursors.SizeNESW;
                            break;
                        case enCanvasAnchors.RightBottom:
                            setCursorAction?.Invoke(Editor.enCursors.SizeNWSE);
                            //this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                            break;
                        case enCanvasAnchors.LeftBottom:
                            setCursorAction?.Invoke(Editor.enCursors.SizeNESW);
                            //this.Cursor = System.Windows.Input.Cursors.SizeNESW;
                            break;
                    }

                    return true;
                }
            }
            return false;
        }
        bool ProcessMousePointAtTransformHandles(ref EngineNS.PointF mouseInViewport, Action<EngineNS.UISystem.Editor.enCursors> setCursorAction)
        {
            for (int i = 0; i < mTransformHandles.Length; i++)
            {
                var data = mTransformHandles[i];
                if (data == null)
                    continue;
                if (data.Show.Visibility != EngineNS.UISystem.Visibility.Visible)
                    continue;
                if (data.Show.DesignRect.Contains(ref mouseInViewport))
                {
                    switch ((enTransformOPHandleType)i)
                    {
                        case enTransformOPHandleType.Left:
                            //this.Cursor = System.Windows.Input.Cursors.SizeWE;
                            setCursorAction(Editor.enCursors.SizeWE);
                            break;
                        case enTransformOPHandleType.LeftTop:
                            //this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                            setCursorAction(Editor.enCursors.SizeNWSE);
                            break;
                        case enTransformOPHandleType.Top:
                            //this.Cursor = System.Windows.Input.Cursors.SizeNS;
                            setCursorAction(Editor.enCursors.SizeNS);
                            break;
                        case enTransformOPHandleType.RightTop:
                            //this.Cursor = System.Windows.Input.Cursors.SizeNESW;
                            setCursorAction(Editor.enCursors.SizeNESW);
                            break;
                        case enTransformOPHandleType.Right:
                            //this.Cursor = System.Windows.Input.Cursors.SizeWE;
                            setCursorAction(Editor.enCursors.SizeWE);
                            break;
                        case enTransformOPHandleType.RightBottom:
                            //this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                            setCursorAction(Editor.enCursors.SizeNWSE);
                            break;
                        case enTransformOPHandleType.Bottom:
                            //this.Cursor = System.Windows.Input.Cursors.SizeNS;
                            setCursorAction(Editor.enCursors.SizeNS);
                            break;
                        case enTransformOPHandleType.LeftBottom:
                            //this.Cursor = System.Windows.Input.Cursors.SizeNESW;
                            setCursorAction(Editor.enCursors.SizeNESW);
                            break;
                    }

                    return true;
                }
            }
            return false;
        }


        float[] mSnapPoints = new float[] { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
        float GetValidValue(float inValue, float minValue, float maxValue)
        {
            if (inValue < minValue)
                return minValue;
            if (inValue > maxValue)
                return maxValue;
            foreach (var snapPoint in mSnapPoints)
            {
                if (System.Math.Abs(inValue - snapPoint) < 0.01f)
                    return snapPoint;
            }
            return inValue;
        }
        public bool IsSelectedOperator()
        {
            if (mSelectedHandleType != enTransformOPHandleType.Unknow)
                return true;
            if (mSelectedAnchorHandleType != enCanvasAnchors.Unknow)
                return true;
            return false;
        }

        // 操作
        public void Operation(EngineNS.UISystem.Editor.SelectedData operationUIData, ref EngineNS.Vector2 mouseDelta, float scaleDelta)
        {
            if (!mInitialized)
                return;
            OperationAnchorsHandle(operationUIData, ref mouseDelta, scaleDelta);
            OperationTransformHandle(operationUIData, ref mouseDelta, scaleDelta);
        }
        void OperationAnchorsHandle(EngineNS.UISystem.Editor.SelectedData operationUIData, ref EngineNS.Vector2 mouseDelta, float scaleDelta)
        {
            if (mSelectedAnchorHandleType == enCanvasAnchors.Unknow)
                return;

            var anchorOp = operationUIData.UI.Slot as EngineNS.UISystem.Controls.Containers.CanvasSlot;
            if (anchorOp == null)
                return;
            var parent = operationUIData.UI.Parent;
            var parentWidth = parent.DesignRect.Width;
            var parentHeight = parent.DesignRect.Height;
            var handle = mCanvasAnchorsHandles[(int)mSelectedAnchorHandleType];
            var tagDesignRect = operationUIData.UI.DesignRect;

            switch (mSelectedAnchorHandleType)
            {
                case enCanvasAnchors.Left:
                    {
                        var newMinX = handle.StartMin.X + (mouseDelta.X * scaleDelta) / parentWidth;
                        newMinX = GetValidValue(newMinX, 0.0f, anchorOp.Maximum.X);
                        anchorOp.Minimum = new EngineNS.Vector2(newMinX, anchorOp.Minimum.Y);
                    }
                    break;
                case enCanvasAnchors.Right:
                    {
                        var newMaxX = handle.StartMax.X + (mouseDelta.X * scaleDelta) / parentWidth;
                        newMaxX = GetValidValue(newMaxX, anchorOp.Minimum.X, 1.0f);
                        anchorOp.Maximum = new EngineNS.Vector2(newMaxX, anchorOp.Maximum.Y);
                    }
                    break;
                case enCanvasAnchors.Top:
                    {
                        var newMinY = handle.StartMin.Y + (mouseDelta.Y * scaleDelta) / parentHeight;
                        newMinY = GetValidValue(newMinY, 0.0f, anchorOp.Maximum.Y);
                        anchorOp.Minimum = new EngineNS.Vector2(anchorOp.Minimum.X, newMinY);
                    }
                    break;
                case enCanvasAnchors.Bottom:
                    {
                        var newMaxY = handle.StartMax.Y + (mouseDelta.Y * scaleDelta) / parentHeight;
                        newMaxY = GetValidValue(newMaxY, anchorOp.Minimum.Y, 1.0f);
                        anchorOp.Maximum = new EngineNS.Vector2(anchorOp.Maximum.X, newMaxY);
                    }
                    break;
                case enCanvasAnchors.LeftTop:
                    {
                        var newMinX = handle.StartMin.X + (mouseDelta.X * scaleDelta) / parentWidth;
                        newMinX = GetValidValue(newMinX, 0.0f, anchorOp.Maximum.X);
                        anchorOp.Minimum = new EngineNS.Vector2(newMinX, anchorOp.Minimum.Y);

                        var newMinY = handle.StartMin.Y + (mouseDelta.Y * scaleDelta) / parentHeight;
                        newMinY = GetValidValue(newMinY, 0.0f, anchorOp.Maximum.Y);
                        anchorOp.Minimum = new EngineNS.Vector2(anchorOp.Minimum.X, newMinY);
                    }
                    break;
                case enCanvasAnchors.RightTop:
                    {
                        var newMaxX = handle.StartMax.X + (mouseDelta.X * scaleDelta) / parentWidth;
                        newMaxX = GetValidValue(newMaxX, anchorOp.Minimum.X, 1.0f);
                        anchorOp.Maximum = new EngineNS.Vector2(newMaxX, anchorOp.Maximum.Y);

                        var newMinY = handle.StartMin.Y + (mouseDelta.Y * scaleDelta) / parentHeight;
                        newMinY = GetValidValue(newMinY, 0.0f, anchorOp.Maximum.Y);
                        anchorOp.Minimum = new EngineNS.Vector2(anchorOp.Minimum.X, newMinY);
                    }
                    break;
                case enCanvasAnchors.LeftBottom:
                    {
                        var newMinX = handle.StartMin.X + (mouseDelta.X * scaleDelta) / parentWidth;
                        newMinX = GetValidValue(newMinX, 0.0f, anchorOp.Maximum.X);
                        anchorOp.Minimum = new EngineNS.Vector2(newMinX, anchorOp.Minimum.Y);

                        var newMaxY = handle.StartMax.Y + (mouseDelta.Y * scaleDelta) / parentHeight;
                        newMaxY = GetValidValue(newMaxY, anchorOp.Minimum.Y, 1.0f);
                        anchorOp.Maximum = new EngineNS.Vector2(anchorOp.Maximum.X, newMaxY);
                    }
                    break;
                case enCanvasAnchors.RightBottom:
                    {
                        var newMaxX = handle.StartMax.X + (mouseDelta.X * scaleDelta) / parentWidth;
                        newMaxX = GetValidValue(newMaxX, anchorOp.Minimum.X, 1.0f);
                        anchorOp.Maximum = new EngineNS.Vector2(newMaxX, anchorOp.Maximum.Y);

                        var newMaxY = handle.StartMax.Y + (mouseDelta.Y * scaleDelta) / parentHeight;
                        newMaxY = GetValidValue(newMaxY, anchorOp.Minimum.Y, 1.0f);
                        anchorOp.Maximum = new EngineNS.Vector2(anchorOp.Maximum.X, newMaxY);
                    }
                    break;
                case enCanvasAnchors.Center:
                    {
                        var valX = handle.StartMin.X + (mouseDelta.X * scaleDelta) / parentWidth;
                        valX = GetValidValue(valX, 0.0f, 1.0f);
                        var valY = handle.StartMin.Y + (mouseDelta.Y * scaleDelta) / parentHeight;
                        valY = GetValidValue(valY, 0.0f, 1.0f);
                        anchorOp.Maximum = new EngineNS.Vector2(float.MaxValue, float.MaxValue);
                        anchorOp.Minimum = new EngineNS.Vector2(valX, valY);
                        anchorOp.Maximum = anchorOp.Minimum;

                    }
                    break;
            }
            anchorOp.ProcessSetContentDesignRect(ref tagDesignRect);
        }
        void OperationTransformHandle(EngineNS.UISystem.Editor.SelectedData data, ref EngineNS.Vector2 mouseDelta, float scaleDelta)
        {
            if (mSelectedHandleType == enTransformOPHandleType.Unknow)
                return;

            switch (mSelectedHandleType)
            {
                case enTransformOPHandleType.Left:
                    {
                        //foreach (var data in mSelectedUIDatas.Values)
                        {
                            var ds = data.UI.DesignRect;
                            var width = data.StartRect.Width - mouseDelta.X * scaleDelta;
                            if(width < 0)
                            {
                                ds.Width = -width;
                            }
                            else
                            {
                                ds.X = data.StartRect.X + mouseDelta.X * scaleDelta;
                                ds.Width = width;
                            }
                            if (data.UI.Slot != null)
                                data.UI.Slot.ProcessSetContentDesignRect(ref ds);
                            else
                                data.UI.SetDesignRect(ref ds);
                        }
                    }
                    break;
                case enTransformOPHandleType.LeftTop:
                    {
                        //foreach (var data in mSelectedUIDatas.Values)
                        {
                            var ds = data.UI.DesignRect;
                            var width = data.StartRect.Width - mouseDelta.X * scaleDelta;
                            if (width < 0)
                            {
                                ds.Width = -width;
                            }
                            else
                            {
                                ds.X = data.StartRect.X + mouseDelta.X * scaleDelta;
                                ds.Width = width;
                            }

                            var height = data.StartRect.Height - mouseDelta.Y * scaleDelta;
                            if(height < 0)
                            {
                                ds.Height = -height;
                            }
                            else
                            {
                                ds.Y = data.StartRect.Y + mouseDelta.Y * scaleDelta;
                                ds.Height = height;
                            }
                            if (data.UI.Slot != null)
                                data.UI.Slot.ProcessSetContentDesignRect(ref ds);
                            else
                                data.UI.SetDesignRect(ref ds);
                        }
                    }
                    break;
                case enTransformOPHandleType.Top:
                    {
                        //foreach (var data in mSelectedUIDatas.Values)
                        {
                            var ds = data.UI.DesignRect;
                            var height = data.StartRect.Height - mouseDelta.Y * scaleDelta;
                            if (height < 0)
                            {
                                ds.Height = -height;
                            }
                            else
                            {
                                ds.Y = data.StartRect.Y + mouseDelta.Y * scaleDelta;
                                ds.Height = height;
                            }
                            if (data.UI.Slot != null)
                                data.UI.Slot.ProcessSetContentDesignRect(ref ds);
                            else
                                data.UI.SetDesignRect(ref ds);
                        }
                    }
                    break;
                case enTransformOPHandleType.RightTop:
                    {
                        //foreach (var data in mSelectedUIDatas.Values)
                        {
                            var ds = data.UI.DesignRect;
                            var height = data.StartRect.Height - mouseDelta.Y * scaleDelta;
                            if (height < 0)
                            {
                                ds.Height = -height;
                            }
                            else
                            {
                                ds.Y = data.StartRect.Y + mouseDelta.Y * scaleDelta;
                                ds.Height = height;
                            }
                            var width = data.StartRect.Width + mouseDelta.X * scaleDelta;
                            if (width < 0)
                            {
                                ds.X = data.StartRect.Left + width;
                                ds.Width = -width;
                            }
                            else
                                ds.Width = width;
                            if (data.UI.Slot != null)
                                data.UI.Slot.ProcessSetContentDesignRect(ref ds);
                            else
                                data.UI.SetDesignRect(ref ds);
                        }
                    }
                    break;
                case enTransformOPHandleType.Right:
                    {
                        //foreach (var data in mSelectedUIDatas.Values)
                        {
                            var ds = data.UI.DesignRect;
                            var width = data.StartRect.Width + mouseDelta.X * scaleDelta;
                            if (width < 0)
                            {
                                ds.X = data.StartRect.Left + width;
                                ds.Width = -width;
                            }
                            else
                                ds.Width = width;
                            if (data.UI.Slot != null)
                                data.UI.Slot.ProcessSetContentDesignRect(ref ds);
                            else
                                data.UI.SetDesignRect(ref ds);
                        }
                    }
                    break;
                case enTransformOPHandleType.RightBottom:
                    {
                        //foreach (var data in mSelectedUIDatas.Values)
                        {
                            var ds = data.UI.DesignRect;
                            var width = data.StartRect.Width + mouseDelta.X * scaleDelta;
                            if (width < 0)
                            {
                                ds.X = data.StartRect.Left + width;
                                ds.Width = -width;
                            }
                            else
                                ds.Width = width;
                            var height = data.StartRect.Height + mouseDelta.Y * scaleDelta;
                            if (height < 0)
                            {
                                ds.Y = data.StartRect.Top + height;
                                ds.Height = -height;
                            }
                            else
                                ds.Height = height;
                            if (data.UI.Slot != null)
                                data.UI.Slot.ProcessSetContentDesignRect(ref ds);
                            else
                                data.UI.SetDesignRect(ref ds);
                        }
                    }
                    break;
                case enTransformOPHandleType.Bottom:
                    {
                        //foreach (var data in mSelectedUIDatas.Values)
                        {
                            var ds = data.UI.DesignRect;
                            var height = data.StartRect.Height + mouseDelta.Y * scaleDelta;
                            if (height < 0)
                            {
                                ds.Y = data.StartRect.Top + height;
                                ds.Height = -height;
                            }
                            else
                                ds.Height = height;
                            if (data.UI.Slot != null)
                                data.UI.Slot.ProcessSetContentDesignRect(ref ds);
                            else
                                data.UI.SetDesignRect(ref ds);
                        }
                    }
                    break;
                case enTransformOPHandleType.LeftBottom:
                    {
                        //foreach (var data in mSelectedUIDatas.Values)
                        {
                            var ds = data.UI.DesignRect;
                            var width = data.StartRect.Width - mouseDelta.X * scaleDelta;
                            if (width < 0)
                            {
                                ds.Width = -width;
                            }
                            else
                            {
                                ds.X = data.StartRect.X + mouseDelta.X * scaleDelta;
                                ds.Width = width;
                            }
                            var height = data.StartRect.Height + mouseDelta.Y * scaleDelta;
                            if (height < 0)
                            {
                                ds.Y = data.StartRect.Top + height;
                                ds.Height = -height;
                            }
                            else
                                ds.Height = height;
                            if (data.UI.Slot != null)
                                data.UI.Slot.ProcessSetContentDesignRect(ref ds);
                            else
                                data.UI.SetDesignRect(ref ds);
                        }
                    }
                    break;
            }

        }

        public void Commit(CCommandList cmd)
        {
            if (!mInitialized)
                return;
            var idMat = EngineNS.Matrix.Identity;
            foreach (var data in mTransformHandles)
            {
                data?.Show?.Commit(cmd, ref idMat, 1.0f);
            }
            foreach (var data in mCanvasAnchorsHandles)
            {
                data?.Show?.Commit(cmd, ref idMat, 1.0f);
            }
        }
        public void Draw(EngineNS.CRenderContext rc, EngineNS.CCommandList cmd, EngineNS.Graphics.View.CGfxScreenView view)
        {
            if (!mInitialized)
                return;
            foreach (var data in mTransformHandles)
            {
                data?.Show?.Draw(rc, cmd, view);
            }
            foreach (var data in mCanvasAnchorsHandles)
            {
                data?.Show?.Draw(rc, cmd, view);
            }
        }
    }
}
