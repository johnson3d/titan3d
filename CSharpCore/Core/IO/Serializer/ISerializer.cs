using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO.Serializer
{
    #region Attribute
    public class EnumSizeAttribute : EngineNS.Editor.Editor_BaseAttribute
    {
        public Type SizeType;
        public EnumSizeAttribute(Type sizeType)
        {
            SizeType = sizeType;
        }

        public override object[] GetConstructParams()
        {
            return new object[] { SizeType };
        }
    }
    public class FixedSizeAttribute : EngineNS.Editor.Editor_BaseAttribute
    {
        public int FixedSize;
        public FixedSizeAttribute(int size)
        {
            FixedSize = size;
        }

        public override object[] GetConstructParams()
        {
            return new object[] { FixedSize };
        }
    }
    public class ExcelSheetAttribute : EngineNS.Editor.Editor_BaseAttribute
    {
        public string SheetName;
        public ExcelSheetAttribute(string name)
        {
            SheetName = name;
        }

        public override object[] GetConstructParams()
        {
            return new object[] { SheetName };
        }
    }
    public class DisableCloneAttribute : EngineNS.Editor.Editor_BaseAttribute
    {
        public override object[] GetConstructParams()
        {
            return new object[0];
        }
    }
    #endregion

    public interface ISerializer
    {
        void ReadObjectXML(XmlNode node);
        void WriteObjectXML(XmlNode node);
        void ReadObject(IReader pkg);
        void ReadObject(IReader pkg, Rtti.MetaData metaData);
        void WriteObject(IWriter pkg);
        void WriteObject(IWriter pkg, Rtti.MetaData metaData);
        ISerializer CloneObject();
    }

    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class Serializer : ISerializer
    {
        public virtual void BeforeRead() { }
        public virtual void BeforeWrite() { }

        public virtual void ReadObject(IReader pkg, Rtti.MetaData metaData)
        {
            SerializerHelper.ReadObject(this, pkg, metaData);
            //for (var i = 0; i < metaData.Members.Count; i++)
            //{
            //    var mbr = metaData.Members[i];
            //    if (mbr.IsList)
            //        mbr.Serializer.ReadValueList(this, mbr.PropInfo, pkg);
            //    else
            //        mbr.Serializer.ReadValue(this, mbr.PropInfo, pkg);
            //}
        }
        public void ReadObject(IReader pkg)
        {
            SerializerHelper.ReadObject(this, pkg);
        }
        
        public void WriteObject(IWriter pkg)
        {
            SerializerHelper.WriteObject(this, pkg);
        }
        public void WriteObject(IWriter pkg, Rtti.MetaData metaData)
        {
            SerializerHelper.WriteObject(this, pkg, metaData);
            //for (var i = 0; i < metaData.Members.Count; i++)
            //{
            //    var mbr = metaData.Members[i];
            //    if (mbr.IsList)
            //        mbr.Serializer.WriteValueList(this, mbr.PropInfo, pkg);
            //    else
            //        mbr.Serializer.WriteValue(this, mbr.PropInfo, pkg);
            //}
        }

        public virtual ISerializer CloneObject()
        {
            return SerializerHelper.CloneObject(this);
        }
        
        
        
        public void ReadObjectXML(XmlNode node)
        {
            SerializerHelper.ReadObjectXML(this, node);
        }
        
        public void WriteObjectXML(XmlNode node)
        {
            SerializerHelper.WriteObjectXML(this, node);
        }
    }
}
