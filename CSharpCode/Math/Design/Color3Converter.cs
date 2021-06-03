using System;

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace EngineNS.Design
{
    //public class Color3Converter : System.ComponentModel.ExpandableObjectConverter
    //{
    //    private System.ComponentModel.PropertyDescriptorCollection m_Properties;
    //    public Color3Converter()
	   // {
		  //  Type type = typeof(Color3);
    //        PropertyDescriptor[] propArray = new PropertyDescriptor[]
		  //  {
			 //   new FieldPropertyDescriptor(type.GetField("Red")),
			 //   new FieldPropertyDescriptor(type.GetField("Green")),
			 //   new FieldPropertyDescriptor(type.GetField("Blue")),
		  //  };

		  //  m_Properties = new PropertyDescriptorCollection(propArray);
	   // }

	   // public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	   // {
		  //  if( destinationType == typeof(string) || destinationType == typeof(InstanceDescriptor) )
			 //   return true;
		  //  else
			 //   return base.CanConvertTo(context, destinationType);
	   // }

    //    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	   // {
		  //  if( sourceType == typeof(string) )
			 //   return true;
		  //  else
    //            return base.CanConvertFrom(context, sourceType);
	   // }

    //    public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
	   // {
		  //  if( destinationType == null )
			 //   throw new ArgumentNullException( "destinationType" );

		  //  if( culture == null )
			 //   culture = CultureInfo.CurrentCulture;

		  //  Color3 color = (Color3)( value );

		  //  if( destinationType == typeof(string) && color != null )
		  //  {
			 //   string separator = culture.TextInfo.ListSeparator + " ";
			 //   TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
			 //   string[] stringArray = new string[ 3 ];

			 //   stringArray[0] = converter.ConvertToString( context, culture, color.Red );
			 //   stringArray[1] = converter.ConvertToString( context, culture, color.Green );
			 //   stringArray[2] = converter.ConvertToString( context, culture, color.Blue );

			 //   return string.Join( separator, stringArray );
		  //  }
		  //  else if( destinationType == typeof(InstanceDescriptor) && color != null )
		  //  {
			 //   ConstructorInfo info = (typeof(Color3)).GetConstructor( new Type[] { typeof(float), typeof(float), typeof(float) } );
			 //   if( info != null )
				//    return new InstanceDescriptor( info, new Object[] { color.Red, color.Green, color.Blue } );
		  //  }

    //        return base.ConvertTo(context, culture, value, destinationType);
	   // }

	   // public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
	   // {
		  //  if( culture == null )
			 //   culture = CultureInfo.CurrentCulture;

		  //  string Str = (string)( value );

		  //  if( Str != null )
		  //  {
			 //   Str = Str.Trim();
			 //   TypeConverter floatConverter = TypeDescriptor.GetConverter(typeof(float));
			 //   string[] stringArray = Str.Split( culture.TextInfo.ListSeparator[0] );

			 //   if( stringArray.Length == 1 )
			 //   {
				//    uint number = 0;
				//    if( uint.TryParse( Str,out number ) )
				//    {
				//	    Color4 color = new Color4( number );
				//	    return new Color3( color.Red, color.Green, color.Blue );
				//    }

				//    TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
    //                object colorFromStr = colorConverter.ConvertFromString( context, culture, Str );
    //                Color4 valueColor = new Color4((Color)(colorFromStr));
    //                return new Color3(valueColor.Red, valueColor.Green, valueColor.Blue);
			 //   }
			 //   else if( stringArray.Length == 3 )
			 //   {
				//    int red;
				//    int green;
				//    int blue;
				//    if( int.TryParse( stringArray[0], out red ) && int.TryParse( stringArray[1], out green ) && int.TryParse( stringArray[2], out blue ) )
				//    {
    //                    Color4 color = new Color4(Color.FromArgb(red, green, blue));
				//	    return new Color3( color.Red, color.Green, color.Blue );
				//    }

				//    float r = (float)( floatConverter.ConvertFromString( context, culture, stringArray[0] ) );
				//    float g = (float)( floatConverter.ConvertFromString( context, culture, stringArray[1] ) );
				//    float b = (float)( floatConverter.ConvertFromString( context, culture, stringArray[2] ) );

				//    return new Color3( r, g, b );
			 //   }
			 //   else
				//    throw new ArgumentException("Invalid color format.");
		  //  }

		  //  return base.ConvertFrom(context, culture, value);
	   // }

    //    public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
	   // {
		  //  //SLIMDX_UNREFERENCED_PARAMETER(context);

		  //  return true;
	   // }

    //    public override Object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
	   // {
		  //  //SLIMDX_UNREFERENCED_PARAMETER(context);

		  //  if( propertyValues == null )
			 //   throw new ArgumentNullException( "propertyValues" );

		  //  return new Color3( (float)( propertyValues["Red"] ),
			 //   (float)( propertyValues["Green"] ), (float)( propertyValues["Blue"] ) );
	   // }

    //    public override bool GetPropertiesSupported(ITypeDescriptorContext tdc)
	   // {
		  //  return true;
	   // }

    //    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext tdc, object obj, Attribute[] atts)
	   // {
		  //  return m_Properties;
	   // }
    //}
}
