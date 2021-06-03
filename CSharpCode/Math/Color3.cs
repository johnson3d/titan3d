namespace EngineNS
{
    /// <summary>
    /// 颜色结构体
    /// </summary>
    [System.Serializable]
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    //[System.ComponentModel.TypeConverter( typeof(EngineNS.Design.Color3Converter))]
	public struct Color3 : System.IEquatable<Color3>
    {
        /// <summary>
        /// 红色值
        /// </summary>
        public float Red;
        /// <summary>
        /// 绿色值
        /// </summary>
		public float Green;
        /// <summary>
        /// 蓝色值
        /// </summary>
		public float Blue;
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="red">红色值</param>
        /// <param name="green">绿色值</param>
        /// <param name="blue">蓝色值</param>
        public Color3( float red, float green, float blue )
	    {
            Red = red;
            Green = green;
            Blue = blue;
	    }
        /// <summary>
        /// 重载操作符==
        /// </summary>
        /// <param name="left">颜色值</param>
        /// <param name="right">颜色值</param>
        /// <returns>如果两个颜色相同返回true，否则返回false</returns>
	    public static bool operator == ( Color3 left, Color3 right )
	    {
            return left.Equals(right);
		    //return Color3.Equals( left, right );
	    }
        /// <summary>
        /// 重载操作符!=
        /// </summary>
        /// <param name="left">颜色值</param>
        /// <param name="right">颜色值</param>
        /// <returns>如果两个颜色不相同返回true，否则返回false</returns>
        public static bool operator !=(Color3 left, Color3 right)
	    {
            return !left.Equals(right);
		    //return !Equals( left, right );
	    }
        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
	    public override int GetHashCode()
	    {
		    return Red.GetHashCode() + Green.GetHashCode() + Blue.GetHashCode();
	    }
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="value">需要判断的对象</param>
        /// <returns>如果相同返回true，否则返回false</returns>
	    public override bool Equals( object value )
	    {
		    if( value == null )
			    return false;

		    if( value.GetType() != GetType() )
			    return false;

		    return Equals( (Color3)( value ) );
	    }
        /// <summary>
        /// 判断两个颜色是否相同
        /// </summary>
        /// <param name="value">颜色值</param>
        /// <returns>如果相同返回true，否则返回false</returns>
	    public bool Equals( Color3 value )
	    {
		    return ( Red == value.Red && Green == value.Green && Blue == value.Blue );
	    }
        /// <summary>
        /// 判断两个颜色是否相同
        /// </summary>
        /// <param name="value1">颜色值</param>
        /// <param name="value2">颜色值</param>
        /// <returns>如果相同返回true，否则返回false</returns>
        public static bool Equals(ref Color3 value1, ref Color3 value2)
	    {
		    return ( value1.Red == value2.Red && value1.Green == value2.Green && value1.Blue == value2.Blue );
	    }
    }
}
