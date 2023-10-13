using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Render;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace EngineNS.DesignMacross.Base.Outline
{
    public interface IOutlineElement : IRenderableElement
    {
        public string Name { get; set; }
        public List<IOutlineElement> Children { get; set; }
        public IDescription Description { get; set; }
        public void Construct();
    }
    public interface IOutlineElementsList : IOutlineElement
    {
        public INotifyCollectionChanged NotifiableDescriptions { get; set; }
    }
    public interface IOutline : IOutlineElement
    {

    }
}
