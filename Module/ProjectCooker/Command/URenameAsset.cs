using System;
using System.Collections.Generic;
using EngineNS;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCooker.Command
{
    class URenameAsset : UCookCommand
    {
        public const string Param_Source = "Source=";
        public const string Param_Target = "Target=";
        public override async System.Threading.Tasks.Task ExecuteCommand(string[] args)
        {
            var source = FindArgument(args, Param_Source);
            var target = FindArgument(args, Param_Target);

            var srcName = RName.ParseFrom(source);
            var tarName = RName.ParseFrom(target);
            srcName = RName.GetRName("utest/mesh/puppet_low_ue4.vms");
            tarName = RName.GetRName("utest/mesh/puppet_low_ue4_renamed.vms");
            if (srcName == null || tarName == null || tarName.ExtName != srcName.ExtName)
            {
                throw new Exception("Source Or Target is invalid");
            }
            switch (srcName.ExtName)
            {
                case ".srv":
                    {
                        
                    }
                    break;
                case ".vms":
                    {
                        await ProcMesh(srcName, tarName);
                    }
                    break;
                case ".material":
                    {

                    }
                    break;
                case ".uminst":
                    {

                    }
                    break;
                default:
                    break;
            }
        }
        async System.Threading.Tasks.Task<EngineNS.IO.IAsset> LoadAsset(RName name)
        {
            switch (name.ExtName)
            {
                case ".srv":
                    {

                    }
                    break;
                case ".vms":
                    {
                        
                    }
                    break;
                case ".ums":
                    {
                        return await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(name);
                    }
                    break;
                case ".material":
                    {

                    }
                    break;
                case ".uminst":
                    {

                    }
                    break;
                default:
                    break;
            }
            return null;
        }
        async System.Threading.Tasks.Task ProcMesh(EngineNS.RName src, EngineNS.RName tar)
        {
            var asset = await EngineNS.UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(src);
            if (asset == null)
                return;
            var ameta = asset.GetAMeta();
            if (ameta == null)
                return;

            List<EngineNS.IO.IAssetMeta> holders = new List<EngineNS.IO.IAssetMeta>();
            UEngine.Instance.AssetMetaManager.GetAssetHolder(ameta, holders);

            List<EngineNS.IO.IAsset> holdAssets = new List<EngineNS.IO.IAsset>();
            foreach (var i in holders)
            {
                var holdAsset = await LoadAsset(i.GetAssetName());
                if (holdAsset != null)
                {
                    holdAssets.Add(holdAsset);
                }
            }

            var ametaPath = ameta.GetAssetName().Address + IO.IAssetMeta.MetaExt;
            ameta.SetAssetName(tar);
            ameta.SaveAMeta();
            System.IO.File.Delete(ametaPath);
            //asset.SaveAssetTo(tar);
            System.IO.File.Move(src.Address, tar.Address);

            EngineNS.UEngine.Instance.GfxDevice.MeshPrimitiveManager.UnsafeRenameForCook(src, tar);

            foreach (var i in holdAssets)
            {
                i.SaveAssetTo(i.GetAMeta().GetAssetName());
            }
        }
    }
}
