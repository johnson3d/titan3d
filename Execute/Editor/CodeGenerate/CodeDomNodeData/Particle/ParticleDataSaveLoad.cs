using EngineNS.IO;
using Macross;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode.Particle
{
    public class ParticleDataSaveLoad
    {

        public static void SaveData(string name, XndNode xndNode, bool newGuid, StructNodeControlConstructionParams csparam, IParticleNode pnode, CodeGenerateSystem.Controls.NodesContainerControl mLinkedNodesContainer)
        {
            var psd = pnode as IParticleSaveData;
            if (psd != null && psd.IsLoadLink() == false)
            {
               var test = ParticleDataSaveLoad.LoadData(csparam, mLinkedNodesContainer, psd);
            }

            if (mLinkedNodesContainer != null)
            {
                var att = xndNode.AddAttrib(name);
                att.Version = 0;
                att.BeginWrite();

                List<string> keys = new List<string>();
                //var csparam = CSParam as StructNodeControlConstructionParams;
                if (csparam.CategoryDic == null)
                {
                    att.Write(0);
                    att.EndWrite();
                }
                else
                {
                    att.Write(csparam.CategoryDic.Count);
                    foreach (var category in csparam.CategoryDic)
                    {
                        att.Write(category.Key);
                        att.Write(category.Value.Items.Count);

                        keys.Add(category.Key);

                    }
                    att.EndWrite();

                    for (int i = 0; i < keys.Count; i++)
                    {
                        var Node = xndNode.AddNode(keys[i], 0, 0);
                        var category = csparam.CategoryDic[keys[i]];
                        for (int j = 0; j < category.Items.Count; j++)
                        {
                            var childNode = Node.AddNode("childNode", 0, 0);
                            category.Items[j].Save(childNode);
                        }
                    }
                }

            }

            //base.Save(xndNode, newGuid);
            if (pnode.GetCreateObject() != null)
            {
                var childenode = xndNode.AddNode("CreateObjectNode", 0, 0);

                pnode.GetCreateObject().Save(childenode, newGuid);
            }
            
        }

        public static async System.Threading.Tasks.Task<bool> LoadData(StructNodeControlConstructionParams csparam, CodeGenerateSystem.Controls.NodesContainerControl mLinkedNodesContainer, IParticleSaveData psd)
        {
            if (psd == null)
                return false;

            if (mLinkedNodesContainer == null)
            {
                await psd.AwaitLoad();
            }

            if (psd.IsLoadLink())
                return true;

            var xndNode = psd.GetXndNode();
            if (xndNode == null)
                return false;

            var att = xndNode.FindAttrib(psd.GetXndAttribName());
            if (att != null)
            {
                List<string> keys = new List<string>();
                switch (att.Version)
                {
                    case 0:
                        {
                            att.BeginRead();

                            int count = 0;
                            att.Read(out count);
                            if (count != 0)
                            {
                                Macross.NodesControlAssist NodesControlAssist;
                                MacrossPanelBase MacrossOpPanel;
                                IMacrossOperationContainer NodesControlAssist_HostControl;

                                NodesControlAssist = mLinkedNodesContainer.HostControl as Macross.NodesControlAssist;
                                MacrossOpPanel = NodesControlAssist.HostControl.MacrossOpPanel;
                                NodesControlAssist_HostControl = NodesControlAssist.HostControl;

                                csparam.CategoryDic = new Dictionary<string, Category>();
                                for (int i = 0; i < count; i++)
                                {
                                    string key;
                                    int itemcount;

                                    att.Read(out key);

                                    keys.Add(key);

                                    att.Read(out itemcount);
                                    csparam.CategoryDic[key] = new Category(MacrossOpPanel);

                                    //var category = csparam.CategoryDic[key];
                                    //for (int j = 0; j < itemcount; j++)
                                    //{
                                    //    category.Items.Add(new CategoryItem(null, category));
                                    //    //category.Items[j].Load(xndNode, NodesControlAssist_HostControl);
                                    //}
                                }
                                //此刻已经读完
                                att.EndRead();
                                for (int i = 0; i < keys.Count; i++)
                                {
                                    var Node = xndNode.FindNode(keys[i]);
                                    var category = csparam.CategoryDic[keys[i]];

                                    var childNodes = Node.GetNodes();
                                    foreach (var childNode in childNodes)
                                    {
                                        var item = new CategoryItem(null, category);
                                        item.Load(childNode, NodesControlAssist_HostControl);
                                        category.Items.Add(item);
                                    }
                                }
                            }
                        }
                        break;
                }
                //att.EndRead();

            }

            psd.SetLoadLink(true);

            return true;

        }

        public static async System.Threading.Tasks.Task<bool> LoadData2(string name, XndNode xndNode, StructNodeControlConstructionParams csparam, IParticleNode pnode, CodeGenerateSystem.Controls.NodesContainerControl mLinkedNodesContainer)
        {
            var psd = pnode as IParticleSaveData;
            if (psd != null)
            {
                psd.SetXndNode(xndNode);
                psd.SetXndAttribName(name);
            }


            if (pnode.GetCreateObject() != null)
            {
                var childenode = xndNode.FindNode("CreateObjectNode");
                await pnode.GetCreateObject().Load(childenode);
            }

            return false;
        }

        public static async System.Threading.Tasks.Task<bool> LoadData(string name, XndNode xndNode, StructNodeControlConstructionParams csparam, IParticleNode pnode, CodeGenerateSystem.Controls.NodesContainerControl mLinkedNodesContainer)
        {
            if (mLinkedNodesContainer != null)
            {
                var att = xndNode.FindAttrib(name);
                if (att != null)
                {
                    List<string> keys = new List<string>();
                    switch (att.Version)
                    {
                        case 0:
                            {
                                att.BeginRead();

                                int count = 0;
                                att.Read(out count);
                                if (count != 0)
                                {
                                    Macross.NodesControlAssist NodesControlAssist;
                                    MacrossPanelBase MacrossOpPanel;
                                    IMacrossOperationContainer NodesControlAssist_HostControl;

                                    NodesControlAssist = mLinkedNodesContainer.HostControl as Macross.NodesControlAssist;
                                    MacrossOpPanel = NodesControlAssist.HostControl.MacrossOpPanel;
                                    NodesControlAssist_HostControl = NodesControlAssist.HostControl;

                                    csparam.CategoryDic = new Dictionary<string, Category>();
                                    for (int i = 0; i < count; i++)
                                    {
                                        string key;
                                        int itemcount;

                                        att.Read(out key);

                                        keys.Add(key);

                                        att.Read(out itemcount);
                                        csparam.CategoryDic[key] = new Category(MacrossOpPanel);

                                        //var category = csparam.CategoryDic[key];
                                        //for (int j = 0; j < itemcount; j++)
                                        //{
                                        //    category.Items.Add(new CategoryItem(null, category));
                                        //    //category.Items[j].Load(xndNode, NodesControlAssist_HostControl);
                                        //}
                                    }
                                    //此刻已经读完
                                    att.EndRead();
                                    for (int i = 0; i < keys.Count; i++)
                                    {
                                        var Node = xndNode.FindNode(keys[i]);
                                        var category = csparam.CategoryDic[keys[i]];

                                        var childNodes = Node.GetNodes();
                                        foreach (var childNode in childNodes)
                                        {
                                            var item = new CategoryItem(null, category);
                                            item.Load(childNode, NodesControlAssist_HostControl);
                                            category.Items.Add(item);
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    //att.EndRead();

                }
            }


            if (pnode.GetCreateObject() != null)
            {
                var childenode = xndNode.FindNode("CreateObjectNode");
                await pnode.GetCreateObject().Load(childenode);
            }

            return false;
        }


        public static void ResetNodeConrol(bool NeedResetLoadValue, CodeGenerateSystem.Controls.NodesContainerControl mLinkedNodesContainer, StructNodeControlConstructionParams csparam)
        {
            if (NeedResetLoadValue == false)
                return;

            Macross.NodesControlAssist NodesControlAssist = mLinkedNodesContainer.HostControl as Macross.NodesControlAssist;
            MacrossPanelBase MacrossOpPanel = NodesControlAssist.HostControl.MacrossOpPanel;
            IMacrossOperationContainer NodesControlAssist_HostControl = NodesControlAssist.HostControl;
            var categoryDic = csparam.CategoryDic;
            csparam.CategoryDic = new Dictionary<string, Category>();
            foreach (var categorydic in categoryDic)
            {
                csparam.CategoryDic[categorydic.Key] = new Category(MacrossOpPanel);
                for (int i = 0; i < categorydic.Value.Items.Count; i++)
                {
                    csparam.CategoryDic[categorydic.Key].Items.Add(categorydic.Value.Items[i]);
                    categorydic.Value.Items[i].SetParentCategory(csparam.CategoryDic[categorydic.Key]);

                    var data = new Macross.CategoryItem.InitializeData();
                    data.Reset();
                    categorydic.Value.Items[i].Initialize(NodesControlAssist_HostControl, data);
                }
            }
        }
    }
}
