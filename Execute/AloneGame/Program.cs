using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AloneGame
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int sm = 5;
            var gpu = FindArgument(args, "gpu=");
            if (gpu != null)
            {
                gpu = gpu.Substring("gpu=".Length);
                var caps = MobileGPUSimulator.Instance.GetGPUCaps(gpu);
                MobileGPUSimulator.Instance.Gpu = gpu;
                sm = caps.ShaderModel;
            }

            EngineNS.IO.FileManager.UseCooked = "android";
            var platform = FindArgument(args, "usecooked=");
            if (platform != null)
            {
                EngineNS.IO.FileManager.UseCooked = platform.Substring("usecooked=".Length);
            }

            if (EngineNS.IO.FileManager.UseCooked == "android")
                EngineNS.IO.FileManager.UseCooked += "/Assets";

            var texFormat = FindArgument(args, "textureformat=");
            if (texFormat != null)
            {
                texFormat = texFormat.Substring("textureformat=".Length);
                switch(texFormat)
                {
                    case "none":
                        EngineNS.CGfxTextureStreaming.TryCompressFormat = EngineNS.CGfxTextureStreaming.EPixelCompressMode.None;
                        break;
                    case "etc2":
                        EngineNS.CGfxTextureStreaming.TryCompressFormat = EngineNS.CGfxTextureStreaming.EPixelCompressMode.ETC2;
                        break;
                    case "astc":
                        EngineNS.CGfxTextureStreaming.TryCompressFormat = EngineNS.CGfxTextureStreaming.EPixelCompressMode.ASTC;
                        break;
                }
            }

            EngineNS.CRenderContext.ShaderModel = sm;

            var GameForm = new Form1();
            if (FindArgument(args, "create_debug_layer") != null)
            {
                GameForm.CreateDebugLayer = true;
            }
            else
            {
                GameForm.CreateDebugLayer = false;
            }
            
            GameForm.Show();

            //EngineNS.CIPlatform.Instance.AttachCLRProfiler();
            EngineNS.CIPlatform.Instance.WindowFormRun();
            //Application.Run(new Form1());
        }

        static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                    return i;
            }
            return null;
        }
    }
}
