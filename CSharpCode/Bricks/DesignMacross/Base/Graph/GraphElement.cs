using EngineNS;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace EngineNS.DesignMacross.Base.Graph
{
    public class TtGraphElementStyleCollection : IO.BaseSerializer
    {
        [Rtti.Meta]
        public Dictionary<Guid, IGraphElementStyle> GraphElementStyles { get; set; } = new Dictionary<Guid, IGraphElementStyle>();
        public IGraphElementStyle GetOrAdd(Guid id)
        {
            if (!GraphElementStyles.ContainsKey(id))
            {
                var style = new TtGraphElementStyle();
                GraphElementStyles.Add(id, style);
                return style;
            }
            else
            {
                return GraphElementStyles[id];
            }
        }
    }
    public class TtGraphElementStyle : IO.BaseSerializer, IGraphElementStyle
    {
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        [Rtti.Meta]
        public SizeF Size { get; set; } = new SizeF();
    }
    public abstract class TtWidgetGraphElement : IWidgetGraphElement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "WidgetGraphElement";
        public Vector2 Location { get => Style.Location; set => Style.Location = value; }
        public Vector2 AbsLocation { get => TtDesignGraphUtil.CalculateAbsLocation(this); }
        public virtual SizeF Size { get => Style.Size; set => Style.Size = value; }
        public IGraphElement Parent { get; set; } = null;
        public IDescription Description { get; set; } = null;
        public virtual IGraphElementStyle Style { get; set; } = new TtGraphElementStyle();
        public abstract bool CanDrag();
        public abstract bool HitCheck(Vector2 pos);
        public abstract void OnDragging(Vector2 delta);
        public abstract void OnSelected(ref FGraphElementRenderingContext context);
        public abstract void OnUnSelected();
    }

    public abstract class TtDescriptionGraphElement : IDescriptionGraphElement, IContextMeunable, IGraphElementDraggable, ILayoutable, IGraphElementSelectable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get => Description?.Name; set => Description.Name = value; }
        public Vector2 Location { get => Style.Location; set => Style.Location = value; }
        public Vector2 AbsLocation { get => TtDesignGraphUtil.CalculateAbsLocation(this); }
        public virtual SizeF Size { get => Style.Size; set => Style.Size = value; }
        public IGraphElement Parent { get; set; } = null;
        public IDescription Description { get; set; } = null;

        public virtual IGraphElementStyle Style { get; set; } = new TtGraphElementStyle();

        public TtDescriptionGraphElement(IDescription description, IGraphElementStyle style)
        {
            Debug.Assert(description != null, nameof(description) + "!= null");
            Id = description.Id;
            Description = description;
            if (style != null)
            {
                Style = style;
            }
        }
        public TtDescriptionGraphElement(IDescription description)
        {
            Debug.Assert(description != null, nameof(description) + "!= null");
            Id = description.Id;
            Description = description;
        }
        public virtual void ConstructElements(ref FGraphElementRenderingContext context)
        {

        }
        public void SetContextMenuableId(TtPopupMenu PopupMenu)
        {
            PopupMenu.StringId = Name + "_" + Id + "_" + "ContextMenu";
        }
        #region ISelectable
        public virtual bool HitCheck(Vector2 pos)
        {
            Rect rect = new Rect(AbsLocation, Size);
            //冗余一点
            Rect mouseRect = new Rect(pos - Vector2.One, new SizeF(1.0f, 1.0f));
            return rect.IntersectsWith(mouseRect);
        }
        public virtual void OnSelected(ref FGraphElementRenderingContext context)
        {
            context.EditorInteroperation.PGMember.Target = Description;
        }
        public virtual void OnUnSelected()
        {

        }
        #endregion ISelectable
        #region IContextMeunable
        public virtual void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu)
        {
  
        }
        #endregion IContextMeunable
        #region IDraggable
        public bool CanDrag()
        {
            return true;
        }
        public void OnDragging(Vector2 delta)
        {
            Location += delta;
        }
        #endregion IDraggable

        #region ILayoutable
        public virtual FMargin Margin { get; set; } = FMargin.Default;
        public abstract SizeF Measuring(SizeF availableSize);
        public abstract SizeF Arranging(Rect finalRect);
        #endregion ILayoutable
    }
    public class TtDescriptionGraphElementsPoolManager
    {
        public static TtDescriptionGraphElementsPoolManager Instance { get; } = new TtDescriptionGraphElementsPoolManager();
        protected Dictionary<UTypeDesc, TtDescriptionGraphElementsPool> ElementsPools = new Dictionary<UTypeDesc, TtDescriptionGraphElementsPool>();
        //public IGraphElement GetDescriptionGraphElement(UTypeDesc type, IDescription description, IGraphElementStyle elementStyle)
        //{
        //    if (!ElementsPools.ContainsKey(type))
        //    {
        //        ElementsPools.Add(type, new TtDescriptionGraphElementsPool(type));
        //    }
        //    var result = ElementsPools[type].Get(description, elementStyle);
        //    Debug.Assert(UTypeDesc.TypeOf(result.GetType()) == type);
        //    return result;
        //}
        public IDescriptionGraphElement GetDescriptionGraphElement(UTypeDesc type, IDescription description, IGraphElementStyle elementStyle)
        {
            if (!ElementsPools.ContainsKey(type))
            {
                ElementsPools.Add(type, new TtDescriptionGraphElementsPool(type));
            }
            var result = ElementsPools[type].Get(description, elementStyle) as IDescriptionGraphElement;
            Debug.Assert(UTypeDesc.TypeOf(result.GetType()) == type);
            return result;
        }
        public void Return(IGraphElement graphElement)
        {
            var elementType = UTypeDesc.TypeOf(graphElement.GetType());
            if (ElementsPools.ContainsKey(elementType))
            {
                ElementsPools[elementType].Return(graphElement);
            }
        }
    }

    public class TtDescriptionGraphElementsPool
    {
        Stack<IGraphElement> mPool = new Stack<IGraphElement>();
        UTypeDesc ElementType;
        public TtDescriptionGraphElementsPool(UTypeDesc type)
        {
            ElementType = type;
        }
        public int GrowStep
        {
            get;
            set;
        } = 10;
        public int PoolSize
        {
            get
            {
                return mPool.Count;
            }
        }
        public int AliveNumber
        {
            get;
            private set;
        } = 0;
        protected IGraphElement Create(IDescription description, IGraphElementStyle elementStyle)
        {
            return UTypeDescManager.CreateInstance(ElementType, new object[] { description, elementStyle }) as IGraphElement;
        }
        protected IGraphElement Create(IGraphElementStyle elementStyle)
        {
            return UTypeDescManager.CreateInstance(ElementType, new object[] { elementStyle }) as IGraphElement;
        }
        public IGraphElement Get(IDescription description, IGraphElementStyle elementStyle)
        {
            lock (this)
            {
                AliveNumber++;
                if (mPool.Count == 0)
                {
                    return Create(description, elementStyle);
                }
                var result = mPool.Peek() as IDescriptionGraphElement;
                result.Description = description;
                result.Style = elementStyle;
                mPool.Pop();
                return result;
            }
        }

        public bool Return(IGraphElement graphElement)
        {
            lock (this)
            {
                mPool.Push(graphElement);
                AliveNumber--;
                return true;
            }
        }
    }
}

