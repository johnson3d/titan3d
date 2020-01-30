using System;
using System.Collections.Generic;
using System.Text;

using EngineNS.IO;

namespace EnsureFile
{
    public class Compare
    {
        public void FindSubFolder(XmlNode xnode, string folder, Dictionary<string, string> files)
        {
            XmlNode filenode = xnode.FindNode("Files");
            if (filenode != null)
            {
                List<XmlNode> filenodes = filenode.FindNodes("File");
                if (filenodes != null)
                {
                    foreach (var node in filenodes)
                    {
                        XmlAttrib name = node.FindAttrib("Name");
                        XmlAttrib md5 = node.FindAttrib("MD5");
                        string str = folder + "/" + name.Value;
                        if (files.ContainsKey(str) == false)
                        {
                            files.Add(str, md5.Value);
                        }
                        else
                        {
                            Console.WriteLine("Compare Class the file is alive: " + str);
                        }
                    }
                }

            }

            List<XmlNode> nodes = xnode.FindNodes("Folder");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    XmlAttrib name = node.FindAttrib("Name");
                    FindSubFolder(node, folder + "/" + name.Value, files);
                }
            }
        }

        public void ConvertXMLToList(XmlHolder xml, Dictionary<string, string> files)
        {
            if (xml == null)
                return;

            if (xml.RootNode == null)
                return;

            FindSubFolder(xml.RootNode, "", files);

        }
        //比较两个mdd5文件 返回需要更新的文件
        public string[] CompareMD5(string oldfile, string newfile)
        {
            if (System.IO.File.Exists(newfile) == false)
                return null;

            List<string> needfiles = new List<string>();
            if (System.IO.File.Exists(oldfile) == false)
            {
                using (XmlHolder xml = XmlHolder.LoadXML(newfile))
                {
                    Dictionary<string, string> files = new Dictionary<string, string>();
                    ConvertXMLToList(xml, files);
                    var enu = files.GetEnumerator();
                    while (enu.MoveNext())
                    {
                        //老的里面没有 新的里面有的
                        needfiles.Add(enu.Current.Key);
                    }
                    enu.Dispose();
                }

                return needfiles.ToArray();
            }

            XmlHolder oldxml = XmlHolder.LoadXML(oldfile);
            Dictionary<string, string> oldfiles = new Dictionary<string, string>();
            ConvertXMLToList(oldxml, oldfiles);

            XmlHolder newxml = XmlHolder.LoadXML(newfile);
            Dictionary<string, string> newfiles = new Dictionary<string, string>();
            ConvertXMLToList(newxml, newfiles);

            List<string> deletefiles = new List<string>();
            var oldenu = oldfiles.GetEnumerator();
            while (oldenu.MoveNext())
            {
                string result;
                if (newfiles.TryGetValue(oldenu.Current.Key, out result))
                {
                    if (result.Equals(oldenu.Current.Value))
                    {
                        //一样
                        //int xx = 0;
                    }
                    else
                    {
                        needfiles.Add(oldenu.Current.Key);
                    }
                    
                    newfiles.Remove(oldenu.Current.Key);
                }
                else
                {
                    //老的里面有 新的里面没有
                    deletefiles.Add(oldenu.Current.Key);

                }
            }
            oldenu.Dispose();


            var newenu = newfiles.GetEnumerator();
            while (newenu.MoveNext())
            {
                //老的里面没有 新的里面有的
                needfiles.Add(newenu.Current.Key);
            }
            newenu.Dispose();

            return needfiles.ToArray();
        }
    }
}
