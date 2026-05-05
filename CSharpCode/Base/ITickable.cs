using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EngineNS
{
    public interface ITickable
    {
        //int GetTickOrder();
        void TickLogic(float ellapse);
        void TickRender(float ellapse);
        void TickBeginFrame(float ellapse);
        void TickSync(float ellapse);
    }

    public struct FHostNotify
    {
        public int Msg;
        public string Info;
        public object Parameter;
    }

    public interface IMemberTickable
    {
        Thread.Async.TtTask<bool> Initialize(object host);
        void Cleanup(object host);
        void TickLogic(object host, float ellapse);
        void OnHostNotify(object host, in FHostNotify notify);
    }
    public class TtMemberTickables
    {
        private List<IMemberTickable> Members = new List<IMemberTickable>();
        public void CollectMembers(object host)
        {
            var type = host.GetType();
            var props = host.GetType().GetProperties();
            foreach (var i in props)
            {
                if (Rtti.TtTypeDesc.GetInterface(i.PropertyType,"IMemberTickable") != null)
                {
                    var member = i.GetValue(host) as IMemberTickable;
                    if (member != null)
                        Members.Add(member);
                }
            }
        }
        public async Thread.Async.TtTask InitializeMembers(object host)
        {
            foreach (var i in Members)
            {
                if (await i.Initialize(host) == false)
                {
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"TtMemberTickables:{i.GetType().FullName} Initialize failed");
                }
            }
        }
        public void CleanupMembers(object host)
        {
            foreach (var i in Members)
            {
                i.Cleanup(host);
            }
            Members.Clear();
        }
        public void TickLogic(object host, float ellapse)
        {
            foreach(var i in Members)
            {
                i.TickLogic(host, ellapse);
            }
        }
        public void SendNotify(object host, in FHostNotify notify)
        {
            foreach (var i in Members)
            {
                i.OnHostNotify(host, in notify);
            }
        }
    }

    public partial class TtTickableManager
    {
        public List<WeakReference<ITickable>> Tickables { get; } = new List<WeakReference<ITickable>>();
        private struct FTickSync
        {
            public object Arg;
            public Action<object> OnTick;
        }
        private List<FTickSync> SyncTicks = new List<FTickSync>();
        public void Cleanup()
        {
            Tickables.Clear();
        }
        [Rtti.Meta("")]
        public void AddTickable(ITickable tickable)
        {
            lock(this)
            {
                foreach(var i in Tickables)
                {
                    ITickable cur;
                    if(i.TryGetTarget(out cur))
                    {
                        if (tickable == cur)
                            return;
                    }
                }
                Tickables.Add(new WeakReference<ITickable>(tickable));
                for (int i = 0; i < Tickables.Count; i++)
                {
                    ITickable xx;
                    Tickables[i].TryGetTarget(out xx);
                    if (xx == null)
                    {
                        Tickables.RemoveAt(i);
                        i--;
                    }
                }
                //Tickables.Sort((x, y) =>
                //{
                //    ITickable xx, yy;
                //    x.TryGetTarget(out xx);
                //    y.TryGetTarget(out yy);
                //    return xx.GetTickOrder().CompareTo(yy.GetTickOrder());
                //}); 
            }
        }
        [Rtti.Meta("")]
        public void RemoveTickable(ITickable tickable)
        {
            lock (this)
            {
                foreach (var i in Tickables)
                {
                    ITickable cur;
                    if (i.TryGetTarget(out cur))
                    {
                        if (tickable == cur)
                        {
                            Tickables.Remove(i);
                            return;
                        }
                    }
                }
            }
        }

        public void AddTickSync(Action<object> action, object arg)
        {
            lock (this)
            {
                var t = new FTickSync();
                t.OnTick = action;
                t.Arg = arg;
                SyncTicks.Add(t);
            }
        }
        public void ProcessTicSync()
        {
            lock (this)
            {
                foreach (var i in SyncTicks)
                {
                    i.OnTick(i.Arg);
                }
                SyncTicks.Clear();
            }
        }
    }

    
}



#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS
{
	partial class TtTickableManager
	{
		public unsafe void macross_AddTickable (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, ITickable tickable) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			AddTickable(tickable);
		}
		public unsafe void macross_RemoveTickable (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, ITickable tickable) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			RemoveTickable(tickable);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross