using System;

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace EngineNS.Design
{
 //   public class Vector4Converter : System.ComponentModel.ExpandableObjectConverter
	//{
	//	private System.ComponentModel.PropertyDescriptorCollection m_Properties;

 //       public Vector4Converter()
	//    {
	//	    Type type = typeof(Vector4);
 //           PropertyDescriptor[] propArray = new PropertyDescriptor[]
	//	    {
	//		    new FieldPropertyDescriptor(type.GetField("X")),
	//		    new FieldPropertyDescriptor(type.GetField("Y")),
	//		    new FieldPropertyDescriptor(type.GetField("Z")),
	//		    new FieldPropertyDescriptor(type.GetField("W")),
	//	    };

	//	    m_Properties = new PropertyDescriptorCollection(propArray);
	//    }

 //       public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	//    {
	//	    if( destinationType == typeof(string) || destinationType == typeof(InstanceDescriptor) )
	//		    return true;
	//	    else
	//		    return base.CanConvertTo(context, destinationType);
	//    }

 //       public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	//    {
	//	    if( sourceType == typeof(string) )
	//		    return true;
	//	    else
	//		    return base.CanConvertFrom(context, sourceType);
	//    }

 //       public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
	//    {
	//	    if( destinationType == null )
	//		    throw new ArgumentNullException( "destinationType" );

	//	    if( culture == null )
	//		    culture = CultureInfo.CurrentCulture;

	//	    Vector4 vector = (Vector4)( value );

	//	    if( destinationType == typeof(string) && vector != null )
	//	    {
	//		    String separator = culture.TextInfo.ListSeparator + " ";
	//		    TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
	//		    string[] stringArray = new string[ 4 ];

	//		    stringArray[0] = converter.ConvertToString( context, culture, vector.X );
	//		    stringArray[1] = converter.ConvertToString( context, culture, vector.Y );
	//		    stringArray[2] = converter.ConvertToString( context, culture, vector.Z );
	//		    stringArray[3] = converter.ConvertToString( context, culture, vector.W );

	//		    return String.Join( separator, stringArray );
	//	    }
	//	    else if( destinationType == typeof(InstanceDescriptor) && vector != null )
	//	    {
	//		    ConstructorInfo info = (typeof(Vector4)).GetConstructor( new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) } );
	//		    if( info != null )
	//			    return new InstanceDescriptor( info, new object[] { vector.X, vector.Y, vector.Z, vector.W } );
	//	    }

	//	    return base.ConvertTo(context, culture, value, destinationType);
	//    }

 //       public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
	//    {
	//	    if( culture == null )
	//		    culture = CultureInfo.CurrentCulture;

	//	    String Str = (string)( value );

	//	    if( Str != null )
	//	    {
	//		    Str = Str.Trim();
	//		    TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
	//		    string[] stringArray = Str.Split( culture.TextInfo.ListSeparator[0] );

	//		    if( stringArray.Length != 4 )
	//			    throw new ArgumentException("Invalid vector format.");

	//		    float X = (float)( converter.ConvertFromString( context, culture, stringArray[0] ) );
	//		    float Y = (float)( converter.ConvertFromString( context, culture, stringArray[1] ) );
	//		    float Z = (float)( converter.ConvertFromString( context, culture, stringArray[2] ) );
	//		    float W = (float)( converter.ConvertFromString( context, culture, stringArray[3] ) );

	//		    return new Vector4(X, Y, Z, W);
	//	    }

	//	    return base.ConvertFrom(context, culture, value);
	//    }

 //       public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
	//    {
	//	    //SLIMDX_UNREFERENCED_PARAMETER(context);

	//	    return true;
	//    }

 //       public override Object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
	//    {
	//	    //SLIMDX_UNREFERENCED_PARAMETER(context);

	//	    if( propertyValues == null )
	//		    throw new ArgumentNullException( "propertyValues" );

	//	    return new Vector4( (float)( propertyValues["X"] ), (float)( propertyValues["Y"] ),
	//		    (float)( propertyValues["Z"] ), (float)( propertyValues["W"] ) );
	//    }

 //       public override bool GetPropertiesSupported(ITypeDescriptorContext tdc)
	//    {
	//	    return true;
	//    }

 //       public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext tdc, Object obj, Attribute[] attrs)
	//    {
	//	    return m_Properties;
	//    }
 //   }
}
