using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Editor;
using EngineNS.Rtti;
using System.ComponentModel;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    public class TtDataPinDescriptionElementStyle : TtGraphElementStyle
    {


    }

    public class TtDataInPinDescriptionElementStyle : TtGraphElementStyle
    {
        public TtGraphElement_PinEditableBox PinEditableBox { get; set; } = new();
    }

    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    [GraphElementStyle(typeof(TtDataPinDescriptionElementStyle))]
    public class TtDataPinDescription : TtPinDescription
    {

    }
    [GraphElement(typeof(TtGraphElement_DataInPin))]
    [GraphElementStyle(typeof(TtDataInPinDescriptionElementStyle))]
    public class TtDataInPinDescription : TtDataPinDescription
    {
        [Rtti.Meta("")]
        public string TypeVaule { get; set; } = null;
        [Category("Option")]
        public object Vaule 
        {
            get
            {
                return GetTypeValue();
            }
            set
            {
                TypeVaule = value.ToString();
            }
        }
        public object GetTypeValue()
        {
            if (TypeVaule == null)
            {
                TypeVaule = GetDefaultVale(TypeDesc).ToString();
            }
            return TypeVaule;
        }
        public static object GetDefaultVale(TtTypeDesc typeDesc)
        {
            if (typeDesc.SystemType == typeof(bool))
            {
                return false;
            }
            else if (typeDesc.SystemType == typeof(SByte) ||
                typeDesc.SystemType == typeof(Int16)  ||
                typeDesc.SystemType == typeof(Int32)  ||
                typeDesc.SystemType == typeof(byte)   ||
                typeDesc.SystemType == typeof(UInt16) ||
                typeDesc.SystemType == typeof(UInt32) ||
                typeDesc.SystemType == typeof(UInt64) ||
                typeDesc.SystemType == typeof(Int64)  ||
                typeDesc.SystemType == typeof(float)  ||
                typeDesc.SystemType == typeof(double))
            {
                return 0;
            }
            else if (typeDesc.SystemType == typeof(Vector2))
            {
                return Vector2.Zero;
            }
            else if (typeDesc.SystemType == typeof(Vector3))
            {
                return Vector3.Zero;
            }
            else if (typeDesc.SystemType == typeof(Vector4))
            {
                return Vector4.Zero;
            }
            else if (typeDesc.SystemType == typeof(string))
            {
                return "";
            }
            else if (typeDesc.SystemType == typeof(Color4b))
            {
                return Color4b.Black;
            }
            return null;
        }
    }
    [GraphElement(typeof(TtGraphElement_DataPin))]
    public class TtDataOutPinDescription : TtDataPinDescription
    {

    }


    [GraphElement(typeof(TtGraphElement_DataLine))]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtDataLineDescription : TtLineDescription
    {

    }
}
