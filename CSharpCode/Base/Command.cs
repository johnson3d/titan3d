using System;
using System.Collections.Generic;
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
