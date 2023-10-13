using EngineNS.DesignMacross.Base.Description;
using System;
using System.Collections.Generic;

namespace EngineNS.DesignMacross.Base.Graph
{

    public abstract class TtGraph : IGraph, IDraggable, ISelectable, IEnumChild
    {
        public Guid Id { get; set; } = Guid.Empty;
        public TtGraphViewPort ViewPort { get; set; } = new TtGraphViewPort();
        public TtGraphCamera Camera { get; set; } = new TtGraphCamera();
        public string Name { get=>Description.Name; set=>Description.Name = value; }
        public Vector2 Location { get; set; } = Vector2.Zero;
        public Vector2 AbsLocation { get => TtGraphMisc.CalculateAbsLocation(this); }
        public SizeF Size { get; set; } = new SizeF(100,100);

        public IGraphElement Parent { get; set; } = null;
        public virtual IDescription Description { get; set; } = null;
        [Rtti.Meta]
        public List<IGraphElement> Elements { get; set; } = new List<IGraphElement>();
        
        public TtGraph(IDescription description)
        {
            Id = description.Id;
            Description = description;
        }
        public virtual void Construct()
        {
        }
        #region IDraggable
        public bool CanDrag()
        {
            return true;
        }
        public void OnDragging(Vector2 delta)
        {
            Camera.Location -= delta;
        }
        #endregion IDraggable
        #region ISelectable

        public bool HitCheck(Vector2 pos)
        {
            Rect rect = new Rect(Location, Size);
            //冗余一点
            Rect mouseRect = new Rect(pos - Vector2.One, new SizeF(1.0f, 1.0f));
            return rect.IntersectsWith(mouseRect);
        }
        public void OnSelected()
        {

        }
        public void OnUnSelected()
        {
           
        }
        #endregion ISelectable
        public virtual List<IGraphElement> EnumerateChild<T>() where T : class
        {
            List<IGraphElement> list = new List<IGraphElement>();
            foreach (var element in Elements)
            {
                if (element is T)
                {
                    list.Add(element);
                }
            }
            return list;
        }
    }
}
