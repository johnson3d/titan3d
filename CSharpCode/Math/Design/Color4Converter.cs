using System;

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace EngineNS.Design
{
 //   public class Color4Converter : System.ComponentModel.ExpandableObjectConverter
	//{
	//	private System.ComponentModel.PropertyDescriptorCollection m_Properties;
 //       public Color4Converter()
	//    {
	//	    Type type = typeof(Color4);
 //           PropertyDescriptor[] propArray = new PropertyDescriptor[]
	//	    {
	//		    new FieldPropertyDescriptor(type.GetField("Red")),
	//		    new FieldPropertyDescriptor(type.GetField("Green")),
	//		    new FieldPropertyDescriptor(type.GetField("Blue")),
	//		    new FieldPropertyDescriptor(type.GetField("Alpha")),
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

	//	    Color4 color = (Color4)( value );

	//	    if( destinationType == typeof(string) && color != null )
	//	    {
	//		    String separator = culture.TextInfo.ListSeparator + " ";
	//		    TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
	//		    String[] stringArray = new String[ 4 ];

	//		    stringArray[0] = converter.ConvertToString( context, culture, color.Alpha );
	//		    stringArray[1] = converter.ConvertToString( context, culture, color.Red );
	//		    stringArray[2] = converter.ConvertToString( context, culture, color.Green );
	//		    stringArray[3] = converter.ConvertToString( context, culture, color.Blue );

	//		    return String.Join( separator, stringArray );
	//	    }
	//	    else if( destinationType == typeof(InstanceDescriptor) && color != null )
	//	    {
	//		    ConstructorInfo info = (typeof(Color4)).GetConstructor( new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) } );
	//		    if( info != null )
	//			    return new InstanceDescriptor( info, new Object[] { color.Alpha, color.Red, color.Green, color.Blue } );
	//	    }

	//	    return base.ConvertTo(context, culture, value, destinationType);
	//    }

 //       public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
	//    {
	//	    if( culture == null )
	//		    culture = CultureInfo.CurrentCulture;

 //           var valueType = value.GetType();
 //           if(valueType == typeof(string))
 //           {
 //               String Str = (string)(value);

 //               if (Str != null)
 //               {
 //                   Str = Str.Trim();
 //                   TypeConverter floatConverter = TypeDescriptor.GetConverter(typeof(float));
 //                   string[] stringArray = Str.Split(culture.TextInfo.ListSeparator[0]);

 //                   if (stringArray.Length == 1)
 //                   {
 //                       uint number = 0;
 //                       if (uint.TryParse(Str, out number))
 //                           return new Color4(number);

 //                       TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
 //                       return new Color4((Color)(colorConverter.ConvertFromString(context, culture, Str)));
 //                   }
 //                   else if (stringArray.Length == 3)
 //                   {
 //                       int red;
 //                       int green;
 //                       int blue;
 //                       if (int.TryParse(stringArray[0], out red) && int.TryParse(stringArray[1], out green) && int.TryParse(stringArray[2], out blue))
 //                           return new Color4(Color.FromArgb(red, green, blue));

 //                       float r = (float)(floatConverter.ConvertFromString(context, culture, stringArray[0]));
 //                       float g = (float)(floatConverter.ConvertFromString(context, culture, stringArray[1]));
 //                       float b = (float)(floatConverter.ConvertFromString(context, culture, stringArray[2]));

 //                       return new Color4(r, g, b);
 //                   }
 //                   else if (stringArray.Length == 4)
 //                   {
 //                       int red;
 //                       int green;
 //                       int blue;
 //                       int alpha;
 //                       if (int.TryParse(stringArray[0], out alpha) && int.TryParse(stringArray[1], out red) && int.TryParse(stringArray[2], out green) && int.TryParse(stringArray[3], out blue))
 //                           return new Color4(Color.FromArgb(alpha, red, green, blue));

 //                       float a = (float)(floatConverter.ConvertFromString(context, culture, stringArray[0]));
 //                       float r = (float)(floatConverter.ConvertFromString(context, culture, stringArray[1]));
 //                       float g = (float)(floatConverter.ConvertFromString(context, culture, stringArray[2]));
 //                       float b = (float)(floatConverter.ConvertFromString(context, culture, stringArray[3]));

 //                       return new Color4(a, r, g, b);
 //                   }
 //                   else
 //                       throw new ArgumentException("Invalid color format.");
 //               }
 //           }
 //           else if(valueType == typeof(EngineNS.Color))
 //           {
 //               var cl = (EngineNS.Color)value;
 //               return new Color4(cl.A, cl.R, cl.G, cl.B);
 //           }

 //           return base.ConvertFrom(context, culture, value);
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

	//	    return new Color4( (float)( propertyValues["Alpha"] ), (float)( propertyValues["Red"] ),
	//		    (float)( propertyValues["Green"] ), (float)( propertyValues["Blue"] ) );
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
