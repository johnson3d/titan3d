using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Graphics.View;

namespace EngineNS.UISystem
{
    public class UIHostInitializer : Controls.Containers.BorderInitializer
    {

    }
    internal class UIHostSlot : Controls.Containers.UIContainerSlot
    {
        SizeF mContentDesiredSize;
        public override SizeF Measure(ref SizeF availableSize)
        {
            var size = this.Parent.DesignRect.Size;
            mContentDesiredSize = Content.MeasureOverride(ref size);
            return mContentDesiredSize;
        }
        public override void Arrange(ref RectangleF arrangeSize)
        {
            var rect = this.Parent.DesignRect;
            rect.Width = (mContentDesiredSize.Width == 0) ? rect.Width : mContentDesiredSize.Width;
            rect.Height = (mContentDesiredSize.Height == 0) ? rect.Height : mContentDesiredSize.Height;
            Content.ArrangeOverride(ref rect);
        }
        public override void ProcessSetContentDesignRect(ref RectangleF tagRect)
        {
            Content.SetDesignRect(ref tagRect);
        }
        public override bool NeedUpdateLayoutWhenChildDesiredSizeChanged(UIElement child)
        {
            return false;
        }
    }
    // 界面容器，所有的界面都需要通过UIHost包装，可以有多个
    public partial class UIHost : Controls.Containers.Panel
    {
        EngineNS.SizeF mWindowSize = new SizeF(1920, 1080);
        public EngineNS.SizeF WindowSize
        {
            get => mWindowSize;
            set
            {
                var oldWinSize = mWindowSize;
                if (oldWinSize.Equals(ref value))
                    return;
                mWindowSize = value;
                SizeF tagDesignSize;
                mDpiScale = EngineNS.CEngine.Instance.UIManager.Config.GetDPIScaleAndDesignSize(mWindowSize.Width, mWindowSize.Height, out tagDesignSize);
                var newRect = new EngineNS.RectangleF(0, 0, tagDesignSize.Width, tagDesignSize.Height);
                SetDesignRect(ref newRect, true);
                for(int i=0; i<mChildrenUIElements.Count; i++)
                {
                    var child = mChildrenUIElements[i];
                    child.UpdateLayout();
                }
            }
        }

        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            return WindowSize;
        }
        public override void ArrangeOverride(ref RectangleF arrangeSize)
        {
            for (int i = 0; i < mChildrenUIElements.Count; i++)
            {
                var child = mChildrenUIElements[i];
                child.Arrange(ref arrangeSize);
            }
        }
        public override void OnChildDesiredSizeChanged(UIElement child)
        {

        }

        public override async Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            if (await base.Initialize(rc, init) == false)
                return false;

            IsLayoutIslandRoot = true;
            DesignRect = new RectangleF(0, 0, mWindowSize.Width, mWindowSize.Height);
            DesignClipRect = DesignRect;
            return true;
        }

        List<Controls.Containers.Popup> mPopupedElements = new List<Controls.Containers.Popup>();
        public Controls.Containers.Popup PopupedUIElement(ref PointF pt)
        {
            for(int i=mPopupedElements.Count - 1; i >= 0; --i)
            {
                var ppUI = mPopupedElements[i];
                if (ppUI.IsPointIn(ref pt))
                    return ppUI;
            }
            return null;
        }

        public override bool Commit(CCommandList cmd, ref Matrix parentTransformMatrix, float dpiScale)
        {
            for (int i = 0; i < mChildrenUIElements.Count; i++)
            {
                var elem = mChildrenUIElements[i];
                if (elem == null)
                    continue;
                if (elem.DesignClipRect.Width <= 0 || elem.DesignClipRect.Height <= 0)
                    continue;
                elem.Commit(cmd, ref parentTransformMatrix, dpiScale);
            }
            return true;
        }
        float mDpiScale = 1.0f;
        public float DpiScale => mDpiScale;
        public void Commit(EngineNS.CCommandList cmd)
        {
            Matrix mat = Matrix.Identity;
            Commit(cmd, ref mat, mDpiScale);
        }
        public override bool Draw(CRenderContext rc, CCommandList cmd, CGfxScreenView view)
        {
            using(var i = mAttachments.Values.GetEnumerator())
            {
                while(i.MoveNext())
                {
                    i.Current.BeforeDrawUI(rc, cmd, view);
                }
            }
            for(int i=0; i<mChildrenUIElements.Count; i++)
            {
                var elem = mChildrenUIElements[i];
                if (elem == null)
                    continue;

                if (elem.DesignClipRect.Width <= 0 || elem.DesignClipRect.Height <= 0)
                    continue;
                elem.Draw(rc, cmd, view);
            }
            using (var i = mAttachments.Values.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    i.Current.AfterDrawUI(rc, cmd, view);
                }
            }
            return true;
        }
        Dictionary<string,UIHostAttachments> mAttachments = new Dictionary<string, UIHostAttachments>();
        public void AddAttachments(string name, UIHostAttachments exec)
        {
            mAttachments[name] = exec;
        }
        public void RemoveDetachments(string name)
        {
            mAttachments.Remove(name);
        }
        public UIHostAttachments GetAttachments(string name)
        {
            UIHostAttachments exec;
            if(mAttachments.TryGetValue(name, out exec))
            {
                return exec;
            }
            return null;
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async System.Threading.Tasks.Task AddChildWithRName(
            [EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.UISystem.UIElement))]
            RName name)
        {
            var element = await EngineNS.CEngine.Instance.UIManager.GetUIAsync(name);
            if (element == null)
                return;
            AddChild(element);
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: EngineNS.Editor.Editor_TypeChangeWithParam(1)]
        public async System.Threading.Tasks.Task<UIElement> AddChildWithRNameWithReturn(
            [EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.UISystem.UIElement))]
            RName name,
            [EngineNS.Editor.Editor_TypeFilterAttribute(typeof(UIElement))]
            System.Type type)
        {
            var element = await EngineNS.CEngine.Instance.UIManager.GetUIAsync(name);
            if (element == null)
                return null;
            AddChild(element);
            return element;
        }
        public override void AddChild(UIElement element, bool updateLayout = true)
        {
            element.RootUIHost = this;
            base.AddChild(element, updateLayout);
            //if(element.Slot != null)
            //{
            //    System.Diagnostics.Debug.Assert(element.Slot.GetType() == typeof(UIHostSlot));
            //    element.Slot.Parent = this;
            //    element.Slot.Content = element;
            //}
            //else
            {
                var slot = new UIHostSlot();
                slot.Parent = this;
                slot.Content = element;
                element.Slot = slot;
                element.UpdateLayout();
            }
        }
        public override void InsertChild(int index, UIElement element, bool updateLayout = true)
        {
            element.RootUIHost = this;
            base.InsertChild(index, element, updateLayout);
            //if (element.Slot != null)
            //{
            //    System.Diagnostics.Debug.Assert(element.Slot.GetType() == typeof(UIHostSlot));
            //    element.Slot.Parent = this;
            //    element.Slot.Content = element;
            //}
            //else
            {
                var slot = new UIHostSlot();
                slot.Parent = this;
                slot.Content = element;
                element.Slot = slot;
                element.UpdateLayout();
            }
        }

        public override Vector2 GetPointWith2DSpacePoint(ref Vector2 pt)
        {
            Vector2 retPt = Vector2.Zero;
            retPt.X = pt.X / mDpiScale - DesignRect.X;
            retPt.Y = pt.Y / mDpiScale - DesignRect.Y;
            return retPt;
        }
    }

    public class UIHostAttachments
    {
        public virtual void BeforeDrawUI(CRenderContext rc, CCommandList cmd, CGfxScreenView view)
        {

        }
        public virtual void AfterDrawUI(CRenderContext rc, CCommandList cmd, CGfxScreenView view)
        {

        }
    }
}

namespace EngineNS.Graphics.View
{
    public partial class CGfxSceneView
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public UISystem.UIHost UIHost
        {
            get;
            set;
        }
    }
}
