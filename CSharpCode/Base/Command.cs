using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace EngineNS
{
    public class TtCommand
    {
        public string CmdName { get; protected set; }
        public string CmdHelp { get; protected set; }
        public KeyValuePair<string, string>[] GetArguments(string txt)
        {
            if (txt == null)
                return null;
            var segs = txt.Split(' ');
            var result = new KeyValuePair<string, string>[segs.Length];
            for (int i = 0; i < segs.Length; i++)
            {
                var a = segs[i].Split('=');
                if(a.Length == 2)
                {
                    result[i] = new KeyValuePair<string, string>(a[0], a[1]);
                }
            }
            return result;
        }
        public string FindArgument(KeyValuePair<string, string>[] args, string argName)
        {
            if (args == null)
                return null;
            foreach(var i in args)
            {
                if (i.Key == argName)
                    return i.Value;
            }
            return null;
        }
        public virtual void Execute(string args)
        {

        }
    }

    public class TtCommandManager
    {
        public Dictionary<string, TtCommand> Commands { get; } = new Dictionary<string, TtCommand>();
        static TtCommandManager mInstance;
        public static TtCommandManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new TtCommandManager();
                    mInstance.Initialize();
                }
                return mInstance;
            }
        }
        public TtCommand TryGetCommand(string name)
        {
            TtCommand cmd;
            if (Commands.TryGetValue(name, out cmd))
                return cmd;
            return null;
        }
        void Initialize()
        {
            Commands.Clear();
            foreach (var i in Rtti.TtTypeDescManager.Instance.Services)
            {
                foreach (var j in i.Value.Types)
                {
                    if (j.Value.IsSubclassOf(Rtti.TtTypeDescGetter<TtCommand>.TypeDesc))
                    {
                        var cmd = Rtti.TtTypeDescManager.CreateInstance(j.Value) as TtCommand;
                        Commands.Add(cmd.CmdName, cmd);
                    }
                }
            }
        }
    }
    
    [Macross.TtMacross()]
    public partial class TtCommandMacross
    {
        [Rtti.Meta()]
        public delegate bool OnProcAsset(IO.IAssetMeta ameta);
        [Rtti.Meta()]
        public void IterateDirectory(string dir, string ext, OnProcAsset fun, bool bAllDir = false)
        {
            if (dir == null)
                return;
            dir = RName.GetRName(dir).Address;
            var files = IO.TtFileManager.GetFiles(dir, "*" + ext, bAllDir);
            var root = EngineNS.TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game);
            foreach (var i in files)
            {
                var rp = EngineNS.IO.TtFileManager.GetRelativePath(root, i);
                var rn = EngineNS.RName.GetRName(rp, EngineNS.RName.ERNameType.Game);
                var ameta = EngineNS.TtEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
                if (ameta == null)
                {
                    continue;
                }
                fun(ameta);
            }
        }
        [Rtti.Meta()]
        public virtual async Thread.Async.TtTask DoCommand(TtMcCommand host)
        {

        }
    }
    public partial class TtMcCommand : TtCommand
    {
        public TtMcCommand()
        {
            CmdName = "McCmd";
            CmdHelp = "McCmd Macross=(string) OnGameThread=(bool)";
        }
        [Category("Option")]
        [Rtti.Meta]
        [RName.PGRName(FilterExts = EngineNS.Bricks.CodeBuilder.TtMacross.AssetExt, MacrossType = typeof(TtCommandMacross))]
        public RName McName
        {
            get
            {
                if (mMcObject == null)
                    return null;
                return mMcObject.Name;
            }
            set
            {
                if (value == null)
                {
                    mMcObject = null;
                    return;
                }
                if (mMcObject == null)
                {
                    mMcObject = Macross.UMacrossGetter<TtCommandMacross>.NewInstance();
                }
                mMcObject.Name = value;
            }
        }
        Macross.UMacrossGetter<TtCommandMacross> mMcObject;
        public Macross.UMacrossGetter<TtCommandMacross> McObject
        {
            get => mMcObject;
        }
        KeyValuePair<string, string>[] Args;
        [Rtti.Meta]
        public string FindArgument(string argName)
        {
            return FindArgument(Args, argName);
        }
        public override void Execute(string argsText)
        {
            Args = GetArguments(argsText);
            var flt = FindArgument(Args, "Macross");
            bool bRunInLogicThread = false;
            var OnGameThread = FindArgument(Args, "OnGameThread");
            if (OnGameThread != null)
            {
                bRunInLogicThread = System.Convert.ToBoolean(OnGameThread);
            }

            if (flt.StartsWith("@Game/"))
            {
                flt = flt.Substring("@Game/".Length);
                McName = RName.GetRName(flt + Bricks.CodeBuilder.TtMacross.AssetExt, RName.ERNameType.Game);
            }
            else if (flt.StartsWith("@Engine/"))
            {
                flt = flt.Substring("@Engine/".Length);
                McName = RName.GetRName(flt + Bricks.CodeBuilder.TtMacross.AssetExt, RName.ERNameType.Engine);
            }
            else
            {
                McName = RName.GetRName(flt + Bricks.CodeBuilder.TtMacross.AssetExt, RName.ERNameType.Game);
            }
            if (McObject != null)
            {
                if (bRunInLogicThread)
                {
                    TtEngine.Instance.EventPoster.RunOn((state) =>
                    {
                        McObject.Get()?.DoCommand(this);
                        return true;
                    }, Thread.Async.EAsyncTarget.Logic);
                }
                else
                {
                    McObject.Get()?.DoCommand(this);
                }
            }
        }
    }

    public class TtCommand_List : TtCommand
    {
        public TtCommand_List()
        {
            CmdName = "List";
            CmdHelp = "List Filter=(string)";
        }
        public override void Execute(string argsText)
        {
            var args = GetArguments(argsText);
            var flt = FindArgument(args, "Filter");
            
            Profiler.Log.WriteLine<Profiler.TtDebugLogCategory>(Profiler.ELogTag.Info, $"List Command with {flt}:");
            foreach (var i in TtCommandManager.Instance.Commands)
            {
                if (string.IsNullOrEmpty(flt) || i.Key.Contains(flt))
                {
                    Profiler.Log.WriteLine<Profiler.TtDebugLogCategory>(Profiler.ELogTag.Info, $"{i.Key}");
                }
            }
        }
    }

    public class TtCommand_Help : TtCommand
    {
        public TtCommand_Help()
        {
            CmdName = "Help";
            CmdHelp = "Help Cmd=(string)";
        }
        public override void Execute(string argsText)
        {
            var args = GetArguments(argsText);
            var id = FindArgument(args, "Cmd");
            if (id == null)
                return;

            var cmd = TtCommandManager.Instance.TryGetCommand(id);
            if (cmd == null)
            {
                Profiler.Log.WriteLine<Profiler.TtDebugLogCategory>(Profiler.ELogTag.Info, $"Command {id} is not found");
            }
            else
            {
                Profiler.Log.WriteLine<Profiler.TtDebugLogCategory>(Profiler.ELogTag.Info, $"Help {id}: {cmd.CmdHelp}");
            }
        }
    }

    public class TtCommand_GpuSetBreakOn : TtCommand
    {
        public TtCommand_GpuSetBreakOn()
        {
            CmdName = "GpuSetBreakOn";
            CmdHelp = "GpuSetBreakOn Id=(int) Op=(true:false)";
            //CREATE_HEAP 566 
            //DESTROY_HEAP 592
        }
        public override void Execute(string argsText)
        {
            var args = GetArguments(argsText);
            var id = FindArgument(args, "Id");
            if (id == null)
                return;
            
            var op = FindArgument(args, "Op");
            bool open = true;
            if (op != null)
            {
                if (op == "false")
                {
                    open = false;
                }
            }
            TtEngine.Instance.GfxDevice.RenderContext.SetBreakOnId(int.Parse(id), open);
        }
    }

    public class TtCommand_PrintAttachmentPool : TtCommand
    {
        public TtCommand_PrintAttachmentPool()
        {
            CmdName = "PrintAttachmentPool";
            CmdHelp = "PrintAttachmentPool";
        }
        public override void Execute(string argsText)
        {
            TtEngine.Instance.GfxDevice.AttachBufferManager.PrintCachedBuffer = true;
        }
    }
}
