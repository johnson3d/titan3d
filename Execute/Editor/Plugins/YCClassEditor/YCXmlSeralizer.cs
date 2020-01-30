using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace YCClassEditor
{
    class YCXmlSeralizer
    {
        string path;
        public YCXmlSeralizer()
        {
            path = ClassEditor.CustomDirectory+"\\CustomXmls\\";
        }
        public void SerializeClass(YCPropertyCollection pc, string classname)
        {
            //创建XmlDocument对象
            XmlDocument xmlDoc = new XmlDocument();
            //XML的声明<?xml version="1.0" encoding="gb2312"?> 
            XmlDeclaration xmlSM = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            //追加xmldecl位置
            xmlDoc.AppendChild(xmlSM);
            //添加一个名为Gen的根节点
            XmlElement xml = xmlDoc.CreateElement("", "Class", "");
            xml.SetAttribute("classname", classname);
            //追加class的根节点位置
            xmlDoc.AppendChild(xml);
            foreach (var p in pc.LstData)
            {
                //循环添加Property节点,与class所匹配，查找<class>
                XmlNode classNode = xmlDoc.SelectSingleNode("Class");
                //添加一个名为<Zi>的节点   
                XmlElement propertyNode = xmlDoc.CreateElement("Property");
                //为<Zi>节点的属性
                propertyNode.SetAttribute("name", p.name);
                propertyNode.SetAttribute("type", p.type);
                XmlElement defaultvalue = xmlDoc.CreateElement("DefaultValue");
                //InnerText:获取或设置节点及其所有子节点的串连值
                defaultvalue.InnerText = p.defaultvalue;
                propertyNode.AppendChild(defaultvalue);
                XmlElement blist = xmlDoc.CreateElement("bList");
                //InnerText:获取或设置节点及其所有子节点的串连值
                blist.InnerText = p.bList ? "1" : "0";
                propertyNode.AppendChild(blist);
                XmlElement brange = xmlDoc.CreateElement("bRange");
                brange.InnerText = p.bRange ? "1" : "0";
                propertyNode.AppendChild(brange);
                XmlElement min = xmlDoc.CreateElement("Min");
                min.InnerText = p.min;
                propertyNode.AppendChild(min);
                XmlElement max = xmlDoc.CreateElement("Max");
                max.InnerText = p.max;
                propertyNode.AppendChild(max);

                XmlElement bstep = xmlDoc.CreateElement("bStep");
                //InnerText:获取或设置节点及其所有子节点的串连值
                bstep.InnerText = p.bStep ? "1" : "0";
                propertyNode.AppendChild(bstep);
                XmlElement step = xmlDoc.CreateElement("Step");
                step.InnerText = p.step;
                propertyNode.AppendChild(step);

                XmlElement etype = xmlDoc.CreateElement("EnumType");
                etype.InnerText = p.enumtype;
                propertyNode.AppendChild(etype);

                XmlElement battribute = xmlDoc.CreateElement("bAttribute");
                battribute.InnerText = p.hasAttribute ? "1" : "0";
                propertyNode.AppendChild(battribute);

                XmlElement attributeName = xmlDoc.CreateElement("AttributeName");
                attributeName.InnerText = p.attribute;
                propertyNode.AppendChild(attributeName);

                XmlElement attributeInput = xmlDoc.CreateElement("AttributeInput");
                attributeInput.InnerText = p.attributeInput;
                propertyNode.AppendChild(attributeInput);

                classNode.AppendChild(propertyNode);//添加到<class>节点中   
            }
            xmlDoc.Save(path + "Class_" + classname + ".xml"); //保存好创建的XML文档}
        }
        public void DeSerializeClass(YCPropertyCollection pc, string classname)
        {
            
            //清理pc
            pc.LstData.Clear();
            //对每个classname，读取对应的文件
            string xmlfilepath = path + "Class_" + classname + ".xml";
            //将XML文件加载进来
            if (!File.Exists(xmlfilepath))
                return;
            XDocument document = XDocument.Load(xmlfilepath);
            if (document == null) return;
            //获取到XML的根元素进行操作
            XElement root = document.Root;
            //获取根元素下的所有子元素
            IEnumerable<XElement> enumerable = root.Elements();//Properties
            string temp;
            foreach (XElement p in enumerable)
            {
                temp = p.Attribute("name").Value;

                YCProperty mp = new YCProperty(temp);
                mp.type = p.Attribute("type").Value;
                mp.defaultvalue = p.Element("DefaultValue").Value;
                mp.bList = p.Element("bList").Value == "1";
                mp.bRange = p.Element("bRange").Value == "1";
                mp.min = p.Element("Min").Value;
                mp.max = p.Element("Max").Value;
                mp.bStep = p.Element("bStep").Value == "1";
                mp.step = p.Element("Step").Value;
                mp.enumtype = p.Element("EnumType").Value;
                mp.hasAttribute = p.Element("bAttribute").Value == "1";
                mp.attribute = p.Element("AttributeName").Value;
                mp.attributeInput = p.Element("AttributeInput").Value;
                pc.LstData.Add(mp);

            }
            for (int i = 0; i < pc.LstData.Count; ++i)
            {
                pc.LstData[i].id = i;
            }
        }
    }
}
