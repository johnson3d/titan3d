using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem
{
    [IO.Serializer.EnumSizeAttribute(typeof(IO.Serializer.SByteEnum))]
    [EngineNS.Editor.Editor_MacrossClassAttribute(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public enum HorizontalAlignment : sbyte
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Stretch = 3,
    }
    [IO.Serializer.EnumSizeAttribute(typeof(IO.Serializer.SByteEnum))]
    [EngineNS.Editor.Editor_MacrossClassAttribute(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public enum VerticalAlignment : sbyte
    {
        Top = 0,
        Center = 1,
        Bottom = 2,
        Stretch = 3,
    }
    [IO.Serializer.EnumSizeAttribute(typeof(IO.Serializer.SByteEnum))]
    [EngineNS.Editor.Editor_MacrossClassAttribute(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public enum Visibility : sbyte
    {
        Visible = 0,
        Hidden,
        Collapsed,
    }

    [IO.Serializer.EnumSizeAttribute(typeof(IO.Serializer.SByteEnum))]
    [EngineNS.Editor.Editor_MacrossClassAttribute(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public enum enContainChildType : sbyte
    {
        NoChild = 0,
        SingleChild,
        MultiChild,
    }

    [Rtti.MetaClass]
    public partial class UIElementInitializer : IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        int mId = int.MaxValue;
        [ReadOnly(true)]
        [Rtti.MetaData]
        public int Id
        {
            get => mId;
            set
            {
                mId = value;
                OnPropertyChanged("Id");
            }
        }

        string mName = "";
        [Rtti.MetaData]
        public string Name
        {
            get => mName;
            set
            {
                mName = value;
                OnPropertyChanged("Name");
            }
        }
        [Rtti.MetaData]
        public bool IsVariable { get; set; } = false;
        RectangleF mDesignRect = new RectangleF(0, 0, 100, 100);
        // 在设计分辨率下的大小
        [Rtti.MetaData]
        [Browsable(false)]
        public RectangleF DesignRect
        {
            get => mDesignRect;
            set
            {
                mDesignRect = value;
                OnPropertyChanged("DesignRect");
            }
        }
        RectangleF mDesignClipRect = new RectangleF(0, 0, 100, 100);
        // 设计分辨率下的裁剪区域
        [Rtti.MetaData]
        [Browsable(false)]
        public RectangleF DesignClipRect
        {
            get => mDesignClipRect;
            set
            {
                mDesignClipRect = value;
                OnPropertyChanged("DesignClipRect");
            }
        }

        int mVersion = 0;
        [Rtti.MetaData]
        [Browsable(false)]
        public int Version
        {
            get => mVersion;
            protected set
            {
                mVersion = value;
                OnPropertyChanged("Version");
            }
        }

        Visibility mVisibility = Visibility.Visible;
        [Rtti.MetaData]
        public Visibility Visibility
        {
            get => mVisibility;
            set
            {
                mVisibility = value;
                OnPropertyChanged("Visibility");
            }
        }

        enContainChildType mContainChildType = enContainChildType.NoChild;
        [Rtti.MetaData]
        public enContainChildType ContainChildType
        {
            get => mContainChildType;
            set
            {
                mContainChildType = value;
                OnPropertyChanged("ContainChildType");
            }
        }
    }

   
    public partial class UIElement : INotifyPropertyChanged, ITickInfo
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;

        public struct stUIPropertyChangedData
        {
            public UIElement SenderUI;
            public string PropertyName;
        }
        public delegate void Delegate_UIPropertyChanged(stUIPropertyChangedData data);
        public event Delegate_UIPropertyChanged UIPropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            var data = new stUIPropertyChangedData()
            {
                SenderUI = this,
                PropertyName = propertyName,
            };
            UIPropertyChanged?.Invoke(data);
        }
        #endregion

        #region ITickInfo
        public virtual void BeforeFrame()
        {

        }
        public virtual void TickLogic()
        {

        }
        public virtual void TickRender()
        {

        }
        public virtual void TickSync()
        {

        }
        [Browsable(false)]
        public virtual bool EnableTick
        {
            get;
            set;
        }
        public static Profiler.TimeScope UIElementScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(UIElement), nameof(TickLogic));
        public virtual Profiler.TimeScope GetLogicTimeScope()
        {
            return UIElementScopeTickLogic;
        }
        #endregion

        UIHost mRootUIHost;
        [Browsable(false)]
        public UIHost RootUIHost
        {
            get
            {
                if(mRootUIHost == null)
                {
                    var parent = this;
                    while(parent != null)
                    {
                        if (parent is UIHost)
                        {
                            mRootUIHost = parent as UIHost;
                            break;
                        }
                        parent = parent.Parent;
                    }
                }
                return mRootUIHost;
            }
            set
            {
                mRootUIHost = value;
            }
        }

        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.PropReadOnly)]
        public int Id
        {
            get => mInitializer.Id;
        }

        UIElement mParent;
        [Browsable(false)]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.PropReadOnly)]
        public UIElement Parent => mParent;
        internal void RemoveFromParent()
        {
            var panel = mParent as UISystem.Controls.Containers.Panel;
            if (panel != null)
            {
                panel.ChildrenUIElements.Remove(this);
            }
            mParent = null;
            RootUIHost = null;
            NeverMeasured = true;
        }
        internal void SetParent(UIElement parent)
        {
            mParent = parent;
        }

        public virtual void Cleanup()
        {

        }

        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual void AddChild(UIElement element, bool updateLayout = true) { }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual void InsertChild(int index, UIElement element, bool updateLayout = true) { }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual void RemoveChild(UIElement element, bool updateLayout = true) { }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual void ClearChildren(bool updateLayout = true) { }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual UIElement FindChildElement(Int32 id) { return null; }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual int FindChildIndex(UIElement ui) { return -1; }

        #region Drawing

        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [DisplayName("可见性")]
        public Visibility Visibility
        {
            get
            {
                if(mInitializer != null)
                    return mInitializer.Visibility;
                return Visibility.Collapsed;
            }
            set
            {
                if (mInitializer == null)
                    return;
                if (mInitializer.Visibility == value)
                    return;
                mInitializer.Visibility = value;
                OnPropertyChanged("Visibility");
            }
        }

        protected Brush mCurrentBrush;
        [Browsable(false)]
       
        public Brush CurrentBrush => mCurrentBrush;
        public void SetCurrentBrush(Brush brush)
        {
            mCurrentBrush = brush;
        }

        [Browsable(false)]
        public RectangleF DesignRect
        {
            get => mInitializer.DesignRect;
            protected set
            {
                mInitializer.DesignRect = value;
            }
        }
        public void SetDesignRect(ref RectangleF rect, bool updateClipRect = false)
        {
            if (mInitializer == null)
                return;
            if (mInitializer.DesignRect.Equals(ref rect))
                return;
            mInitializer.DesignRect = rect;
            if (updateClipRect)
                UpdateDesignClipRect();
            UpdateLayout();
        }
        [Browsable(false)]
        public RectangleF DesignClipRect
        {
            get => mInitializer.DesignClipRect;
            protected set
            {
                mInitializer.DesignClipRect = value;
            }
        }
        protected void UpdateDesignClipRect()
        {
            var oldClipRect = DesignClipRect;
            if (Parent != null)
            {
                var parentClipRect = Parent.DesignClipRect;
                DesignClipRect = DesignRect.Intersect(ref parentClipRect);
            }
            else
            {
                DesignClipRect = new RectangleF(DesignRect.Left, DesignRect.Top, System.Math.Abs(DesignRect.Width), System.Math.Abs(DesignRect.Height));
            }

            if ((oldClipRect.Width != DesignClipRect.Width) ||
               (oldClipRect.Height != DesignClipRect.Height))
                ForceUpdateDraw = true;
        }

        protected UIElementInitializer mInitializer;
        [Browsable(false)]
        public UIElementInitializer Initializer => mInitializer;
        public virtual async Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            mInitializer = init;
            
            NeverMeasured = true;
            NeverArranged = true;

            UpdateLayout();

            return true;
        }

        public UVFrame GetCurrentFrame()
        {
            bool frameChanged;
            return mCurrentBrush.UVAnim.GetUVFrame(Support.Time.GetTickCount(), out frameChanged);
        }
        [Browsable(false)]
        public bool ForceUpdateDraw
        {
            get
            {
                if (mCurrentBrush != null)
                    return mCurrentBrush.ForceUpdateDraw;
                return false;
            }
            set
            {
                if (mCurrentBrush != null)
                    mCurrentBrush.ForceUpdateDraw = value;
            }
        }

        protected virtual bool IsRenderable()
        {
            //if (mCurrentBrush == null)
            //    return false;
            if (mInitializer.Visibility != Visibility.Visible)
                return false;
            //return IsMeasureValid && IsArrangeValid;
            return true;
        }

        public virtual bool Commit(CCommandList cmd, ref Matrix parentTransformMatrix, float dpiScale)
        {
            if (IsRenderable() == false)
                return false;
            mCurrentBrush?.CommitUVAnim(cmd, this, ref parentTransformMatrix, dpiScale);
            return true;
        }

        public virtual bool Draw(CRenderContext rc, CCommandList cmd, Graphics.View.CGfxScreenView view)
        {
            if (IsRenderable() == false)
                return false;
            mCurrentBrush?.Draw(rc, cmd, view);
            return true;
        }

        #endregion

        //public virtual void Save2Xnd(IO.XndNode node)
        //{
        //    if(mInitializer != null)
        //    {
        //        var attr = node.AddAttrib("Initializer");
        //        attr.BeginWrite();
        //        attr.WriteMetaObject(mInitializer);
        //        attr.EndWrite();
        //    }
        //}
        //public virtual async Task<bool> LoadXnd(CRenderContext rc, EngineNS.IO.XndNode node)
        //{
        //    var attr = node.FindAttrib("Initializer");
        //    if (attr == null)
        //        return true;

        //    UIElementInitializer init = null;
        //    var ret = await EngineNS.CEngine.Instance.EventPoster.Post(() =>
        //    {
        //        attr.BeginRead();
        //        init = attr.ReadMetaObject(null) as UIElementInitializer;
        //        if(init == null)
        //        {
        //            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"UIElement({this.GetType().FullName}) ReadMetaObject failed");
        //        }
        //        attr.EndRead();
        //        return true;
        //    }, Thread.Async.EAsyncTarget.AsyncIO);

        //    if (ret == false)
        //        return false;

        //    return await this.Initialize(rc, init);
        //}

        UIElement mTemplateUI = null;
        [Browsable(false)]
        public UIElement TemplateUI
        {
            get => mTemplateUI;
        }
        public virtual async Task CopyFromTemplate(CRenderContext rc, UIElement template)
        {
            var newInit = mInitializer.CloneObject() as UIElementInitializer;
            await Initialize(rc, newInit);
            mTemplateUI = template;
        }
        public async Task CheckAndAutoReferenceFromTemplateUVAnim(CRenderContext rc)
        {
            if (TemplateUI == null)
                return;

            if (TemplateUI.mInitializer.Version != mInitializer.Version)
                await CopyFromTemplate(rc, TemplateUI);
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual UIElement GetPointAtElement(ref EngineNS.PointF pt, bool onlyClipped = true)
        {
            if (DesignRect.Contains(ref pt))
                return this;
            return null;
        }

        public virtual Vector2 GetPointWith2DSpacePoint(ref Vector2 pt)
        {
            return new Vector2(pt.X - DesignRect.X, pt.Y - DesignRect.Y);
        }
    }
}
