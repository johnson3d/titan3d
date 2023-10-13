using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Base.Graph.Elements
{
    public enum EOrientation
    {
        Horizontal,
        Vertical,
    }
    public struct TtDesignStackPanelStyle
    {
        public TtDesignStackPanelStyle()
        {

        }


    }
    [ImGuiElementRender(typeof(TtGraphElementRender_StackPanel))]
    public class TtGraphElement_StackPanel : IGraphElement, ILayoutable
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = "StackPanel";
        public Vector2 Location { get; set; } = Vector2.Zero;
        public Vector2 AbsLocation { get => TtGraphMisc.CalculateAbsLocation(this); }
        public SizeF Size { get; set; } = new SizeF(200, 100);
        public IGraphElement Parent { get; set; } = null;
        public IDescription Description { get; set; } = null;
        public EOrientation Orientation { get; set; } = EOrientation.Vertical;
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0);
        public List<IGraphElement> Children { get; set; } = new List<IGraphElement>();
        public FMargin Margin { get; set; } = FMargin.Default;
        public void AddElement(IGraphElement element)
        {
            Children.Add(element);
            element.Parent = this;
        }
        public void RemoveElement(IGraphElement element)
        {
            element.Parent = null;
            Children.Remove(element);
        }

        public void Clear()
        {
            foreach(var element in Children)
            {
                element.Parent = null;
            }
            Children.Clear();
        }

        public TtGraphElement_StackPanel()
        {

        }
        public void Construct()
        {
        }
        public bool HitCheck(Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public void OnSelected()
        {
            throw new NotImplementedException();
        }

        public void OnUnSelected()
        {
            throw new NotImplementedException();
        }

        private Dictionary<ILayoutable, SizeF> ChildrenMeasuringSize = new Dictionary<ILayoutable, SizeF>();
        public SizeF Measuring(SizeF availableSize)
        {
            var childrenDesiredSize = new SizeF();
            foreach (var element in Children)
            {
                if (element is ILayoutable layoutable)
                {
                    var childDesireSize = layoutable.Measuring(availableSize);
                    if(ChildrenMeasuringSize.ContainsKey(layoutable))
                    {
                        ChildrenMeasuringSize[layoutable] = childDesireSize;
                    }
                    else
                    {
                        ChildrenMeasuringSize.Add(layoutable, childDesireSize);
                    }
                    if (Orientation == EOrientation.Vertical)
                    {
                        childrenDesiredSize.Height += childDesireSize.Height;
                        if (childrenDesiredSize.Width < childDesireSize.Width)
                        {
                            childrenDesiredSize.Width = childDesireSize.Width;
                        }
                    }
                    else
                    {
                        childrenDesiredSize.Width += childDesireSize.Width;
                        if (childrenDesiredSize.Height < childDesireSize.Height)
                        {
                            childrenDesiredSize.Height = childDesireSize.Height;
                        }
                    }
                }
            }

            return new SizeF(childrenDesiredSize.Width + Margin.Left + Margin.Right, childrenDesiredSize.Height + Margin.Top + Margin.Bottom);
        }

        public SizeF Arranging(Rect finalRect)
        {
            Size = new SizeF(finalRect.Width, finalRect.Height);
            Location = finalRect.Location + new Vector2(Margin.Left, Margin.Top);

            var nextElementLocation = Vector2.Zero;
            foreach (var child in Children)
            {
                var childMeasuringSize = new SizeF();
                if(child is ILayoutable layoutable)
                {
                    childMeasuringSize = ChildrenMeasuringSize[layoutable];
                    var finaleSize = childMeasuringSize;
                    if (Orientation == EOrientation.Vertical)
                    {
                        finaleSize.Width = Size.Width;
                    }
                    else
                    {
                        finaleSize.Height = Size.Height;
                    }
                    var rect = new Rect(nextElementLocation, finaleSize);
                    layoutable.Arranging(rect);
                }
                //TODO: need to deal with not ILayoutable element

                if (Orientation == EOrientation.Vertical)
                {
                    nextElementLocation.Y += childMeasuringSize.Height;
                }
                else
                {
                    nextElementLocation.X += childMeasuringSize.Width;
                }
            }
            return finalRect.Size;
        }
    }

    public class TtGraphElementRender_StackPanel : IGraphElementRender
    {
        public TtGraphElement_StackPanel stackPanel { get; set; } = null;
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            TtGraphElement_StackPanel stackPanel = renderableElement as TtGraphElement_StackPanel;
            var cmd = ImGuiAPI.GetWindowDrawList();
            foreach (var child in stackPanel.Children)
            {
                var render = TtElementRenderDevice.CreateGraphElementRender(child);
                if (render != null)
                {
                    render.Draw(child, ref context);
                } 
            }
        }
    }
}
