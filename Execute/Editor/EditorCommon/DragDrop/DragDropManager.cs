using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EditorCommon.DragDrop
{
    public class DragDropManager : EngineNS.Editor.IEditorInstanceObject
    {
        public static DragDropManager Instance
        {
            get
            {
                var name = typeof(DragDropManager).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = new DragDropManager();
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }

        public void FinalCleanup()
        {
            FlyWindow.Close();
        }

        // 用于显示被拖动对象的窗口，跟随鼠标移动
        DragFlyWindow mFlyWindow = new DragFlyWindow();
        public DragFlyWindow FlyWindow
        {
            get { return mFlyWindow; }
        }
        public void ShowFlyWindow(bool show)
        {
            if (show)
                mFlyWindow.Show();
            else
                mFlyWindow.Hide();
        }

        object mDragType = "";
        public object DragType
        {
            get { return mDragType; }
        }

        DragDropManager()
        {
            //mTickWhenDragThread = new System.Threading.Thread(Tick);
            //mTickWhenDragThread.Name = "Drag Tick Thread";
            //mTickWhenDragThread.IsBackground = true;
            //mTickWhenDragThread.Start();

            System.Windows.DragDrop.AddQueryContinueDragHandler(mFlyWindow, QueryContinueDragEvent);
        }
        ~DragDropManager()
        {
            if (mFlyWindow != null)
                System.Windows.DragDrop.RemoveQueryContinueDragHandler(mFlyWindow, QueryContinueDragEvent);
        }

        void QueryContinueDragEvent(object sender, QueryContinueDragEventArgs e)
        {
            var point = ResourceLibrary.Win32.GetCursorPosInWPF();
            mFlyWindow.Left = point.X;
            mFlyWindow.Top = point.Y;
        }

        public EngineNS.Vector2 GetMousePos()
        {
            var screenPos = new ResourceLibrary.Win32.POINT();
            if (ResourceLibrary.Win32.GetCursorPos(ref screenPos))
            {
                return new EngineNS.Vector2(screenPos.X, screenPos.Y);
            }

            return EngineNS.Vector2.Zero;
        }

        List<IDragAbleObject> mDragedObjectList = new List<IDragAbleObject>();
        public List<IDragAbleObject> DragedObjectList
        {
            get { return mDragedObjectList; }
        }

        public string InfoString
        {
            get
            {
                if (mFlyWindow != null)
                    return mFlyWindow.InfoString;

                return "";
            }
            set
            {
                if (mFlyWindow != null)
                    mFlyWindow.InfoString = value;
            }
        }

        public System.Windows.Media.Brush InfoStringBrush
        {
            get
            {
                if (mFlyWindow != null)
                    return mFlyWindow.InfoStringBrush;
                return Brushes.White;
            }
            set
            {
                if (mFlyWindow != null)
                    mFlyWindow.InfoStringBrush = value;
            }
        }

        // 拖动总入口，开始拖动操作
        public void StartDrag(object type, IDragAbleObject[] dragObjects, string infoStr = "", DragDropEffects effect = DragDropEffects.All)
        {
            if (dragObjects == null || dragObjects.Length == 0)
                return;

            mDragType = type;
            if (mDragType == null)
                mDragType = "";

            mFlyWindow.LayoutRoot.Children.Clear();
            mFlyWindow.InfoString = infoStr;
            int curResCount = 1;
            int ResMaxCount = (dragObjects.Length > 5) ? 5 : dragObjects.Length;
            foreach (var obj in dragObjects)
            {
                var frameworkElement = obj.GetDragVisual();
                if (frameworkElement != null)
                {
                    var brush = new VisualBrush(frameworkElement);
                    var rect = new System.Windows.Shapes.Rectangle()
                    {
                        Fill = brush,
                        Width = frameworkElement.ActualWidth,
                        Height = frameworkElement.ActualHeight,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(curResCount * 5, curResCount * 5, 0, 0),
                        Opacity = curResCount * 1.0 / ResMaxCount
                    };
                    mFlyWindow.LayoutRoot.Children.Add(rect);
                    curResCount++;
                    // 最多显示5个对象
                    if (curResCount > ResMaxCount)
                        break;
                }
            }

            mDragedObjectList = new List<IDragAbleObject>(dragObjects);

            mFlyWindow.Count = dragObjects.Length;
            mFlyWindow.UpdateLayout();
            ShowDragDropWindow();
            System.Windows.DragDrop.DoDragDrop(mFlyWindow, dragObjects, effect);

            mDragType = "";
            HideDragDropWindow();
        }

        private void ShowDragDropWindow()
        {
            if (mFlyWindow.Visibility != Visibility.Visible)
                mFlyWindow.Show();
        }
        private void HideDragDropWindow()
        {
            if (mFlyWindow.Visibility == Visibility.Visible)
            {
                mFlyWindow.LayoutRoot.Children.Clear();
                mFlyWindow.Count = 0;
                mFlyWindow.UpdateLayout();
                mFlyWindow.Hide();
            }
        }

        #region TickDrag

        public delegate System.Threading.Tasks.Task Delegate_OnTickDragDataDragEnter(object sender, System.Windows.Forms.DragEventArgs e);
        public delegate System.Threading.Tasks.Task Delegate_OnTickDragDataDragLeave(object sender, EventArgs e);
        public delegate System.Threading.Tasks.Task Delegate_OnTickDragDataDragDrop(object sender, System.Windows.Forms.DragEventArgs e);
        public delegate System.Threading.Tasks.Task Delegate_OnTickDragDataDragOver(object sender, System.Windows.Forms.DragEventArgs e);

        public delegate void Delegate_DragTick();
        public class TickDragData
        {
            public bool IsDragOver = false;
            public Delegate_DragTick OnDragTick;
            public void Tick()
            {
                if (!IsDragOver)
                    return;

                if (OnDragTick != null)
                    OnDragTick();
            }

            public event Delegate_OnTickDragDataDragEnter OnTickDragDataDragEnter;
            public async System.Threading.Tasks.Task _OnTickDragDataDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
            {
                IsDragOver = true;

                if (OnTickDragDataDragEnter != null)
                    await OnTickDragDataDragEnter(sender, e);
            }
            public event Delegate_OnTickDragDataDragLeave OnTickDragDataDragLeave;
            public async System.Threading.Tasks.Task _OnTickDragDataDragLeave(object sender, EventArgs e)
            {
                if (OnTickDragDataDragLeave != null)
                    await OnTickDragDataDragLeave(sender, e);

                IsDragOver = false;
            }
            public event Delegate_OnTickDragDataDragDrop OnTickDragDataDragDrop;
            public async System.Threading.Tasks.Task _OnTickDragDataDragDrop(object sender, System.Windows.Forms.DragEventArgs e)
            {
                if (OnTickDragDataDragDrop != null)
                    await OnTickDragDataDragDrop(sender, e);

                IsDragOver = false;
            }
            public event Delegate_OnTickDragDataDragOver OnTickDragDataDragOver;
            public async System.Threading.Tasks.Task _OnTickDragDataDragOver(object sender, System.Windows.Forms.DragEventArgs e)
            {
                if (OnTickDragDataDragOver != null)
                    await OnTickDragDataDragOver(sender, e);
            }
        }

        Dictionary<string, TickDragData> mTickDragDataDic = new Dictionary<string, TickDragData>();
        public void RegisterDragTickData(string key, TickDragData data)
        {
            mTickDragDataDic.Add(key, data);
        }
        public TickDragData GetDragTickData(string key)
        {
            return mTickDragDataDic[key];
        }
        public void UnregisterDragTickData(string key)
        {
            mTickDragDataDic.Remove(key);
        }

        //static System.Threading.Thread mTickWhenDragThread;

        //bool mContinueDragTick = true;
        //public void Tick()
        //{
        //    while (mContinueDragTick)
        //    {
        //        foreach(var val in mTickDragDataDic)
        //        {
        //            val.Value.Tick();
        //        }

        //        System.Threading.Thread.Sleep(100);
        //    }
        //}


        #endregion

    }
}
