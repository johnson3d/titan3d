using EngineNS.Bricks.CodeBuilder;
using EngineNS.IO;
using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Controls.Containers;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace EngineNS.UI.Controls
{
    public enum HorizontalAlignment : sbyte
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Stretch = 3,
    }
    public enum VerticalAlignment : sbyte
    {
        Top = 0,
        Center = 1,
        Bottom = 2,
        Stretch = 3,
    }
    public enum Visibility : sbyte
    {
        Visible = 0,
        Hidden,
        Collapsed,
    }
    [Bind.BindableObject]
    public partial class TtUIElement : IO.ISerializer
    {
        static partial void TtUIELement_Template();
        static TtUIElement()
        {
            TtUIELement_Template();
        }

        [Flags]
        internal enum ECoreFlags : uint
        {
            None = 0,
            MeasureDirty = 1 << 0,
            ArrangeDirty = 1 << 1,
            MeasureInProgress = 1 << 2,
            ArrangeInProgress = 1 << 3,
            NeverMeasured = 1 << 4,
            NeverArranged = 1 << 5,
            MeasureDuringArrange = 1 << 6,
            IsLayoutIslandRoot = 1 << 7,
            UseRounding = 1 << 8,           // aligns to pixel boundaries
            BypassLayoutPolicies = 1 << 9,

            IsFocusable = 1 << 10,
            IsMouseOver = 1 << 11,
            IsTouchOver = 1 << 12,
            IsSpaceKeyDown = 1 << 13,

            Is3D = 1 << 14,
            IsScreenSpace = 1 << 15,
        }
        private ECoreFlags mCoreFlags;
        internal bool ReadFlag(ECoreFlags flag)
        {
            return (mCoreFlags & flag) != 0;
        }
        internal void WriteFlag(ECoreFlags flag, bool value)
        {
            if (value)
                mCoreFlags |= flag;
            else
                mCoreFlags &= ~flag;
        }

        [Browsable(false)]
        public bool Is3D
        {
            get { return ReadFlag(ECoreFlags.Is3D); }
            set { WriteFlag(ECoreFlags.Is3D, value); }
        }

        internal enum eInternalFlags : uint
        {
            TemplateIndexDefault = 0xFFFF,

            HasTemplateGeneratedSubTree = 1 << 16,
            ChildIsContentsPresenter = 1 << 17,
            IsLogicalChildrenIterationInProgress = 1 << 18,
            HasLogicalChildren = 1 << 19,
            MeshDirty = 1 << 20,
        }
        eInternalFlags mInternalFlags;
        internal bool ReadInternalFlag(eInternalFlags flag)
        {
            return (mInternalFlags & flag) != 0;
        }
        internal void WriteInternalFlag(eInternalFlags flag, bool set)
        {
            if (set)
                mInternalFlags |= flag;
            else
                mInternalFlags &= ~flag;
        }
        internal bool HasTemplateGeneratedSubTree
        {
            get => ReadInternalFlag(eInternalFlags.HasTemplateGeneratedSubTree);
            set => WriteInternalFlag(eInternalFlags.HasTemplateGeneratedSubTree, value);
        }
        internal bool HasLogicalChildren
        {
            get => ReadInternalFlag(eInternalFlags.HasLogicalChildren);
            set => WriteInternalFlag(eInternalFlags.HasLogicalChildren, value);
        }

        UInt64 mId = 0;
        [Rtti.Meta]
        public UInt64 Id
        {
            get
            {
                if (mId == 0)
                    mId = Standart.Hash.xxHash.xxHash64.ComputeHash(Guid.NewGuid().ToByteArray());
                return mId;
            }
            set => mId = value;
        }

        partial void TtUIElementConstructor_Template();
        partial void TtUIElementConstructor_Editor();
        public TtUIElement()
        {
            NeverMeasured = true;
            NeverArranged = true;
            TtUIElementConstructor_Template();
            TtUIElementConstructor_Editor();

            
        }
        public TtUIElement(TtContainer parent)
        {
            mParent = parent;
            NeverMeasured = true;
            NeverArranged = true;
            TtUIElementConstructor_Template();
            TtUIElementConstructor_Editor();
        }

        string mName;
        [Bind.BindProperty]
        [Rtti.Meta]
        public string Name  // Name需要保证在当前编辑上下文的所有控件中唯一
        {
            get => mName;
            set
            {
                OnValueChange(value, mName);
                mName = value;
            }
        }

        bool mIsEnabled = true;
        [Bind.BindProperty]
        [Rtti.Meta]
        public bool IsEnabled
        {
            get => mIsEnabled;
            set
            {
                OnValueChange(value, mIsEnabled);
                mIsEnabled = value;
            }
        }

        TtUIHost mRootUIHost;
        [Browsable(false)]
        public TtUIHost RootUIHost
        {
            get
            {
                if(mRootUIHost == null)
                {
                    var parent = this;
                    while(parent != null)
                    {
                        if(parent is TtUIHost)
                        {
                            mRootUIHost = parent as TtUIHost;
                            break;
                        }
                        parent = parent.Parent;
                    }
                }
                return mRootUIHost;
            }
            set { mRootUIHost = value; }
        }
        internal UI.Controls.Containers.TtContainer mVisualParent;
        // default is logic parent
        internal UI.Controls.Containers.TtContainer mParent;
        [Browsable(false)]
        public UI.Controls.Containers.TtContainer Parent
        {
            get => mParent;
            //set
            //{
            //    if (mParent == value)
            //        return;
            //    if (mParent != null) 
            //    {
            //        RemoveFromParent();
            //    }
            //    mParent = value;
            //}
        }
        //internal void RemoveFromParent()
        //{
        //    if (mParent != null)
        //    {
        //        RemoveAttachedProperties(mParent.GetType());
        //        mParent.Children.Remove(this);
        //    }
        //    mParent = null;
        //}

        /// <summary>
        /// 获取UI的设计父，区别于Logic Parent和Visual Parent，
        /// Design Parent是用来接受无Visual Parent的路由事件的，
        /// 比如由ComboBox弹出Popup，Popup与ComboBox无Visual或
        /// Logic父子关系，但是能够接受路由事件
        /// </summary>
        /// <returns></returns>
        protected virtual TtUIElement GetUIDesignParent()
        {
            return null;
        }

        public virtual void Cleanup()
        {

        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml) { }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml) { }

        Visibility mVisibility = Visibility.Visible;
        [BindProperty]
        [Rtti.Meta]
        public Visibility Visibility
        {
            get => mVisibility;
            set
            {
                OnValueChange(value, mVisibility);
                mVisibility = value;
            }
        }
        public virtual bool IsMousePointIn(in Vector2 mousePoint)
        {
            // todo: inv transform
            return DesignRect.Contains(in mousePoint);
        }
        public virtual TtUIElement PickElement(in Ray pickRay, in Vector2 posInHost)
        {
            return null;
        }
        public virtual TtUIElement GetPointAtElement(in Vector2 pt, bool onlyClipped = true)
        {
            if(IsMousePointIn(in pt))
                return this;
            return null;
        }
        TtUIElement Get3DParent()
        {
            if (Is3D)
                return this;
            var parent = VisualTreeHelper.GetParent(this);
            if (parent == null)
                return this;
            return parent.Get3DParent();
        }
        public void PositionInParent(ref Vector2 pos, TtUIElement parent)
        {
            if(parent == this)
            {
                pos.X = 0;
                pos.Y = 0;
                return;
            }
            var pe = VisualTreeHelper.GetParent(this);
            if (pe == parent)
            {
                pos.X += DesignRect.X;
                pos.Y += DesignRect.Y;
            }
            else
            {
                pos.X += DesignRect.X;
                pos.Y += DesignRect.Y;
                pe.PositionInParent(ref pos, parent);
            }
        }
        public virtual bool GetElementPointAtPos(in Vector2 mousePt, out Vector2 tagPos)
        {
            var p3d = Get3DParent();
            Vector3 dir = Vector3.Zero;
            var vp = RootUIHost.SceneNode.GetViewport();
            var delta = vp.WindowPos - vp.ViewportPos;
            RootUIHost.RenderCamera.GetPickRay(ref dir, mousePt.X - delta.X, mousePt.Y - delta.Y, vp.ClientSize.Width, vp.ClientSize.Height);
            if(dir == Vector3.Zero)
            {
                tagPos = mousePt;
                return false;
            }
            var data = new RayIntersectData()
            {
                Start = RootUIHost.RenderCamera.GetLocalPosition(),
                Direction = dir,
            };
            var ray = new Ray(data.Start, data.Direction);
            if(!p3d.RayIntersect(ref data))
            {
                tagPos = mousePt;
                return false;
            }

            Vector2 pos = Vector2.Zero;
            PositionInParent(ref pos, p3d);
            var posInElement = data.IntersectPos - pos;
                
            tagPos = new Vector2(posInElement.X - mDesignRect.Location.X, posInElement.Y - mDesignRect.Location.Y);
            if (IsMousePointIn(in posInElement))
                return true;
            else
                return false;
        }

        protected RectangleF mDesignRect;
        [Browsable(false)]
        [Rtti.Meta]
        public RectangleF DesignRect
        {
            get => mDesignRect;
            protected set
            {
                if(!mDesignRect.Equals(in value))
                {
                    mDesignRect = value;
                    mClipRectDirty = true;
                }
            }
        }
        public void SetDesignRect(in RectangleF rect, bool updateClipRect = false)
        {
            mDesignRect = rect;
            if (updateClipRect)
                UpdateDesignClipRect();
            UpdateLayout();
        }
        protected RectangleF mDesignClipRect = new RectangleF(-float.PositiveInfinity, -float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        [Browsable(false)]
        public RectangleF DesignClipRect
        {
            get
            {
                if (mClipRectDirty)
                    UpdateDesignClipRect();
                return mDesignClipRect;
            }
            protected set
            {
                mDesignClipRect = value;
            }
        }
        bool mClipRectDirty = true;
        protected void UpdateDesignClipRect()
        {
            var oldClipRect = mDesignClipRect;
            var parent = VisualTreeHelper.GetParent(this);
            if (parent != null)
            {
                var parentClipRect = parent.DesignClipRect;
                mDesignClipRect = DesignRect.Intersect(in parentClipRect);
            }
            else
            {
                mDesignClipRect = new RectangleF(DesignRect.Left, DesignRect.Top, System.Math.Abs(DesignRect.Width), System.Math.Abs(DesignRect.Height));
            }
            mClipRectDirty = false;
        }

        public virtual Vector2 GetPointWith2DSpacePoint(in Vector2 pt)
        {
            return new Vector2(pt.X - DesignRect.X, pt.Y - DesignRect.Y);
        }

        public delegate bool Delegate_QueryProcess<T>(TtUIElement element, ref T data);
        public virtual bool QueryElements<T>(Delegate_QueryProcess<T> queryAction, ref T queryData)
        {
            return (queryAction?.Invoke(this, ref queryData) == true);
        }

        public void TransformVertex3(in Vector3 inVec, out Vector3 outVec)
        {
            outVec = Vector3.TransformCoordinate(in inVec, in RootUIHost.TransformedElements[mTransformIndex].Matrix);
        }

        public struct RayIntersectData
        {
            public Vector3 Start = Vector3.Zero;
            public Vector3 Direction = Vector3.UnitX;
            public Vector2 IntersectPos = Vector2.Zero;
            public float Distance = float.MaxValue;
            public TtUIElement IntersectedElement = null;

            public RayIntersectData()
            {

            }
        }
        public virtual bool RayIntersect(ref RayIntersectData data)
        {
            var ray = new Ray(data.Start, data.Direction);
            // 0 ---- 1
            // |      |
            // 2 ---- 3
            var top = (this == RootUIHost)? mCurFinalRect.Top : (RootUIHost.WindowSize.Height - mCurFinalRect.Top);
            var bottom = (this == RootUIHost)? mCurFinalRect.Bottom : (RootUIHost.WindowSize.Height - mCurFinalRect.Bottom);
            var v0 = new Vector3(mCurFinalRect.Left, top, 0.0f);
            TransformVertex3(in v0, out v0);
            var v1 = new Vector3(mCurFinalRect.Right, top, 0.0f);
            TransformVertex3(in v1, out v1);
            var v2 = new Vector3(mCurFinalRect.Left, bottom, 0.0f);
            TransformVertex3(in v2, out v2);
            var v3 = new Vector3(mCurFinalRect.Right, bottom, 0.0f);
            TransformVertex3(in v3, out v3);
            float distance, barycentricU, barycentricV;
            if (Ray.Intersects(in ray, v0, v1, v2, out distance, out barycentricU, out barycentricV))
            {
                data.IntersectPos.X = v0.X + barycentricU * (v1.X - v0.X) + (1 - barycentricV) * (v2.X - v0.X);
                data.IntersectPos.Y = v0.Y + barycentricU * (v1.Y - v0.Y) + (1 - barycentricV) * (v2.Y - v0.Y);
                data.IntersectPos = Vector2.TransformCoordinate(data.IntersectPos,
                    in RootUIHost.TransformedElements[mTransformIndex].InvMatrix);
                if(this != RootUIHost)
                    data.IntersectPos.Y = RootUIHost.WindowSize.Height - data.IntersectPos.Y;
                data.Distance = distance;
                //UEngine.Instance.UIManager.DebugHitPt = data.IntersectPos;
                return true;
            }
            else if (Ray.Intersects(in ray, v3, v2, v1, out distance, out barycentricU, out barycentricV))
            {
                data.IntersectPos.X = v3.X + barycentricU * (v2.X - v3.X) + (1 - barycentricV) * (v1.X - v3.X);
                data.IntersectPos.Y = v3.Y + barycentricU * (v2.Y - v3.Y) + (1 - barycentricV) * (v1.Y - v3.Y);
                data.IntersectPos = Vector2.TransformCoordinate(data.IntersectPos,
                    in RootUIHost.TransformedElements[mTransformIndex].InvMatrix);
                if(this != RootUIHost)
                    data.IntersectPos.Y = RootUIHost.WindowSize.Height - data.IntersectPos.Y;
                data.Distance = distance;
                //UEngine.Instance.UIManager.DebugHitPt = data.IntersectPos;
                return true;
            }
            return false;
        }

        public virtual void MergeAABB(ref BoundingBox aabb)
        {
            var top = RootUIHost.WindowSize.Height - mCurFinalRect.Top;
            var bottom = RootUIHost.WindowSize.Height - mCurFinalRect.Bottom;
            var v0 = new Vector3(mCurFinalRect.Left, top, 0.0f);
            TransformVertex3(in v0, out v0);
            var v1 = new Vector3(mCurFinalRect.Right, top, 0.0f);
            TransformVertex3(in v1, out v1);
            var v2 = new Vector3(mCurFinalRect.Left, bottom, 0.0f);
            TransformVertex3(in v2, out v2);
            var v3 = new Vector3(mCurFinalRect.Right, bottom, 0.0f);
            TransformVertex3(in v3, out v3);

            aabb.Merge(v0);
            aabb.Merge(v1);
            aabb.Merge(v2);
            aabb.Merge(v3);
        }

        public virtual void Tick(float elapsedSecond)
        {
        }

        class AttachedPropertiesSaverAttribute : IO.UCustomSerializerAttribute
        {
            public override void Save(IWriter ar, object host, string propName)
            {
                UInt16 count = 0;
                var element = host as TtUIElement;
                var countOffset = ar.GetPosition();
                ar.Write(count);
                foreach(var expr in element.mBindExprDic)
                {
                    if(expr.Key.IsAttachedProperty)
                    {
                        count++;
                        ar.Write(Rtti.UTypeDescManager.Instance.GetTypeStringFromType(expr.Key.HostType.SystemType));
                        ar.Write(expr.Key.Name);
                        ar.Write(Rtti.UTypeDescManager.Instance.GetTypeStringFromType(expr.Key.PropertyType.SystemType));
                        var offset = SerializerHelper.WriteSkippable(ar);
                        var value = expr.Value.GetValue<object>(expr.Key);
                        SerializerHelper.WriteObject(ar, expr.Key.PropertyType.SystemType, value);
                        SerializerHelper.SureSkippable(ar, offset);
                    }
                }
                var cur = ar.GetPosition();
                ar.Seek(countOffset);
                ar.Write(count);
                ar.Seek(cur);
            }
            public override object Load(IReader ar, object host, string propName)
            {
                var element = host as TtUIElement;
                UInt16 count;
                ar.Read(out count);
                for(int i=0; i<count; i++)
                {
                    string hostTypeName;
                    ar.Read(out hostTypeName);
                    string proName;
                    ar.Read(out proName);
                    string proType;
                    ar.Read(out proType);
                    var skipPoint = SerializerHelper.GetSkipOffset(ar);
                    try
                    {
                        var hostType = EngineNS.Rtti.UTypeDesc.TypeOf(hostTypeName);
                        if(hostType == null)
                        {
                            throw new EngineNS.IO.IOException($"Read attacked property: host type {hostTypeName} is missing");
                        }
                        var type = EngineNS.Rtti.UTypeDesc.TypeOf(proType).SystemType;
                        if(type == null)
                        {
                            throw new EngineNS.IO.IOException($"Read attacked property: property type {proType} is missing");
                        }
                        var valObj = EngineNS.IO.SerializerHelper.ReadObject(ar, type, host);
                        var pro = EngineNS.UEngine.Instance.UIBindManager.FindBindableProperty(proName, hostType);
                        if (pro == null)
                            continue;
                        element.mLoadedAttachedValues[pro] = valObj;
                    }
                    catch(Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        ar.Seek(skipPoint);
                    }
                }

                return null;
            }
        }
        [Rtti.Meta, AttachedPropertiesSaverAttribute]
        [Browsable(false)]
        public object AttachedPropertiesSaver
        {
            get => null;
            set {; }
        }
        Dictionary<TtBindableProperty, object> mLoadedAttachedValues = new Dictionary<TtBindableProperty, object>();
        public void SetLoadedAttachedValues()
        {
            foreach(var pv in mLoadedAttachedValues)
            {
                TtBindablePropertyValueBase value;
                if(mBindExprDic.TryGetValue(pv.Key, out value))
                {
                    value.SetValue<object>(this, pv.Key, pv.Value);
                }
            }
            mLoadedAttachedValues.Clear();
        }

        public virtual TtUIElement FindElement(string name)
        {
            if (mName == name)
                return this;
            return null;
        }
        public virtual TtUIElement FindElement(UInt64 id)
        {
            if(mId == id)
                return this;
            return null;
        }
    }
}
