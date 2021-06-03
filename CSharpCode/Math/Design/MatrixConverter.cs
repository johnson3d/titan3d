using System;

using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace EngineNS.Design
{
 //   public class MatrixConverter : System.ComponentModel.ExpandableObjectConverter
	//{
	//	private System.ComponentModel.PropertyDescriptorCollection m_Properties;
 //       public MatrixConverter()
	//    {
	//	    Type type = typeof(Matrix);
 //           PropertyDescriptor[] propArray = new PropertyDescriptor[]
	//	    {
	//		    new FieldPropertyDescriptor(type.GetField("M11")),
	//		    new FieldPropertyDescriptor(type.GetField("M12")),
	//		    new FieldPropertyDescriptor(type.GetField("M13")),
	//		    new FieldPropertyDescriptor(type.GetField("M14")),

	//		    new FieldPropertyDescriptor(type.GetField("M21")),
	//		    new FieldPropertyDescriptor(type.GetField("M22")),
	//		    new FieldPropertyDescriptor(type.GetField("M23")),
	//		    new FieldPropertyDescriptor(type.GetField("M24")),

	//		    new FieldPropertyDescriptor(type.GetField("M31")),
	//		    new FieldPropertyDescriptor(type.GetField("M32")),
	//		    new FieldPropertyDescriptor(type.GetField("M33")),
	//		    new FieldPropertyDescriptor(type.GetField("M34")),

	//		    new FieldPropertyDescriptor(type.GetField("M41")),
	//		    new FieldPropertyDescriptor(type.GetField("M42")),
	//		    new FieldPropertyDescriptor(type.GetField("M43")),
	//		    new FieldPropertyDescriptor(type.GetField("M44")),
	//	    };

	//	    m_Properties = new PropertyDescriptorCollection(propArray);
	//    }

 //       public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	//    {
	//	    if( destinationType == typeof(string) )
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

	//	    Matrix matrix = (Matrix)( value );

	//	    if( destinationType == typeof(string) && matrix != null )
	//	    {
	//		    String separator = culture.TextInfo.ListSeparator + " ";
	//		    TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
	//		    string[] stringArray = new string[16];

	//		    for( int i = 0; i < 4; i++ )
	//		    {
	//			    for( int j = 0; j < 4; j++ )
	//				    stringArray[i * 4 + j] = converter.ConvertToString( context, culture, matrix[i, j] );
	//		    }

	//		    return String.Join( separator, stringArray );
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

	//		    if( stringArray.Length != 16 )
	//			    throw new ArgumentException("Invalid matrix format.");

	//		    Matrix matrix = new Matrix();

	//		    for( int i = 0; i < 4; i++ )
	//		    {
	//			    for( int j = 0; j < 4; j++ )
	//				    matrix[i, j] = (float)( converter.ConvertFromString( context, culture, stringArray[i * 4 + j] ) );
	//		    }

	//		    return matrix;
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

	//	    Matrix matrix;

	//	    matrix.M11 = (float)( propertyValues["M11"] );
	//	    matrix.M12 = (float)( propertyValues["M12"] );
	//	    matrix.M13 = (float)( propertyValues["M13"] );
	//	    matrix.M14 = (float)( propertyValues["M14"] );

	//	    matrix.M21 = (float)( propertyValues["M21"] );
	//	    matrix.M22 = (float)( propertyValues["M22"] );
	//	    matrix.M23 = (float)( propertyValues["M23"] );
	//	    matrix.M24 = (float)( propertyValues["M24"] );

	//	    matrix.M31 = (float)( propertyValues["M31"] );
	//	    matrix.M32 = (float)( propertyValues["M32"] );
	//	    matrix.M33 = (float)( propertyValues["M33"] );
	//	    matrix.M34 = (float)( propertyValues["M34"] );

	//	    matrix.M41 = (float)( propertyValues["M41"] );
	//	    matrix.M42 = (float)( propertyValues["M42"] );
	//	    matrix.M43 = (float)( propertyValues["M43"] );
	//	    matrix.M44 = (float)( propertyValues["M44"] );

	//	    return matrix;
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
