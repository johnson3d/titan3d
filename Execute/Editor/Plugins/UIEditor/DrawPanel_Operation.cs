using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UIEditor
{
    public partial class DrawPanel
    {
        private void Viewport_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals(EngineNS.UISystem.Program.ControlDragType))
                e.Effect = System.Windows.Forms.DragDropEffects.Copy;
            else
                e.Effect = System.Windows.Forms.DragDropEffects.None;
        }
        private void Viewport_DragLeave(object sender, EventArgs e)
        {

        }
        private void Viewport_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals(EngineNS.UISystem.Program.ControlDragType))
            {
                var pt = PointFromScreen(new System.Windows.Point(e.X, e.Y));
                var mousePt = new EngineNS.PointF((float)pt.X, (float)pt.Y);
                if (mWindowDrawSize.Contains(mousePt))
                {
                    var uiElement = CalculateCanAddChildableUIElement(ref mousePt);
                    if (uiElement == null)
                        e.Effect = System.Windows.Forms.DragDropEffects.None;
                    else
                        e.Effect = System.Windows.Forms.DragDropEffects.Copy;
                }
                else
                    e.Effect = System.Windows.Forms.DragDropEffects.None;
            }
        }
        async Task Viewport_DragDropProcess(EngineNS.PointF mousePt)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            // 计算一次鼠标所在的能够添加子的控件
            var uiElement = CalculateCanAddChildableUIElement(ref mousePt);
            if (uiElement == null)
                return;

            var mousePos = GetMouseInUserControl(ref mousePt);
            List<EngineNS.UISystem.UIElement> elemList = new List<EngineNS.UISystem.UIElement>();
            foreach (var dragObj in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
            {
                var ctrlView = dragObj as UIEditor.ControlView;
                if (ctrlView == null)
                    continue;

                var atts = ctrlView.UIControlType.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlInitAttribute), false);
                if (atts.Length <= 0)
                    continue;
                var att = atts[0] as EngineNS.UISystem.Editor_UIControlInitAttribute;
                var init = System.Activator.CreateInstance(att.InitializerType) as EngineNS.UISystem.UIElementInitializer;
                var uiCtrl = System.Activator.CreateInstance(ctrlView.UIControlType) as EngineNS.UISystem.UIElement;
                await uiCtrl.Initialize(rc, init);

                /*

                if (uiElement is EngineNS.UISystem.Controls.Containers.ContentControl)
                {
                    var ctrl = uiElement as EngineNS.UISystem.Controls.Containers.ContentControl;
                    ctrl.Content = uiCtrl;
                    var offsetX = mousePos.X;// - ctrl.DesignRect.X;
                    var offsetY = mousePos.Y;// - ctrl.DesignRect.Y;
                    var dr = uiCtrl.DesignRect;
                    var rect = new EngineNS.RectangleF(offsetX, offsetY, dr.Width, dr.Height);
                    uiCtrl.Slot.ProcessSetContentDesignRect(ref rect);
                }
                else if (uiElement is EngineNS.UISystem.Controls.Containers.Panel)
                {
                    var ctrl = uiElement as EngineNS.UISystem.Controls.Containers.Panel;
                    ctrl.AddChild(uiCtrl);
                    var offsetX = mousePos.X;// - ctrl.DesignRect.X;
                    var offsetY = mousePos.Y;// - ctrl.DesignRect.Y;
                    var dr = uiCtrl.DesignRect;
                    var rect = new EngineNS.RectangleF(offsetX, offsetY, dr.Width, dr.Height);
                    uiCtrl.Slot.ProcessSetContentDesignRect(ref rect);
                }*/

                elemList.Add(uiCtrl);
            }
            HostDesignPanel.BroadcastAddChildren(this, uiElement, mousePos, elemList.ToArray());
        }
        EngineNS.PointF GetMouseInUserControl(ref EngineNS.PointF mouseInViewport)
        {
            var pos = new EngineNS.PointF(mouseInViewport.X - mWindowDrawSize.Left, mouseInViewport.Y - mWindowDrawSize.Top);
            pos.X = pos.X * mWindowDesignSize.Width / mWindowDrawSize.Width;
            pos.Y = pos.Y * mWindowDesignSize.Height / mWindowDrawSize.Height;
            return pos;
        }
        EngineNS.UISystem.UIElement CalculateCanAddChildableUIElement(ref EngineNS.PointF mouseInViewport)
        {
            if (HostDesignPanel.CurrentUI == null)
                return null;

            var pos = GetMouseInUserControl(ref mouseInViewport);
            var element = CheckPointAtUIElement(ref pos, true);
            return CalculateCanAddChildableUIElement(element);
        }
        EngineNS.UISystem.UIElement CalculateCanAddChildableUIElement(EngineNS.UISystem.UIElement uiElement)
        {
            if (uiElement is EngineNS.UISystem.Controls.Containers.Panel)
                return uiElement;
            if (uiElement is EngineNS.UISystem.Controls.Containers.Border)
            {
                var contentCtrl = uiElement as EngineNS.UISystem.Controls.Containers.Border;
                if (contentCtrl.Content == null)
                    return uiElement;
                else
                {
                    if (uiElement.Parent == null)
                        return null;
                    return CalculateCanAddChildableUIElement(uiElement.Parent);
                }
            }
            if (uiElement.Parent == null)
                return null;
            return CalculateCanAddChildableUIElement(uiElement.Parent);
        }
        private void Viewport_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (!EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals(EngineNS.UISystem.Program.ControlDragType))
                return;

            var pt = PointFromScreen(new System.Windows.Point(e.X, e.Y));
            if (!mWindowDrawSize.Contains(new EngineNS.PointF((float)pt.X, (float)pt.Y)))
                return;

            var noUse = Viewport_DragDropProcess(new EngineNS.PointF((float)pt.X, (float)pt.Y));
        }

        bool mDragViewport = false;
        System.Drawing.Point mMouseLoc;
        void Viewport_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Mouse.Capture(sender as IInputElement);
            mMouseLoc = e.Location;
            mStartDrawSize = mGridDrawSize;
            mStartWindowDrawSize = mWindowDrawSize;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var noUse = SelectProcess(false);
            }

            mDragViewport = true;
        }
        class UIUndoRedoData
        {
            public EngineNS.UISystem.UIElement UI;
            public EngineNS.RectangleF DesignRect;
            public EngineNS.RectangleF StartRect;
        }
        void Viewport_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Mouse.Capture(null);
            mDragViewport = false;
            if (mReSelectOnMouseUp)
            {
                var noUse = SelectProcess(true);
            }

            List<UIUndoRedoData> undoRedoDatas = new List<UIUndoRedoData>();
            foreach(var data in mSelectedUIDatas)
            {
                var startRect = data.Value.StartRect;
                if(!data.Value.UI.DesignRect.Equals(ref startRect))
                {
                    var undoRedoData = new UIUndoRedoData()
                    {
                        UI = data.Value.UI,
                        DesignRect = data.Value.UI.DesignRect,
                        StartRect = data.Value.StartRect,
                    };
                    undoRedoDatas.Add(undoRedoData);
                }
            }
            if(undoRedoDatas.Count > 0)
            {
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostDesignPanel.UndoRedoKey, undoRedoDatas,
                                (obj) =>
                                {
                                    var urdlist = obj as List<UIUndoRedoData>;
                                    foreach(var data in urdlist)
                                    {
                                        if (data.UI.Slot != null)
                                            data.UI.Slot.ProcessSetContentDesignRect(ref data.DesignRect);
                                        else
                                            data.UI.SetDesignRect(ref data.DesignRect);
                                    }
                                },
                                undoRedoDatas,
                                (obj) =>
                                {
                                    var urdlist = obj as List<UIUndoRedoData>;
                                    foreach(var data in urdlist)
                                    {
                                        if (data.UI.Slot != null)
                                            data.UI.Slot.ProcessSetContentDesignRect(ref data.StartRect);
                                        else
                                            data.UI.SetDesignRect(ref data.StartRect);
                                    }
                                },
                                "Transform UI");
            }
        }
        void Viewport_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var delta = new EngineNS.Vector2(e.Location.X - mMouseLoc.X, e.Location.Y - mMouseLoc.Y);
            if (e.Button.HasFlag(System.Windows.Forms.MouseButtons.Right) && mDragViewport)
            {
                mGridDrawSize.X = mStartDrawSize.X + delta.X;
                mGridDrawSize.Y = mStartDrawSize.Y + delta.Y;

                mWindowDrawSize.X = mStartWindowDrawSize.X + delta.X;
                mWindowDrawSize.Y = mStartWindowDrawSize.Y + delta.Y;

                UpdateUIShow();
                UpdateUIAssistShow();
            }
            else if (e.Button.HasFlag(System.Windows.Forms.MouseButtons.Left) && mDragViewport)
            {
                if (System.Math.Abs(delta.X) > 2 || System.Math.Abs(delta.Y) > 2)
                    mReSelectOnMouseUp = false;

                var scaleDelta = mWindowDesignSize.Width / mWindowDrawSize.Width;
                //if(mSelectedHandleType != enTransformOPHandleType.Unknow)
                //{
                //    OperationTransformHandle(ref delta, scaleDelta);
                //}
                /*else */if(mSlotOperator != null && mSlotOperator.IsSelectedOperator())
                {
                    if(mSelectedUIDatas.Count == 1)
                    {
                        mSlotOperator?.Operation(mSelectedUIDatas.First().Value, ref delta, scaleDelta);
                    }
                }
                //else if (mSelectedAnchorHandleType != enCanvasAnchors.Unknow)
                //{
                //    OperationAnchorsHandle(ref delta, scaleDelta);
                //}
                else
                {
                    foreach (var data in mSelectedUIDatas.Values)
                    {
                        var ds = data.UI.DesignRect;
                        ds.X = data.StartRect.X + delta.X * scaleDelta;
                        ds.Y = data.StartRect.Y + delta.Y * scaleDelta;
                        if (data.UI.Slot != null)
                            data.UI.Slot.ProcessSetContentDesignRect(ref ds);
                        else
                            data.UI.SetDesignRect(ref ds);
                    }
                }
                UpdateSelectedRectShow();
            }
            else if (HostDesignPanel.CurrentUI != null)
            {
                var mouseInViewport = new EngineNS.PointF(e.Location.X, e.Location.Y);
                // 判断鼠标是不是在控制点上
                //bool isInHandle = ProcessMousePointAtTransformHandles(ref mouseInViewport);
                //if(isInHandle == false)
                //    isInHandle = ProcessMousePointAtAnchorsHandle(ref mouseInViewport);
                bool isInHandle = false;
                if (mSlotOperator != null)
                {
                    isInHandle = mSlotOperator.ProcessMousePointAt(ref mouseInViewport, (cur) =>
                    {
                        switch(cur)
                        {
                            case EngineNS.UISystem.Editor.enCursors.SizeAll:
                                this.Cursor = System.Windows.Input.Cursors.SizeAll;
                                break;
                            case EngineNS.UISystem.Editor.enCursors.SizeWE:
                                this.Cursor = System.Windows.Input.Cursors.SizeWE;
                                break;
                            case EngineNS.UISystem.Editor.enCursors.SizeNS:
                                this.Cursor = System.Windows.Input.Cursors.SizeNS;
                                break;
                            case EngineNS.UISystem.Editor.enCursors.SizeNWSE:
                                this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                                break;
                            case EngineNS.UISystem.Editor.enCursors.SizeNESW:
                                this.Cursor = System.Windows.Input.Cursors.SizeNESW;
                                break;
                        }
                    });
                }
                
                if (!isInHandle)
                {
                    this.Cursor = System.Windows.Input.Cursors.Arrow;
                    // 判断鼠标位于哪个控件上
                    var pos = GetMouseInUserControl(ref mouseInViewport);
                    mMousePointAtUIElement = CheckPointAtUIElement(ref pos, false);
                    UpdatePointAtShow();
                }
            }
        }
        EngineNS.UISystem.UIElement CheckPointAtUIElement(ref EngineNS.PointF pos, bool onlyClipped)
        {
            var pointAtUIElement = HostDesignPanel.CurrentUI.GetPointAtElement(ref pos, onlyClipped);
            var tempParent = pointAtUIElement;
            while(tempParent != null)
            {
                if(tempParent is EngineNS.UISystem.Controls.UserControl)
                    break;

                tempParent = tempParent.Parent;
            }
            if (tempParent == HostDesignPanel.CurrentUI)
                return pointAtUIElement;
            else
                return tempParent;
        }
        EngineNS.UISystem.UIElement mMousePointAtUIElement;
        EngineNS.UISystem.Controls.Image mMousePointAtUIRectShow;
        // 更新鼠标指向框显示
        void UpdatePointAtShow()
        {
            if (mMousePointAtUIRectShow == null)
                return;
            if (mMousePointAtUIElement == null)
            {
                mMousePointAtUIRectShow.Visibility = EngineNS.UISystem.Visibility.Collapsed;
                return;
            }

            var leftDelta = mMousePointAtUIElement.DesignRect.X / mWindowDesignSize.Width;
            var topDelta = mMousePointAtUIElement.DesignRect.Y / mWindowDesignSize.Height;
            var widthDelta = mMousePointAtUIElement.DesignRect.Width / mWindowDesignSize.Width;
            var heightDelta = mMousePointAtUIElement.DesignRect.Height / mWindowDesignSize.Height;
            var dRect = new EngineNS.RectangleF(mWindowDrawSize.Width * leftDelta + mWindowDrawSize.Left - 2,
                                                mWindowDrawSize.Height * topDelta + mWindowDrawSize.Top - 2,
                                                mWindowDrawSize.Width * widthDelta + 4,
                                                mWindowDrawSize.Height * heightDelta + 4);
            mMousePointAtUIRectShow.SetDesignRect(ref dRect, true);
            mMousePointAtUIRectShow.Visibility = EngineNS.UISystem.Visibility.Visible;
        }

        EngineNS.UISystem.Controls.Containers.ISlotOperator mSlotOperator;
        async Task InitSlotOperator()
        {
            mSlotOperator = null;
            if (mSelectedUIDatas.Count != 1)
                return;

            var slotOpType = mSelectedUIDatas.First().Value.UI.Slot?.GetSlotOperatorType();
            if (slotOpType == null)
                return;

            var rc = EngineNS.CEngine.Instance.RenderContext;
            mSlotOperator = System.Activator.CreateInstance(slotOpType) as EngineNS.UISystem.Controls.Containers.ISlotOperator;
            await mSlotOperator.Init(rc);
        }

        Dictionary<EngineNS.UISystem.UIElement, EngineNS.UISystem.Editor.SelectedData> mSelectedUIDatas = new Dictionary<EngineNS.UISystem.UIElement, EngineNS.UISystem.Editor.SelectedData>();
        bool mReSelectOnMouseUp = false;
        async Task SelectProcess(bool mouseUp)
        {
            mReSelectOnMouseUp = false;
            var mouseInViewport = new EngineNS.PointF(mMouseLoc.X, mMouseLoc.Y);
            // 判断是否在handle上
            if(mSelectedUIDatas.Count == 1)
            {   
                mSlotOperator?.ProcessSelect(mSelectedUIDatas.First().Value, ref mouseInViewport);
            }
            if (mSlotOperator == null || (mSlotOperator != null && !mSlotOperator.IsSelectedOperator()))
            {
                var pos = GetMouseInUserControl(ref mouseInViewport);
                bool multi = false;
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    multi = true;
                }

                List<EngineNS.UISystem.UIElement> selectUIs = new List<EngineNS.UISystem.UIElement>();
                var element = CheckPointAtUIElement(ref pos, false);
                if (element != null)
                {
                    bool find = false;
                    if (multi)
                    {
                        if (mSelectedUIDatas.ContainsKey(element))
                        {
                            find = true;
                            mSelectedUIDatas.Remove(element);
                        }
                        foreach(var ui in mSelectedUIDatas)
                        {
                            selectUIs.Add(ui.Key);
                        }
                    }
                    else
                    {
                        if (mSelectedUIDatas.ContainsKey(element) && !mouseUp)
                        {
                            find = true;
                            mReSelectOnMouseUp = true;
                            foreach(var ui in mSelectedUIDatas)
                            {
                                selectUIs.Add(ui.Key);
                            }
                        }
                        else
                        {
                            //mSelectedUIDatas.Clear();
                            selectUIs.Clear();
                        }
                    }

                    if (find == false)
                    {
                        //var data = new EngineNS.UISystem.Editor.SelectedData();
                        //data.UI = element;
                        //data.StartRect = element.DesignRect;
                        //var rc = EngineNS.CEngine.Instance.RenderContext;
                        //var showRectInit = new EngineNS.UISystem.Controls.ImageInitializer();
                        //showRectInit.ImageBrush.ImageName = EngineNS.RName.GetRName("uieditor/uva_selectrect.uvanim", EngineNS.RName.enRNameType.Editor);
                        //showRectInit.ImageBrush.TileMode = EngineNS.UISystem.Brush.enTileMode.None;
                        //var showRect = new EngineNS.UISystem.Controls.Image();
                        //await showRect.Initialize(rc, showRectInit);
                        //data.ShowRect = showRect;

                        //mSelectedUIDatas[element] = data;
                        selectUIs.Add(element);
                    }
                    // 刷新所有StartDesignRect
                    foreach (var data in mSelectedUIDatas.Values)
                    {
                        data.StartRect = data.UI.DesignRect;
                    }
                }
                else if (!multi)
                {
                    //mSelectedUIDatas.Clear();
                    selectUIs.Clear();
                }

                //var selectUIs = mSelectedUIDatas.Keys.ToArray();
                HostDesignPanel.BroadcastSelectedUI(this, selectUIs.ToArray());
                await InitSlotOperator();
                UpdateSelectedRectShow();
            }
            else
            {
                foreach (var data in mSelectedUIDatas.Values)
                {
                    data.StartRect = data.UI.DesignRect;
                }
            }
        }
        internal async Task OnReceiveSelectUIs(EngineNS.UISystem.UIElement[] selectedUIs)
        {
            mSelectedUIDatas.Clear();
            if(selectedUIs != null)
            {
                foreach (var ui in selectedUIs)
                {
                    var data = new EngineNS.UISystem.Editor.SelectedData();
                    data.UI = ui;
                    data.StartRect = ui.DesignRect;
                    var rc = EngineNS.CEngine.Instance.RenderContext;
                    var showRectInit = new EngineNS.UISystem.Controls.ImageInitializer();
                    showRectInit.ImageBrush.ImageName = EngineNS.RName.GetRName("uieditor/uva_selectrect.uvanim", EngineNS.RName.enRNameType.Editor);
                    showRectInit.ImageBrush.TileMode = EngineNS.UISystem.Brush.enTileMode.None;
                    var showRect = new EngineNS.UISystem.Controls.Image();
                    await showRect.Initialize(rc, showRectInit);
                    data.ShowRect = showRect;

                    mSelectedUIDatas[ui] = data;
                }
            }
            await InitSlotOperator();
            UpdateSelectedRectShow();
        }
        void UpdateSelectedRectShow()
        {
            if (mSelectedUIDatas.Count == 0)
            {
                //UpdateTransformOperationAssistShow();
                mSlotOperator?.UpdateShow(null, ref mWindowDesignSize, ref mWindowDrawSize);
                //UpdateCanvasAnchorsHandlesShow();
                return;
            }
            foreach (var data in mSelectedUIDatas.Values)
            {
                var uiElement = data.UI;
                var show = data.ShowRect;

                var leftDelta = uiElement.DesignRect.X / mWindowDesignSize.Width;
                var topDelta = uiElement.DesignRect.Y / mWindowDesignSize.Height;
                var widthDelta = uiElement.DesignRect.Width / mWindowDesignSize.Width;
                var heightDelta = uiElement.DesignRect.Height / mWindowDesignSize.Height;
                var dRect = new EngineNS.RectangleF(mWindowDrawSize.Width * leftDelta + mWindowDrawSize.Left - 1,
                                                    mWindowDrawSize.Height * topDelta + mWindowDrawSize.Top - 1,
                                                    mWindowDrawSize.Width * widthDelta + 2,
                                                    mWindowDrawSize.Height * heightDelta + 2);
                if (!show.DesignRect.Equals(ref dRect))
                    show.SetDesignRect(ref dRect, true);
            }
            //UpdateTransformOperationAssistShow();
            if(mSelectedUIDatas.Count == 1)
            {
                mSlotOperator?.UpdateShow(mSelectedUIDatas.First().Value, ref mWindowDesignSize, ref mWindowDrawSize);
            }
            //UpdateCanvasAnchorsHandlesShow();
        }
    }
}
