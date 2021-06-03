using System;

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace EngineNS.Design
{
 //   public class RayConverter : System.ComponentModel.ExpandableObjectConverter
	//{
	//	private System.ComponentModel.PropertyDescriptorCollection m_Properties;

 //       public RayConverter()
	//    {
	//	    Type type = typeof(Ray);
 //           PropertyDescriptor[] propArray = new PropertyDescriptor[]
	//	    {
	//		    new FieldPropertyDescriptor(type.GetField("Position")),
	//		    new FieldPropertyDescriptor(type.GetField("Direction")),
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

	//	    Ray ray = (Ray)( value );

	//	    if( destinationType == typeof(string) && ray != null )
	//	    {
	//		    String separator = culture.TextInfo.ListSeparator + " ";
	//		    TypeConverter converter = TypeDescriptor.GetConverter(typeof(Vector3));
	//		    string[] stringArray = new string[2];

	//		    stringArray[0] = converter.ConvertToString( context, culture, ray.Position );
	//		    stringArray[1] = converter.ConvertToString( context, culture, ray.Direction );

	//		    return String.Join( separator, stringArray );
	//	    }
	//	    else if( destinationType == typeof(InstanceDescriptor) && ray != null )
	//	    {
	//		    ConstructorInfo info = (typeof(Ray)).GetConstructor( new Type[] { typeof(Vector3), typeof(Vector3) } );
	//		    if( info != null )
	//			    return new InstanceDescriptor( info, new Object[] { ray.Position, ray.Direction } );
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
	//		    TypeConverter converter = TypeDescriptor.GetConverter(typeof(Vector3));
	//		    string[] stringArray = Str.Split( culture.TextInfo.ListSeparator[0] );

	//		    if( stringArray.Length != 2 )
	//			    throw new ArgumentException("Invalid ray format.");

	//		    Vector3 Position = (Vector3)( converter.ConvertFromString( context, culture, stringArray[0] ) );
	//		    Vector3 Direction = (Vector3)( converter.ConvertFromString( context, culture, stringArray[1] ) );

	//		    return new Ray(Position, Direction);
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

	//	    return new Ray( (Vector3)( propertyValues["Position"] ), (Vector3)( propertyValues["Direction"] ) );
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
