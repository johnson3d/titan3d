using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CodeGenerateSystem.Base
{
    //public class UsefulMemberHostData
    //{
    //    public enum enHostType
    //    {
    //        Normal,     // 从链接点
    //        Static,     // 静态类
    //        Instance,   // 单件类
    //        This,       // this
    //        Member,     // 成员
    //    }
    //    public enHostType HostType = enHostType.Normal;
    //    // 类型全名称
    //    public string ClassTypeFullName;
    //    // 成员名称
    //    public string MemberHostName;
    //    // 
    //    public BaseNodeControl HostControl;
    //    // 连接点
    //    public LinkPinControl LinkObject;

    //    public override string ToString()
    //    {
    //        var retStr = HostType.ToString() + "," +
    //                     ClassTypeFullName;
    //        if (HostControl != null)
    //            retStr += "," + HostControl.Id.ToString();
    //        else
    //            retStr += ",null";
    //        if (LinkObject != null)
    //            retStr += "," + LinkObject.GUID.ToString();
    //        else
    //            retStr += ",null";
    //        if (string.IsNullOrEmpty(MemberHostName))
    //            retStr += ",";
    //        else
    //            retStr += "," + MemberHostName;

    //        return retStr;
    //    }

    //    public void ParseString(string str, NodesContainer nodesContainer)
    //    {
    //        if (string.IsNullOrEmpty(str) || nodesContainer == null)
    //            return;

    //        var splits = str.Split(',');
    //        if (splits.Length < 4)
    //            return;

    //        HostType = (enHostType)EngineNS.Rtti.RttiHelper.EnumTryParse(typeof(enHostType), splits[0]);
    //        ClassTypeFullName = splits[1];
    //        if (splits[2] != "null")
    //        {
    //            var hostControlId = EngineNS.Rtti.RttiHelper.GuidTryParse(splits[2]);
    //            HostControl = nodesContainer.FindControl(hostControlId);
    //            if (HostControl != null && splits[3] != "null")
    //            {
    //                var linkObjId = EngineNS.Rtti.RttiHelper.GuidTryParse(splits[3]);
    //                LinkObject = HostControl.GetLinkPinInfo(ref linkObjId);
    //            }
    //        }

    //        if (splits.Length >= 5)
    //            MemberHostName = splits[4];
    //    }

    //    public static UsefulMemberHostData Parse(string str, NodesContainer nodesContainer)
    //    {
    //        var retData = new UsefulMemberHostData();
    //        retData.ParseString(str, nodesContainer);
    //        return retData;
    //    }
    //}

    //public interface UsefulMember
    //{
    //    /// <summary>
    //    /// 获取能够使用的节点集合
    //    /// </summary>
    //    List<UsefulMemberHostData> GetUsefulMembers();

    //    /// <summary>
    //    /// 获取指定链接点能够使用的节点集合
    //    /// </summary>
    //    List<UsefulMemberHostData> GetUsefulMembers(LinkControl linkCtrl);

    //}

    //public interface GeneratorClass
    //{
    //    CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
    //    {
    //        get;
    //    }        
    //}
}
