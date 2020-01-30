using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GComponentInitializer), "定时器组件", "Timer", "TimerComponent ")]
    
    public class GTimerComponent : GComponent
    {
        public delegate bool FOnTimer(Timer t, GTimerComponent comp);
        public class Timer
        {
            public float Interval = 1.0f;
            public string Name;
            public long mPrevTriggerTime;
            public FOnTimer OnTimer;
            public bool Enable = true;
            public bool OnlyForGame = true;
        }
        internal List<Timer> Timers
        {
            get;
        } = new List<Timer>();
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetTimer(string name, bool enable, float interval, FOnTimer onTimer, bool onlyForGame=true)
        {
            for (int i = 0; i < Timers.Count; i++)
            {
                if (Timers[i].Name == name)
                {
                    Timers[i].Enable = enable;
                    Timers[i].Interval = interval;
                    Timers[i].OnTimer = onTimer;
                    Timers[i].mPrevTriggerTime = CEngine.Instance.EngineTime;
                    Timers[i].OnlyForGame = onlyForGame;
                    return;
                }
            }
            var tm = new Timer();
            tm.Enable = enable;
            tm.Interval = interval;
            tm.OnTimer = onTimer;
            tm.mPrevTriggerTime = CEngine.Instance.EngineTime;
            tm.Name = name;
            tm.OnlyForGame = onlyForGame;
            Timers.Add(tm);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void RemoveTimer(string name)
        {
            for (int i = 0; i < Timers.Count; i++)
            {
                if (Timers[i].Name == name)
                {
                    Timers.RemoveAt(i);
                    return;
                }
            }
        }
        public override void Tick(GPlacementComponent placement)
        {
            var now = CEngine.Instance.EngineTime;
            for (int i = 0; i < Timers.Count; i++)
            {
                if (Timers[i].Enable == false)
                    continue;
                if(CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor &&
                    Timers[i].OnlyForGame)
                {
                    continue;
                }
                if (now - Timers[i].mPrevTriggerTime > Timers[i].Interval * 1000)
                {
                    if(Timers[i].OnTimer!=null)
                        Timers[i].OnTimer(Timers[i], this);

                    Timers[i].mPrevTriggerTime = now;
                }
            }
        }
    }
}

namespace EngineNS.GamePlay.Actor
{
    public partial class GActor
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async System.Threading.Tasks.Task SetTimer(string name, bool enable, float interval, EngineNS.GamePlay.Component.GTimerComponent.FOnTimer onTimer)
        {
            var timerComp = GetComponent<EngineNS.GamePlay.Component.GTimerComponent>();
            if(timerComp==null)
            {
                timerComp = new EngineNS.GamePlay.Component.GTimerComponent();
                timerComp.OnlyForGame = true;
                var initer = new EngineNS.GamePlay.Component.GComponent.GComponentInitializer();
                await timerComp.SetInitializer(CEngine.Instance.RenderContext, this, null, initer);
                this.AddComponent(timerComp);
            }
            timerComp.SetTimer(name, enable, interval, onTimer, false);
        }
    }
}