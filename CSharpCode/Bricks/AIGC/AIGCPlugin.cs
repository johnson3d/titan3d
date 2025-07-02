using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AIGC
{
    public abstract class TtAIGCPlugin : AssemblyLoader.IPlugin
    {
        public virtual void OnLoadedPlugin()
        {

        }
        public virtual void OnUnloadPlugin()
        {

        }
        public enum EMeshType
        {
            Unknown = 0,
            Obj,
            Glb,
            Fbx,
        }
        public struct FMeshResult
        {
            public EMeshType Type;
            public Support.TtBlobObject MeshData;
        }
        public abstract void Initialize(string appId, string apiKey);
        public abstract Thread.Async.TtTask<FMeshResult> GenerateMeshFromImage(byte[] imageBytes);
        public abstract Thread.Async.TtTask<FMeshResult> GenerateMeshFromText(string description);
        public abstract Thread.Async.TtTask<FMeshResult> GenerateAnimationFromVideo(byte[] skeletalBytes, byte[] videoBytes);
        public abstract Thread.Async.TtTask<FMeshResult> GenerateAnimationFromText(byte[] skeletalBytes, string description);

        public static TtAIGCPlugin FindAIGCPlugin(string pluginName = "TencentAIGC")
        {
            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule(pluginName);
            if (serverPlugin != null)
            {
                return serverPlugin.GetPluginObject<TtAIGCPlugin>();
            }
            return null;
        }
    }
}

namespace EngineNS.UnitTest
{
    [UnitTest.TtTest(Enable = false)]
    public class TtTest_AIGCPlugin
    {
        public void UnitTestEntrance()
        {
            var action = async () =>
            {
                var plugin = Bricks.AIGC.TtAIGCPlugin.FindAIGCPlugin();
                if (plugin!=null)
                {
                    var root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.PluginSource);
                    var text = IO.TtFileManager.ReadAllText(root + "/AIGC/TencentAIGC/AppKey.txt");
                    if (text!=null)
                    {
                        var segs = text.Split(';');
                        plugin.Initialize(segs[0], segs[1]);
                        var t = await plugin.GenerateMeshFromText("A flower");
                    }
                }
            };
            action();
        }
    }
}