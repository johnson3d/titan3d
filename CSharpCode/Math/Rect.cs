namespace EngineNS
{
    /// <summary>
    /// 矩形结构体
    /// </summary>
    public struct Rect
    {
        /// <summary>
        /// 重载"=="号运算符
        /// </summary>
        /// <param name="rect1">矩形对象</param>
        /// <param name="rect2">矩形对象</param>
        /// <returns>如果两个矩形相同返回true，否则返回false</returns>
        public static bool operator ==(Rect rect1, Rect rect2)
        {
            return (rect1._x == rect2._x &&
                    rect1._y == rect2._y &&
                    rect1._width == rect2._width &&
                    rect1._height == rect2._height);
        }
        /// <summary>
        /// 重载"!="号运算符
        /// </summary>
        /// <param name="rect1">矩形对象</param>
        /// <param name="rect2">矩形对象</param>
        /// <returns>如果两个矩形不相同返回true，否则返回false</returns>
        public static bool operator !=(Rect rect1, Rect rect2)
        {
            return !(rect1 == rect2);
        }
        /// <summary>
        /// 判断两个矩形对象是否相同
        /// </summary>
        /// <param name="rect1">矩形对象</param>
        /// <param name="rect2">矩形对象</param>
        /// <returns>如果两个矩形相同返回true，否则返回false</returns>
        public static bool Equals(Rect rect1, Rect rect2)
        {
            return (rect1 == rect2);
        }
        /// <summary>
        /// 判断两个矩形对象是否相同
        /// </summary>
        /// <param name="o">可转换为矩形的对象</param>
        /// <returns>如果两个矩形相同返回true，否则返回false</returns>
        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            if (o.GetType() != GetType())
                return false;

            return Equals((Rect)o);
        }
        /// <summary>
        /// 判断两个矩形对象是否相同
        /// </summary>
        /// <param name="value">矩形对象</param>
        /// <returns>如果两个矩形相同返回true，否则返回false</returns>
        public bool Equals(Rect value)
        {
            return (this._x == value._x &&
                    this._y == value._y &&
                    this._width == value._width &&
                    this._height == value._height);
        }
        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        /// <returns>返回对象的哈希值</returns>
        public override int GetHashCode()
        {
            return _x.GetHashCode() + _y.GetHashCode() + _width.GetHashCode() + _height.GetHashCode();
        }
        /// <summary>
        /// 解析资源
        /// </summary>
        /// <param name="source">资源路径</param>
        /// <returns>返回解析出来的矩形资源</returns>
        public static Rect Parse(string source)
        {
            var splits = source.Split(',');
            if (splits.Length != 4)
                return DefaultRect;

            Rect result;
            result._x = System.Convert.ToSingle(splits[0]);
            result._y = System.Convert.ToSingle(splits[1]);
            result._width = System.Convert.ToSingle(splits[2]);
            result._height = System.Convert.ToSingle(splits[3]);
            return result;
        }
        public static Rect DefaultRect = new Rect();
        /// <summary>
        /// 转换成string字符串
        /// </summary>
        /// <returns>返回转换后的string字符串</returns>
        public override string ToString()
        {
            return (_x + "," + _y + "," + _width + "," + _height);
        }
        //public string ToString(IFormatProvider provider);
        //string IFormattable.ToString(string format, IFormatProvider provider);
        //internal string ConvertToString(string format, IFormatProvider provider);
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="location">对象位置坐标</param>
        /// <param name="size">对象大小</param>
        public Rect(EngineNS.Vector2 location, Size size)
        {
            _x = location.X;
            _y = location.Y;
            _width = size.Width;
            _height = size.Height;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="location">对象位置坐标</param>
        /// <param name="size">对象大小</param>
        public Rect(EngineNS.Vector2 location, SizeF size)
        {
            _x = location.X;
            _y = location.Y;
            _width = size.Width;
            _height = size.Height;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public Rect(float x, float y, float width, float height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public Rect(double x, double y, double width, double height)
        {
            _x = (float)x;
            _y = (float)y;
            _width = (float)width;
            _height = (float)height;
        }
        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="size">对象大小</param>
        public Rect(Size size)
        {
            _x = 0;
            _y = 0;
            _width = size.Width;
            _height = size.Height;
        }
        /// <summary>
        /// 对象置空
        /// </summary>
        [Rtti.Meta]
        public static Rect Empty
        {
            get { return DefaultRect; }
        }
        /// <summary>
        /// 只读属性，该对象是否为空
        /// </summary>
        [Rtti.Meta]
        public bool IsEmpty
        {
            get
            {
                if(_x != 0 || _y != 0 || _width > 0 || _height > 0)
                    return false;

                return true;
            }
        }
        /// <summary>
        /// 对象的位置坐标
        /// </summary>
        [Rtti.Meta]
        public Vector2 Location
        {
            get
            {
                EngineNS.Vector2 result;
                result.X = _x;
                result.Y = _y;
                return result;
            }
            set
            {
                _x = value.X;
                _y = value.Y;
            }
        }
        /// <summary>
        /// 对象的大小
        /// </summary>
        [Rtti.Meta]
        public SizeF Size
        {
            get
            {
                SizeF result = new SizeF();
                result.Width = _width;
                result.Height = _height;
                return result;
            }
            set
            {
                _width = value.Width;
                _height = value.Height;
            }
        }

        internal float _x;
        /// <summary>
        /// X坐标
        /// </summary>
        [Rtti.Meta]
        public float X
        {
            get { return _x; }
            set { _x = value; }
        }
        internal float _y;
        /// <summary>
        /// Y坐标
        /// </summary>
        [Rtti.Meta]
        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }
        internal float _width;
        /// <summary>
        /// 宽
        /// </summary>
        [Rtti.Meta]
        public float Width
        {
            get { return _width; }
            set { _width = value; }
        }
        internal float _height;
        /// <summary>
        /// 高
        /// </summary>
        [Rtti.Meta]
        public float Height
        {
            get { return _height; }
            set { _height = value; }
        }
        /// <summary>
        /// 只读属性，最左边的值
        /// </summary>
        [Rtti.Meta]
        public float Left
        {
            get { return _x; }
        }
        /// <summary>
        /// 只读属性，最高点的值
        /// </summary>
        [Rtti.Meta]
        public float Top
        {
            get { return _y; }
        }
        /// <summary>
        /// 只读属性，右侧的值
        /// </summary>
        [Rtti.Meta]
        public float Right
        {
            get { return _x + _width; }
        }
        /// <summary>
        /// 只读属性，最下方的值
        /// </summary>
        [Rtti.Meta]
        public float Bottom
        {
            get { return _y + _height; }
        }
        /// <summary>
        /// 只读属性，左上方坐标
        /// </summary>
        [Rtti.Meta]
        public Vector2 TopLeft
        {
            get
            {
                Vector2 result;
                result.X = Left;
                result.Y = Top;
                return result;
            }
        }
        /// <summary>
        /// 只读属性，右上方坐标
        /// </summary>
        [Rtti.Meta]
        public Vector2 TopRight
        {
            get
            {
                Vector2 result;
                result.X = Right;
                result.Y = Top;
                return result;
            }
        }
        /// <summary>
        /// 只读属性，左下方坐标
        /// </summary>
        [Rtti.Meta]
        public Vector2 BottomLeft
        {
            get
            {
                Vector2 result;
                result.X = Left;
                result.Y = Bottom;
                return result;
            }
        }
        /// <summary>
        /// 只读属性，右下方坐标
        /// </summary>
        [Rtti.Meta]
        public Vector2 BottomRight
        {
            get
            {
                Vector2 result;
                result.X = Right;
                result.Y = Bottom;
                return result;
            }
        }
        /// <summary>
        /// 是否包含某点
        /// </summary>
        /// <param name="point">点对象</param>
        /// <returns>如果包含该点返回true，否则返回false</returns>
        [Rtti.Meta]
        public bool Contains(Vector2 point)
        {
            if (point.X >= Left && point.X <= Right &&
               point.Y >= Top && point.Y <= Bottom)
                return true;

            return false;
        }
        /// <summary>
        /// 是否包含某点
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>如果包含该点返回true，否则返回false</returns>
        [Rtti.Meta]
        public bool Contains(float x, float y)
        {
            if (x >= Left && x <= Right &&
               y >= Top && y <= Bottom)
                return true;

            return false;
        }
        /// <summary>
        /// 与矩形的碰撞
        /// </summary>
        /// <param name="rect">矩形对象</param>
        /// <returns>如果碰撞返回true，否则返回false</returns>
        [Rtti.Meta]
        public bool Contains(Rect rect)
        {
            if (rect.Left >= Left && rect.Right <= Right &&
               rect.Top >= Top && rect.Bottom <= Bottom)
                return true;

            return false;
        }
        /// <summary>
        /// 该对象是否相应的矩形对象相交
        /// </summary>
        /// <param name="rect">矩形对象</param>
        /// <returns>如果相交返回true，否则返回false</returns>
        [Rtti.Meta]
        public bool IntersectsWith(Rect rect)
        {
            if (rect.Left > Right || rect.Right < Left ||
               rect.Top > Bottom || rect.Bottom < Top)
                return false;

            return true;
        }
        /// <summary>
        /// 相交矩形的交点
        /// </summary>
        /// <param name="rect">矩形对象</param>
        [Rtti.Meta]
        public void Intersect(Rect rect)
        {
            var tempRect = Intersect(this, rect);
            _x = tempRect.Left;
            _y = tempRect.Top;
            _width = tempRect.Width;
            _height = tempRect.Height;
        }
        /// <summary>
        /// 相交矩形的交点
        /// </summary>
        /// <param name="rect1">矩形对象</param>
        /// <param name="rect2">矩形对象</param>
        /// <returns>返回相交矩形的交点</returns>
        [Rtti.Meta]
        public static Rect Intersect(Rect rect1, Rect rect2)
        {
            if (!rect1.IntersectsWith(rect2))
                return Rect.Empty;

            Rect result;

            result._x = System.Math.Max(rect1.Left, rect2.Left);
            result._y = System.Math.Max(rect1.Top, rect2.Top);
            result._width = System.Math.Min(rect1.Right, rect2.Right) - result._x;
            result._height = System.Math.Min(rect1.Bottom, rect2.Bottom) - result._y;

            return result;
        }
        //public void Union(Rect rect);
        //public static Rect Union(Rect rect1, Rect rect2);
        //public void Union(Point point);
        //public static Rect Union(Rect rect, Point point);
        //public void Offset(Vector offsetVector);
        //public void Offset(float offsetX, float offsetY);
        //public static Rect Offset(Rect rect, Vector offsetVector);
        //public static Rect Offset(Rect rect, float offsetX, float offsetY);
        //public void Inflate(Size size);
        //public void Inflate(float width, float height);
        //public static Rect Inflate(Rect rect, Size size);
        //public static Rect Inflate(Rect rect, float width, float height);
        //public static Rect Transform(Rect rect, Matrix matrix);
        //public void Transform(Matrix matrix);
        //public void Scale(float scaleX, float scaleY);
        //static Rect();

    }
}
