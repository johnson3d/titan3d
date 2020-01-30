using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Runner
{
    public class ProcessExecuter
    {
        public static async System.Threading.Tasks.Task<bool> RunProcess(string cmd, List<string> args, List<string> outInfos)
        {
            if (CEngine.Instance.FileManager.FileExists(cmd) == false)
                return false;
            await CEngine.Instance.EventPoster.Post(() =>
            {
                var p = new System.Diagnostics.Process();
                p.EnableRaisingEvents = true;
                p.StartInfo.FileName = cmd;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string finalArgs = "";
                foreach (var i in args)
                {
                    finalArgs += i + " ";
                }
                p.StandardInput.WriteLine(finalArgs + " &exit");
                p.StandardInput.AutoFlush = true;
                outInfos.Add(p.StandardOutput.ReadToEnd());
                outInfos.Add(p.StandardError.ReadToEnd());
                p.WaitForExit();
                p.Close();
                return true;
            }, Thread.Async.EAsyncTarget.AsyncEditorSlow);

            return true;
        }
        public static async System.Threading.Tasks.Task<bool> RunEditorCMD(List<string> args, List<string> outInfos)
        {
            string cmd = CEngine.Instance.FileManager.Bin + "EditorCMD.exe";
            return await RunProcess(cmd, args, outInfos);
        }
        public static async System.Threading.Tasks.Task<bool> RunCook(RName entry, EPlatformType platform, int shaderModel,
            bool bCookShader, bool bRecompileShader, bool bGenVSProj, bool bCopyInfo, List<string> outInfos)
        {
            List<string> args = new List<string>();
            args.Add("cook");
            args.Add($"entry={entry.Name}");
            args.Add($"platform={PlatformType2Name(platform)}");
            args.Add($"shadermodel={shaderModel.ToString()}");

            if(bCookShader)
                args.Add("cookshader");
            if (bRecompileShader)
                args.Add("recompile");
            if (bGenVSProj)
                args.Add("genvsproj");
            if (bCopyInfo)
                args.Add("copyrinfo");

            return await RunEditorCMD(args, outInfos);
        }
        public static string PlatformType2Name(EPlatformType platform)
        {
            switch (platform)
            {
                case EPlatformType.PLATFORM_DROID:
                    return "android";
                case EPlatformType.PLATFORM_IOS:
                    return "ios";
                case EPlatformType.PLATFORM_WIN:
                    return "windows";
                default:
                    return "unknown";
            }
        }
    }
}
