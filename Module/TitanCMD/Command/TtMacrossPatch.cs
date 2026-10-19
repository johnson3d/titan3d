using System;
using EngineNS.Macross;

namespace ProjectCooker.Command
{
    /// <summary>
    /// macross-patch 子命令：调用 TtMacrossPatchBuilder 编译/校验/打包补丁。
    /// 用法：TitanCMD ExeCmd=macross-patch PatchProject=path BaselinePath=path OutputDir=path
    /// </summary>
    class TtMacrossPatch : TtCookCommand
    {
        public override async EngineNS.Thread.Async.TtTask<int> ExecuteCommand(string[] args)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();

            var patchProject = FindArgument(args, "PatchProject=");
            var baselinePath = FindArgument(args, "BaselinePath=");
            var outputDir = FindArgument(args, "OutputDir=");

            if (string.IsNullOrEmpty(patchProject) || string.IsNullOrEmpty(baselinePath) || string.IsNullOrEmpty(outputDir))
            {
                Console.Error.WriteLine("Usage: ExeCmd=macross-patch PatchProject=<dir> BaselinePath=<file> OutputDir=<dir>");
                return 1;
            }

            Console.WriteLine($"macross-patch: PatchProject={patchProject}");
            Console.WriteLine($"macross-patch: BaselinePath={baselinePath}");
            Console.WriteLine($"macross-patch: OutputDir={outputDir}");

            var builder = new TtMacrossPatchBuilder(patchProject, baselinePath, outputDir);
            var result = builder.Build();

            foreach (var diag in result.Diagnostics)
                Console.WriteLine(diag);

            return result.Success ? 0 : 1;
        }
    }
}
