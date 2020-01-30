using System;

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace EngineNS.Design
{
    /// <summary>
    /// 包围盒转换类
    /// </summary>
    public class BoundingBoxConverter : System.ComponentModel.ExpandableObjectConverter
	{
		private PropertyDescriptorCollection m_Properties;
        /// <summary>
        /// 构造函数
        /// </summary>
        public BoundingBoxConverter()
	    {
		    Type type = typeof(BoundingBox);
            PropertyDescriptor[] propArray = new PropertyDescriptor[]
		    {
			    new FieldPropertyDescriptor(type.GetField("Minimum")),
			    new FieldPropertyDescriptor(type.GetField("Maximum")),
		    };

		    m_Properties = new PropertyDescriptorCollection(propArray);
	    }
        /// <summary>
        /// 可以转换的类型
        /// </summary>
        /// <param name="context">属性类型描述</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>可以转换返回true，否则返回false</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	    {
		    if( destinationType == typeof(string) || destinationType == typeof(InstanceDescriptor) )
			    return true;
		    else
			    return base.CanConvertTo(context, destinationType);
	    }
        /// <summary>
        /// 是否可以进行转换
        /// </summary>
        /// <param name="context">属性类型描述</param>
        /// <param name="sourceType">源类型</param>
        /// <returns>可以转换返回true，否则返回false</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	    {
		    if( sourceType == typeof(string) )
			    return true;
		    else
			    return base.CanConvertFrom(context, sourceType);
	    }
        /// <summary>
        /// 转换到某一类型
        /// </summary>
        /// <param name="context">属性类型描述</param>
        /// <param name="culture">区域信息</param>
        /// <param name="value">需要转换的类对象</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回转换后的类对象</returns>
        public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
	    {
		    if( destinationType == null )
			    throw new ArgumentNullException( "destinationType" );

		    if( culture == null )
			    culture = CultureInfo.CurrentCulture;

		    BoundingBox box = (BoundingBox)( value );

		    if( destinationType == typeof(string) && box != null )
		    {
			    String separator = culture.TextInfo.ListSeparator + " ";
			    TypeConverter converter = TypeDescriptor.GetConverter(typeof(Vector3));
			    string[] stringArray = new string[ 2 ];

			    stringArray[0] = converter.ConvertToString( context, culture, box.Maximum );
			    stringArray[1] = converter.ConvertToString( context, culture, box.Minimum );

			    return String.Join( separator, stringArray );
		    }
		    else if( destinationType == typeof(InstanceDescriptor) && box != null )
		    {
			    ConstructorInfo info = (typeof(BoundingBox)).GetConstructor( new Type[] { typeof(Vector3), typeof(Vector3) } );
			    if( info != null )
				    return new InstanceDescriptor( info, new Object[] { box.Maximum, box.Minimum } );
		    }

		    return base.ConvertTo(context, culture, value, destinationType);
	    }
        /// <summary>
        /// 从源对象转换该类对象
        /// </summary>
        /// <param name="context">属性类型描述</param>
        /// <param name="culture">区域信息</param>
        /// <param name="value">源对象</param>
        /// <returns>返回转换后的对象</returns>
        public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
	    {
		    if( culture == null )
			    culture = CultureInfo.CurrentCulture;

		    String Str = (string)( value );

		    if( Str != null )
		    {
			    Str = Str.Trim();
			    TypeConverter converter = TypeDescriptor.GetConverter(typeof(Vector3));
			    string[] stringArray = Str.Split( culture.TextInfo.ListSeparator[0] );

			    if( stringArray.Length != 2 )
				    throw new ArgumentException("Invalid box format.");

			    Vector3 Maximum = (Vector3)( converter.ConvertFromString( context, culture, stringArray[0] ) );
			    Vector3 Minimum = (Vector3)( converter.ConvertFromString( context, culture, stringArray[1] ) );

			    return new BoundingBox(Maximum, Minimum);
		    }

		    return base.ConvertFrom(context, culture, value);
	    }
        /// <summary>
        /// 获取该实例对象是否支持转换
        /// </summary>
        /// <param name="context">属性类型描述</param>
        /// <returns>支持转换返回true，否则返回false</returns>
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
	    {
		    //SLIMDX_UNREFERENCED_PARAMETER(context);
		    return true;
	    }
        /// <summary>
        /// 创建实例对象
        /// </summary>
        /// <param name="context">属性类型描述</param>
        /// <param name="propertyValues">属性值</param>
        /// <returns>返回创建的实例对象</returns>
        public override Object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
	    {
		    //SLIMDX_UNREFERENCED_PARAMETER(context);

		    if( propertyValues == null )
			    throw new ArgumentNullException( "propertyValues" );

		    return new BoundingBox( (Vector3)( propertyValues["Maximum"] ), (Vector3)( propertyValues["Minimum"] ) );
	    }
        /// <summary>
        /// 获取相应的属性是否支持转换
        /// </summary>
        /// <param name="tdc">属性描述对象</param>
        /// <returns>支持转换返回true，否则返回false</returns>
        public override bool GetPropertiesSupported(ITypeDescriptorContext tdc)
	    {
		    return true;
	    }
        /// <summary>
        /// 获取对象的属性
        /// </summary>
        /// <param name="tdc">属性描述对象</param>
        /// <param name="obj">对象</param>
        /// <param name="attrs">属性列表</param>
        /// <returns>返回相应的属性</returns>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext tdc, Object obj, Attribute[] attrs)
	    {
		    return m_Properties;
	    }
    }
}
