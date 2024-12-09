using EngineNS.DesignMacross.Base.Description;
using EngineNS.Rtti;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

namespace EngineNS.DesignMacross.Base.Graph
{
    public struct FDataLineStyle
    {
        public Color4b Normal = Color4b.White;
        
        public Color4b Selected = Color4b.White;

        public FDataLineStyle()
        {
        }
    }
    public class TtDesignMacrossGraphStyles
    {
        public static float LineBezierMinDelta { get; set; } = 50;
        public static float LineBezierMaxDelta { get; set; } = 700;
        public static float LineNormalThickness = 5;
        public static float LineHighLightThickness = 9;
        public static float LineLowLightThickness = 3;
        public static float LineSelectedThickness = 7;
        static Dictionary<TtTypeDesc, FDataLineStyle> LineStyles { get; set; } = new()
        {
            { TtTypeDesc.TypeOf<bool>(), new FDataLineStyle(){ Normal = Color4b.Red, Selected = Color4b.Red } },
            { TtTypeDesc.TypeOf<int>(), new FDataLineStyle(){ Normal = Color4b.Cyan, Selected = Color4b.Cyan } },
            { TtTypeDesc.TypeOf<float>(), new FDataLineStyle(){ Normal = Color4b.Green, Selected = Color4b.Green } },
            { TtTypeDesc.TypeOf<string>(), new FDataLineStyle(){ Normal = Color4b.Magenta, Selected = Color4b.Magenta } },
            { TtTypeDesc.TypeOf<Vector3>(), new FDataLineStyle(){ Normal = Color4b.Gold, Selected = Color4b.Gold } },
        };
        public static FDataLineStyle GetDataLineStyle(TtTypeDesc typeDesc)
        {
            if(typeDesc != null && LineStyles.ContainsKey(typeDesc))
            {
                return LineStyles[typeDesc];
            }
            return new FDataLineStyle() { Normal = Color4b.Blue, Selected = Color4b.Blue };
        }
    }
    
    public class TtGraphElementStyleCollection : IO.BaseSerializer
    {
        
        [Rtti.Meta]
        public Dictionary<Guid, IGraphElementStyle> GraphElementStyles { get; set; } = new Dictionary<Guid, IGraphElementStyle>();
        public IGraphElementStyle GetOrAdd(IDescription description)
        {
            if (!GraphElementStyles.ContainsKey(description.Id))
            {
                var styleAttribute = description.GetType().GetCustomAttribute<GraphElementStyleAttribute>();
                if (styleAttribute == null)
                {
                    var style = new TtGraphElementStyle();
                    GraphElementStyles.Add(description.Id, style);
                    return style;
                }
                else
                {
                    var style = TtTypeDescManager.CreateInstance(styleAttribute.GraphElementType) as IGraphElementStyle;
                    GraphElementStyles.Add(description.Id, style);
                    return style;
                }
            }
            else
            {
                return GraphElementStyles[description.Id];
            }
        }

        public IGraphElementStyle GetOrAdd(IDescription description, Vector2 location)
        {
            if (!GraphElementStyles.ContainsKey(description.Id))
            {
                var styleAttribute = description.GetType().GetCustomAttribute<GraphElementStyleAttribute>();
                if (styleAttribute == null)
                {
                    var newStyle = new TtGraphElementStyle();
                    GraphElementStyles.Add(description.Id, newStyle);
                }
                else
                {
                    var newStyle = TtTypeDescManager.CreateInstance(styleAttribute.GraphElementType) as IGraphElementStyle;
                    GraphElementStyles.Add(description.Id, newStyle);
                }
            }
            var style = GraphElementStyles[description.Id];
            style.Location = location;
            return style;
        }
        public bool Contains(Guid id)
        {
            return GraphElementStyles.ContainsKey(id);
        }
    }
    public class TtGraphElementStyle : IO.BaseSerializer, IGraphElementStyle
    {
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        [Rtti.Meta]
        public SizeF Size { get; set; } = new SizeF();
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0, 0);
    }
    public abstract class TtWidgetGraphElement : IWidgetGraphElement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "WidgetGraphElement";
        public Vector2 Location { get => Style.Location; set => Style.Location = value; }
        public Vector2 AbsLocation { get => TtDesignGraphUtil.CalculateAbsLocation(this); }
        public virtual SizeF Size { get => Style.Size; set => Style.Size = value; }
        public virtual SizeF MinSize { get; set; }
        public virtual SizeF MaxSize { get; set; }
        public IGraphElement Parent { get; set; } = null;
        public IDescription Description { get; set; } = null;
        public virtual IGraphElementStyle Style { get; set; } = new TtGraphElementStyle();
        public abstract bool CanDrag();
        public abstract bool HitCheck(ref FMouseEventContext context);
        public abstract void OnDragging(Vector2 delta);
        public virtual void OnMouseOver(ref FMouseEventContext context)
        {
        }
        public virtual void OnMouseLeave(ref FMouseEventContext context)
        {
        }

        public virtual void OnMouseLeftButtonDown(ref FMouseEventContext context)
        {

        }

        public virtual void OnMouseLeftButtonUp(ref FMouseEventContext context)
        {
        }

        public virtual void OnMouseRightButtonDown(ref FMouseEventContext context)
        {
        }

        public virtual void OnMouseRightButtonUp(ref FMouseEventContext context)
        {
        }

        public abstract void OnSelected(ref FMouseEventContext context);
        public abstract void OnUnSelected();
    }

    public abstract class TtDescriptionGraphElement : IDescriptionGraphElement, IContextMeunable, IGraphElementDraggable, ILayoutable, IGraphElementSelectable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get => Description?.Name; set => Description.Name = value; }
        public Vector2 Location { get => Style.Location; set => Style.Location = value; }
        public Vector2 AbsLocation { get => TtDesignGraphUtil.CalculateAbsLocation(this); }
        public virtual SizeF Size { get => Style.Size; set => Style.Size = value; }
        public SizeF MinSize { get; set; }
        public SizeF MaxSize { get; set; }
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
        public virtual void AfterConstructElements(ref FGraphElementRenderingContext context)
        {

        }
        public void SetContextMenuableId(TtPopupMenu popupMenu)
        {
            popupMenu.StringId = Name + "_" + Id + "_" + "ContextMenu";
        }
        #region ISelectable
        public virtual bool HitCheck(ref FMouseEventContext context)
        {
            var renderingContext = context.GraphElementRenderingContext;
            var start = renderingContext.ViewportTransform(AbsLocation);
            var end = renderingContext.ViewportTransform(AbsLocation + new Vector2(Size.Width, Size.Height));
            Rect rect = new Rect(start.X, start.Y, end.X - start.X, end.Y - start.Y);
            //冗余一点
            Rect mouseRect = new Rect(context.MouseAbsPos - Vector2.One, new SizeF(1.0f, 1.0f));
            return rect.IntersectsWith(mouseRect);
        }
        public virtual void OnSelected(ref FMouseEventContext context)
        {
            context.GraphElementRenderingContext.EditorInteroperation.PGMember.Target = Description;
        }
        public virtual void OnUnSelected()
        {

        }
        public virtual void OnMouseOver(ref FMouseEventContext context)
        {
        }
        public virtual void OnMouseLeave(ref FMouseEventContext context)
        {

        }

        public virtual void OnMouseLeftButtonDown(ref FMouseEventContext context)
        {
        }

        public virtual void OnMouseLeftButtonUp(ref FMouseEventContext context)
        {
        }

        public virtual void OnMouseRightButtonDown(ref FMouseEventContext context)
        {
        }

        public virtual void OnMouseRightButtonUp(ref FMouseEventContext context)
        {
        }
        #endregion ISelectable
        #region IContextMeunable
        public virtual void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
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
        public EHorizontalAlignment HorizontalAlignment { get; set; } = EHorizontalAlignment.Center;
        public EVerticalAlignment VerticalAlignment { get; set; } = EVerticalAlignment.Center;

        public abstract SizeF Measuring(SizeF availableSize);
        public abstract SizeF Arranging(Rect finalRect);
        #endregion ILayoutable
    }
    public class TtDescriptionGraphElementsPoolManager
    {
        public static TtDescriptionGraphElementsPoolManager Instance { get; } = new TtDescriptionGraphElementsPoolManager();
        protected Dictionary<TtTypeDesc, TtDescriptionGraphElementsPool> ElementsPools = new Dictionary<TtTypeDesc, TtDescriptionGraphElementsPool>();
        //public IGraphElement GetDescriptionGraphElement(TtTypeDesc type, IDescription description, IGraphElementStyle elementStyle)
        //{
        //    if (!ElementsPools.ContainsKey(type))
        //    {
        //        ElementsPools.Add(type, new TtDescriptionGraphElementsPool(type));
        //    }
        //    var result = ElementsPools[type].Get(description, elementStyle);
        //    Debug.Assert(TtTypeDesc.TypeOf(result.GetType()) == type);
        //    return result;
        //}
        public IDescriptionGraphElement GetDescriptionGraphElement(TtTypeDesc type, IDescription description, IGraphElementStyle elementStyle)
        {
            if (!ElementsPools.ContainsKey(type))
            {
                ElementsPools.Add(type, new TtDescriptionGraphElementsPool(type));
            }
            var result = ElementsPools[type].Get(description, elementStyle) as IDescriptionGraphElement;
            Debug.Assert(TtTypeDesc.TypeOf(result.GetType()) == type);
            return result;
        }
        public void Return(IGraphElement graphElement)
        {
            var elementType = TtTypeDesc.TypeOf(graphElement.GetType());
            if (ElementsPools.ContainsKey(elementType))
            {
                ElementsPools[elementType].Return(graphElement);
            }
        }
    }

    public class TtDescriptionGraphElementsPool
    {
        Stack<IGraphElement> mPool = new Stack<IGraphElement>();
        TtTypeDesc ElementType;
        public TtDescriptionGraphElementsPool(TtTypeDesc type)
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
            return TtTypeDescManager.CreateInstance(ElementType, new object[] { description, elementStyle }) as IGraphElement;
        }
        protected IGraphElement Create(IGraphElementStyle elementStyle)
        {
            return TtTypeDescManager.CreateInstance(ElementType, new object[] { elementStyle }) as IGraphElement;
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

