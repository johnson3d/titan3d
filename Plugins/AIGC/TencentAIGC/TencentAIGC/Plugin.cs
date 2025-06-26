using System;
using System.Collections.Generic;
using System.Text;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Hunyuan.V20230901;
using TencentCloud.Hunyuan.V20230901.Models;
using static NPOI.HSSF.Util.HSSFColor;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class TtTencentAIGCAssemblyDesc : TtAssemblyDesc
        {
            public TtTencentAIGCAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:TencentAIGC AssemblyDesc Created");
            }
            ~TtTencentAIGCAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:TencentAIGC AssemblyDesc Destroyed");
            }
            public override string Name { get => "TencentAIGC"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static TtTencentAIGCAssemblyDesc AssmblyDesc = new TtTencentAIGCAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.DataCopyer
{
    [EngineNS.Bricks.AssemblyLoader.TtPlugin]
    public class TtPluginLoader
    {
        public static TtTencentAIGCPlugin mPluginObject = new TtTencentAIGCPlugin();
        public static EngineNS.Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public partial class TtTencentAIGCPlugin : EngineNS.Bricks.AIGC.TtAIGCPlugin
    {
        /// <summary>
        /// https://console.cloud.tencent.com/
        /// </summary>
        Credential mCred;
        ClientProfile mClientProfile = new ClientProfile();
        public override void Initialize(string appId, string apiKey)
        {
            mCred = new Credential
            {
                SecretId = appId,
                SecretKey = apiKey
            };
        }
        public override async Thread.Async.TtTask<FMeshResult> GenerateMeshFromImage(byte[] imageBytes)
        {
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = ("hunyuan.tencentcloudapi.com");
            mClientProfile.HttpProfile = httpProfile;
            HunyuanClient client = new HunyuanClient(mCred, "ap-guangzhou", mClientProfile);

            SubmitHunyuanTo3DJobRequest req = new SubmitHunyuanTo3DJobRequest();
            req.ImageBase64 = Convert.ToBase64String(imageBytes);
            req.Num = 1;

            //SubmitHunyuanTo3DJobResponse resp = client.SubmitHunyuanTo3DJobSync(req);
            //Console.WriteLine(AbstractModel.ToJsonString(resp));
            SubmitHunyuanTo3DJobResponse response = await client.SubmitHunyuanTo3DJob(req);
            //response.JobId;
            //response.RequestId;

            return new FMeshResult
            {
                Type = EMeshType.Obj,
            };
        }
        public override async Thread.Async.TtTask<FMeshResult> GenerateMeshFromText(string description)
        {
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = ("hunyuan.tencentcloudapi.com");
            mClientProfile.HttpProfile = httpProfile;
            HunyuanClient client = new HunyuanClient(mCred, "ap-guangzhou", mClientProfile);

            SubmitHunyuanTo3DJobRequest req = new SubmitHunyuanTo3DJobRequest();
            req.Prompt = description;
            req.Num = 1;

            try
            {
                SubmitHunyuanTo3DJobResponse resp = client.SubmitHunyuanTo3DJobSync(req);
                //Console.WriteLine(AbstractModel.ToJsonString(resp));
                QueryHunyuanTo3DJobRequest req2 = new QueryHunyuanTo3DJobRequest();
                req2.JobId = resp.JobId;
                QueryHunyuanTo3DJobResponse resp2 = client.QueryHunyuanTo3DJobSync(req2);
                if (resp2.ResultFile3Ds[0].File3D[0].Type == "GIF")
                {
                    //resp2.ResultFile3Ds[0].File3D[0].Url
                }
                if (resp2.ResultFile3Ds[0].File3D[1].Type == "OBJ")
                {
                    //resp2.ResultFile3Ds[0].File3D[1].Url
                }
                Console.WriteLine(AbstractModel.ToJsonString(resp2));
            }
            catch (TencentCloudSDKException ex)
            {
                Console.WriteLine($"腾讯云SDK异常: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"其他异常: {ex.Message}");
            }
            
            //SubmitHunyuanTo3DJobResponse response = await client.SubmitHunyuanTo3DJob(req);
            //response.JobId;
            //response.RequestId;

            return new FMeshResult
            {
                Type = EMeshType.Obj,
            };
        }
        public override async Thread.Async.TtTask<FMeshResult> GenerateAnimationFromVideo(byte[] skeletalBytes, byte[] videoBytes)
        {
            return new FMeshResult
            {
                Type = EMeshType.Obj,
            };
        }
        public override async Thread.Async.TtTask<FMeshResult> GenerateAnimationFromText(byte[] skeletalBytes, string description)
        {
            return new FMeshResult
            {
                Type = EMeshType.Obj,
            };
        }
    }
}