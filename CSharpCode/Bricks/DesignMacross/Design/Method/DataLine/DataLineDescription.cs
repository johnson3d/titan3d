using EngineNS.DesignMacross.Base.Description;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    public class TtDataPinDescription : IDescription
    {
        public IDescription Parent { get; set; }
        public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
    public class TtDataLineDescription : IDescription
    {
        public IDescription Parent { get; set; }
        public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}
