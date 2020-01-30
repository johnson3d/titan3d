using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    // 通用函数
    public class CommonMethod
    {
        static System.Random mRandom = new System.Random();
        //[CSUtility.AISystem.Attribute.AllowMethod(EngineNS.ECSType.Common)]
        //[CSUtility.Event.Attribute.AllowMember("函数.随机", EngineNS.ECSType.Common, "获取最小值到最大值之间的随机值")]
        public static int Random(int min, int max)
        {
            return mRandom.Next(min, max);
        }


        //[CSUtility.AISystem.Attribute.AllowMethod(EngineNS.ECSType.Common)]
        //[CSUtility.Event.Attribute.AllowMember("函数.新建列表", EngineNS.ECSType.Common, "创建一个")]
        //public List<object> NewObjectList()
        //{
        //    return new List<object>();
        //}

        //[CSUtility.AISystem.Attribute.AllowMethod(EngineNS.ECSType.Common)]
        //[CSUtility.Event.Attribute.AllowMember(EngineNS.ECSType.Common)]
        //public object GetListElement(List<object> lst, int index)
        //{
        //    if (lst == null)
        //        return null;
        //    if (index >= lst.Count)
        //        return null;
        //    return lst[index];
        //}

        //[CSUtility.AISystem.Attribute.AllowMethod(EngineNS.ECSType.Common)]
        //[CSUtility.Event.Attribute.AllowMethod(EngineNS.ECSType.Common)]
        //public void AddListElement(List<object> lst, object obj)
        //{
        //    if (lst == null)
        //        return;
        //    lst.Add(obj);
        //}

        //[CSUtility.AISystem.Attribute.AllowMethod(EngineNS.ECSType.Common)]
        //[CSUtility.Event.Attribute.AllowMethod(EngineNS.ECSType.Common)]
        //public void RemoveListElement(List<object> lst, object obj)
        //{
        //    if (lst == null)
        //        return;
        //    lst.Remove(obj);
        //}

        //[CSUtility.AISystem.Attribute.AllowMethod(EngineNS.ECSType.Common)]
        //[CSUtility.Event.Attribute.AllowMethod(EngineNS.ECSType.Common)]
        //public Guid ParseGuidFromString(string str)
        //{
        //    if (string.IsNullOrEmpty(str))
        //        return Guid.Empty;
        //    return CSUtility.Support.IHelper.GuidTryParse(str);
        //}
    }
}
