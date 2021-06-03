using System;

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace EngineNS.Design
{
 //   public class PlaneConverter : System.ComponentModel.ExpandableObjectConverter
	//{
	//	private System.ComponentModel.PropertyDescriptorCollection m_Properties;
 //       public PlaneConverter()
	//    {
	//	    Type type = typeof(Plane);
 //           PropertyDescriptor[] propArray = new PropertyDescriptor[]
	//	    {
	//		    new FieldPropertyDescriptor(type.GetField("Normal")),
	//		    new FieldPropertyDescriptor(type.GetField("D")),
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

	//	    Plane plane = (Plane)( value );

	//	    if( destinationType == typeof(string) && plane != null )
	//	    {
	//		    String separator = culture.TextInfo.ListSeparator + " ";
	//		    TypeConverter vector3Converter = TypeDescriptor.GetConverter(typeof(Vector3));
	//		    TypeConverter floatConverter = TypeDescriptor.GetConverter(typeof(float));
	//		    String[] stringArray = new String[2];

	//		    stringArray[0] = vector3Converter.ConvertToString( context, culture, plane.Normal );
	//		    stringArray[1] = floatConverter.ConvertToString( context, culture, plane.D );

	//		    return String.Join( separator, stringArray );
	//	    }
	//	    else if( destinationType == typeof(InstanceDescriptor) && plane != null )
	//	    {
	//		    ConstructorInfo info = (typeof(Plane)).GetConstructor( new Type[] { typeof(Vector3), typeof(float) } );
	//		    if( info != null )
	//			    return new InstanceDescriptor( info, new Object[] { plane.Normal, plane.D } );
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
	//		    TypeConverter vector3Converter = TypeDescriptor.GetConverter(typeof(Vector3));
	//		    TypeConverter floatConverter = TypeDescriptor.GetConverter(typeof(float));
	//		    String[] stringArray = Str.Split( culture.TextInfo.ListSeparator[0] );

	//		    if( stringArray.Length != 2 )
	//			    throw new ArgumentException("Invalid plane format.");

	//		    Vector3 Normal = (Vector3)( vector3Converter.ConvertFromString( context, culture, stringArray[0] ) );
	//		    float D = (float)( floatConverter.ConvertFromString( context, culture, stringArray[1] ) );

	//		    return new Plane(Normal, D);
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

	//	    return new Plane( (Vector3)( propertyValues["Normal"] ), (float)( propertyValues["D"] ) );
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
