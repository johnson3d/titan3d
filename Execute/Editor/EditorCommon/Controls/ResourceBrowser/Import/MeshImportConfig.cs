using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.Controls.ResourceBrowser
{
    public class TextureMaterial
    {
        string mMaterialName = "";
        public string MaterialName
        {
            get { return mMaterialName; }
            set { mMaterialName = value; }
        }

        //通道，原始绝对路径
        private Dictionary<string, string> mTexturesAbsPath = new Dictionary<string, string>();
        public System.Collections.Generic.Dictionary<string, string> TexturesAbsPath
        {
            get { return mTexturesAbsPath; }
            set { mTexturesAbsPath = value; }
        }

        private Dictionary<string, string> mTexturesRelPath = new Dictionary<string, string>();
        public System.Collections.Generic.Dictionary<string, string> TexturesRelPath
        {
            get { return mTexturesRelPath; }
            set { mTexturesRelPath = value; }
        }
    }
    public class MeshConfig
    {
        private String mMeshName = "";
        public System.String MeshName
        {
            get { return mMeshName; }
            set { mMeshName = value; }
        }
        private String mVMSPath = "";
        public System.String VMSPath
        {
            get { return mVMSPath; }
            set { mVMSPath = value; }
        }
        private String mSocketPath = "";
        public System.String SocketPath
        {
            get { return mSocketPath; }
            set { mSocketPath = value; }
        }
        private List<TextureMaterial> mMaterials = new List<TextureMaterial>();
        public System.Collections.Generic.List<TextureMaterial> Materials
        {
            get { return mMaterials; }
            set { mMaterials = value; }
        }

        private EngineNS.Vector3 mPosition = EngineNS.Vector3.Zero;
        public EngineNS.Vector3 Position
        {
            get { return mPosition; }
            set { mPosition = value; }
        }
        private string mMaterialTemplateType = "default";
        public string MaterialTemplateType
        {
            get { return mMaterialTemplateType; }
            set { mMaterialTemplateType = value; }
        }
    }
    class MeshImportConfig
    {
        List<MeshConfig> mMeshConfigs = new List<MeshConfig>();
        public System.Collections.Generic.List<MeshConfig> MeshConfigs
        {
            get { return mMeshConfigs; }
            set { mMeshConfigs = value; }
        }
        public MeshImportConfig()
        {

        }
        public void BatchLoad(string[] filesPath)
        {
            foreach(var filePath in filesPath)
            {
                Load(filePath);
            }
        }
        public void Load(string filePath)
        {
            var hoder = EngineNS.IO.XmlHolder.LoadXML(filePath);
            var rootNode = hoder.RootNode;
            var nodes = rootNode.FindNodes("MeshConfig");
            foreach (var node in nodes)
            {
                MeshConfig meshConfig = new MeshConfig();
                var nameAtt = node.FindAttrib("MeshName");
                if (nameAtt != null)
                    meshConfig.MeshName = nameAtt.Value;
                var vmsPathNode = node.FindNode("VmsPath");
                if (vmsPathNode != null)
                    meshConfig.VMSPath = vmsPathNode.Value;
                var socketPathNode = node.FindNode("SocketPath");
                if (socketPathNode != null)
                    meshConfig.SocketPath = socketPathNode.Value;
                var materialsNode = node.FindNode("Materials");
                if (materialsNode != null)
                {
                    var materialNodes = materialsNode.FindNodes("Material");
                    if (materialNodes != null)
                    {
                        foreach (var materialNode in materialNodes)
                        {
                            TextureMaterial tMat = null;
                            tMat = new TextureMaterial();
                            var name = materialNode.FindAttrib("Name");
                            if (name != null)
                                tMat.MaterialName = name.Value;
                            var datas = materialNode.FindNodes("Data");
                            foreach (var data in datas)
                            {
                                string channel = ""; string tex = "";
                                var channelAtt = data.FindAttrib("Channel");
                                if (channelAtt != null)
                                    channel = channelAtt.Value;
                                var texture = data.FindAttrib("Texture");
                                if (texture != null)
                                    tex = texture.Value;
                                if (channel != "" && tex != "")
                                {
                                    tMat.TexturesAbsPath.Add(channel, tex);
                                }
                            }
                            if (tMat != null)
                                meshConfig.Materials.Add(tMat);
                        }
                    }
                }
                var positionNode = node.FindNode("Position");
                if (positionNode != null)
                {
                    string strPos = positionNode.Value;
                    string[] xyz = strPos.Split(',');
                    if(xyz.Length==3)
                    {
                        float x,y,z;
                        int result = 1;
                        result *= (float.TryParse(xyz[0], out x))?1:0;
                        result *= (float.TryParse(xyz[1], out y)) ? 1 : 0;
                        result *= (float.TryParse(xyz[2], out z)) ? 1 : 0;
                        if(result==1)
                            meshConfig.Position = new EngineNS.Vector3(x,y,z);
                    }
                    
                }
                var materialTemplateTypeNode = node.FindNode("MaterialTemplateType");
                if(materialTemplateTypeNode!=null)
                {
                    meshConfig.MaterialTemplateType = materialTemplateTypeNode.Value;
                }
                mMeshConfigs.Add(meshConfig);
            }
        }
    }
}
