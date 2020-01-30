using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Graphics.View;
using EngineNS.IO;

namespace EngineNS.UISystem.Controls.Containers
{
    [Rtti.MetaClass]
    public class PanelInitializer : UIElementInitializer
    {
        bool mClipToBound = true;
        [Rtti.MetaData]
        [System.ComponentModel.Description("按照边框裁剪")]
        public bool ClipToBound
        {
            get => mClipToBound;
            set
            {
                mClipToBound = value;
                OnPropertyChanged("ClipToBound");
            }
        }

        public PanelInitializer()
        {
            ContainChildType = enContainChildType.MultiChild;
        }
    }
    [Editor_UIControlInit(typeof(PanelInitializer))]
    public class Panel : UIElement
    {

        protected List<UIElement> mChildrenUIElements = new List<UIElement>();
        [Browsable(false)]
       
        public List<UIElement> ChildrenUIElements => mChildrenUIElements;

        PanelInitializer mCurIniter;
        public override Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            mCurIniter = init as PanelInitializer;
            return base.Initialize(rc, init);
        }
        public override void Cleanup()
        {
            for(int i=0; i<mChildrenUIElements.Count; i++)
            {
                mChildrenUIElements[i].Cleanup();
            }
            base.Cleanup();
        }
        //public override void Save2Xnd(XndNode node)
        //{
        //    base.Save2Xnd(node);
        //    var childrenNode = node.AddNode("Children", 0, 0);
        //    for(int i=0; i<mChildrenUIElements.Count; i++)
        //    {
        //        var childNode = childrenNode.AddNode("Child", 0, 0);
        //        var headerAtt = childNode.AddAttrib("header");
        //        if(mChildrenUIElements[i] is UserControl)
        //        {
        //            headerAtt.BeginWrite();
        //            headerAtt.Write((byte)0);
        //            headerAtt.Write(((UserControl)mChildrenUIElements[i]).RName);
        //            headerAtt.EndWrite();
        //        }
        //        else
        //        {
        //            headerAtt.BeginWrite();
        //            headerAtt.Write((byte)1);
        //            var typeSStr = Rtti.RttiHelper.GetTypeSaveString(mChildrenUIElements[i].GetType());
        //            headerAtt.Write(typeSStr);
        //            headerAtt.EndWrite();
        //            mChildrenUIElements[i].Save2Xnd(childNode);
        //        }
        //    }
        //}
        //public override async Task<bool> LoadXnd(CRenderContext rc, XndNode node)
        //{
        //    var retVal = await base.LoadXnd(rc, node);
        //    if (retVal == false)
        //        return retVal;

        //    var childrenNode = node.FindNode("Children");
        //    if (childrenNode == null)
        //        return retVal;
        //    var nodes = childrenNode.GetNodes();
        //    mChildrenUIElements = new List<UIElement>(nodes.Count);
        //    for(int i=0; i<nodes.Count; i++)
        //    {
        //        var headerAtt = nodes[i].FindAttrib("header");
        //        if (headerAtt == null)
        //            continue;

        //        headerAtt.BeginRead();
        //        byte saveType;
        //        headerAtt.Read(out saveType);
        //        switch(saveType)
        //        {
        //            case 0:
        //                {
        //                    RName ctrlRName;
        //                    headerAtt.Read(out ctrlRName);
        //                    headerAtt.EndRead();
        //                    var ctrl = new UserControl();
        //                    await ctrl.LoadUIAsync(rc, ctrlRName);
        //                    AddChild(ctrl);
        //                }
        //                break;
        //            case 1:
        //                {
        //                    string typeSStr;
        //                    headerAtt.Read(out typeSStr);
        //                    headerAtt.EndRead();
        //                    var type = Rtti.RttiHelper.GetTypeFromSaveString(typeSStr);
        //                    if (type != null)
        //                    {
        //                        var ctrl = Activator.CreateInstance(type) as UIElement;
        //                        await ctrl.LoadXnd(rc, nodes[i]);
        //                        AddChild(ctrl);
        //                    }
        //                }
        //                break;
        //        }
        //    }

        //    return true;
        //}
        public override UIElement GetPointAtElement(ref PointF pt, bool onlyClipped = true)
        {
            if(onlyClipped)
            {
                if (!DesignRect.Contains(ref pt))
                    return null;
                for (int i = mChildrenUIElements.Count - 1; i >= 0; i--)
                {
                    var elem = mChildrenUIElements[i].GetPointAtElement(ref pt, onlyClipped);
                    if (elem != null)
                        return elem;
                }
                return this;
            }
            else
            {
                for(int i=mChildrenUIElements.Count - 1; i>=0; i--)
                {
                    var elem = mChildrenUIElements[i].GetPointAtElement(ref pt, onlyClipped);
                    if (elem != null)
                        return elem;
                }
                if (DesignRect.Contains(ref pt))
                    return this;
                return null;
            }
        }
        public override void ClearChildren(bool updateLayout = true)
        {
            for(int i=mChildrenUIElements.Count - 1; i>=0; i--)
            {
                var elem = mChildrenUIElements[i];
                elem.RemoveFromParent();
                elem.Slot = null;
            }
            mChildrenUIElements.Clear();
            if (updateLayout)
                UpdateLayout();
        }
        public override void AddChild(UIElement element, bool updateLayout = true)
        {
            if (element == null)
                return;
            element.RemoveFromParent();
            element.RootUIHost = RootUIHost;
            mChildrenUIElements.Add(element);
            element.SetParent(this);
            if (updateLayout)
                UpdateLayout();
        }
        public override void InsertChild(int index, UIElement element, bool updateLayout = true)
        {
            if (element == null)
                return;
            element.RemoveFromParent();
            element.RootUIHost = RootUIHost;
            if (index >= mChildrenUIElements.Count)
                mChildrenUIElements.Add(element);
            else
                mChildrenUIElements.Insert(index, element);
            element.SetParent(this);
            if (updateLayout)
                UpdateLayout();
        }
        public override void RemoveChild(UIElement element, bool updateLayout = true)
        {
            if(element != null)
            {
                element.RemoveFromParent();
                element.Slot = null;
                if(updateLayout)
                    UpdateLayout();
            }
        }

        public override bool Commit(CCommandList cmd, ref Matrix parentTransformMatrix, float dpiScale)
        {
            if (base.Commit(cmd, ref parentTransformMatrix, dpiScale) == false)
                return false;
            for(int i=0; i<mChildrenUIElements.Count; i++)
            {
                var elem = mChildrenUIElements[i];
                if (elem == null)
                    continue;
                if(mCurIniter.ClipToBound)
                {
                    if (elem.DesignClipRect.Width <= 0 || elem.DesignClipRect.Height <= 0)
                        continue;
                    elem.Commit(cmd, ref parentTransformMatrix, dpiScale);
                }
                else
                    elem.Commit(cmd, ref parentTransformMatrix, dpiScale);
            }
            return true;
        }
        public override bool Draw(CRenderContext rc, CCommandList cmd, CGfxScreenView view)
        {
            if (base.Draw(rc, cmd, view) == false)
                return false;
            for (int i = 0; i < mChildrenUIElements.Count; i++)
            {
                var elem = mChildrenUIElements[i];
                if (elem == null)
                    continue;
                if (mCurIniter.ClipToBound)
                {
                    if (elem.DesignClipRect.Width <= 0 || elem.DesignClipRect.Height <= 0)
                        continue;
                    elem.Draw(rc, cmd, view);
                }
                else
                    elem.Draw(rc, cmd, view);
            }
            return true;
        }

        #region Layout

        uint mTreeLevel = 0;
        internal override uint TreeLevel
        {
            get => mTreeLevel;
            set
            {
                mTreeLevel = value;
                for(int i=0; i<mChildrenUIElements.Count; i++)
                {
                    mChildrenUIElements[i].TreeLevel = mTreeLevel + 1;
                }
            }
        }

        internal override void MarkTreeDirty()
        {
            base.MarkTreeDirty();

            for(int i=0; i<mChildrenUIElements.Count; i++)
            {
                mChildrenUIElements[i].MarkTreeDirty();
            }
        }

        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            var retSize = SizeF.Empty;
            for (int i = 0; i < mChildrenUIElements.Count; i++)
            {
                var child = mChildrenUIElements[i];
                child.Measure(ref availableSize);
                retSize.Width = System.Math.Max(retSize.Width, child.DesiredSize.Width);
                retSize.Height = System.Math.Max(retSize.Height, child.DesiredSize.Height);
            }
            return retSize;
        }
        public override void ArrangeOverride(ref RectangleF arrangeSize)
        {
            base.ArrangeOverride(ref arrangeSize);
            for (int i = 0; i < mChildrenUIElements.Count; i++)
            {
                var child = mChildrenUIElements[i];
                child.Arrange(ref arrangeSize);
            }
        }

        #endregion

        public override UIElement FindChildElement(Int32 id)
        {
            for(int i=0; i<mChildrenUIElements.Count; i++)
            {
                var elem = mChildrenUIElements[i];
                if (elem.Id == id)
                    return elem;

                var panel = elem as Panel;
                if (panel == null)
                    continue;

                var result = panel.FindChildElement(id);
                if (result != null)
                    return result;
            }
            return null;
        }
        public override int FindChildIndex(UIElement ui)
        {
            return mChildrenUIElements.IndexOf(ui);
        }
    }
}
