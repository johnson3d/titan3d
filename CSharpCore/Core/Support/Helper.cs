using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Support
{
    public class Helper
    {
        static public Guid GuidParse(string str)
        {
            if (!str.Contains("-"))
                return Guid.Empty;

            return new Guid(str);
        }
        static public Guid GuidTryParse(string str)
        {
            try
            {
                if (!str.Contains("-"))
                    return Guid.Empty;

                return new Guid(str);
            }
            catch (System.Exception)
            {
                //Log.FileLog.WriteLine(string.Format("Parse Guid Failed:{0}", str));
                return Guid.Empty;
            }
        }
        static public T EnumTryParse<T>(string str)
        {
            try
            {
                return (T)(System.Enum.Parse(typeof(T), str));
            }
            catch (System.Exception)
            {
                return default(T);
            }
        }

        static public object EnumTryParse(System.Type type, string str)
        {
            try
            {
                return System.Enum.Parse(type, str);
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }
}
