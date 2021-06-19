using System;
using System.Collections.Generic;
using SDL2;

namespace EngineNS
{
    public interface ITickable
    {
        void TickLogic(int ellapse);
        void TickRender(int ellapse);
        void TickSync(int ellapse);
    }

    public partial class UTickableManager
    {
        public List<ITickable> Tickables { get; } = new List<ITickable>();
        public void Cleanup()
        {
            Tickables.Clear();
        }
        [Rtti.Meta]
        public void AddTickable(ITickable tickable)
        {
            lock(this)
            {
                if (Tickables.Contains(tickable) == false)
                    Tickables.Add(tickable);
            }
        }
        [Rtti.Meta]
        public void RemoveTickable(ITickable tickable)
        {
            lock (this)
            {
                Tickables.Remove(tickable);
            }
        }
    }

    
}
