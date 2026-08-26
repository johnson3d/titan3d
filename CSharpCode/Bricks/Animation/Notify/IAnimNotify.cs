using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Notify
{
    public interface IAnimNotify : IO.ISerializer
    {
        public bool CanTrigger(Int64 beforeTime, Int64 afterTime);
        public void Trigger(long beforeTime, long afterTime);
        public Guid ID { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 通知的起始时刻(毫秒)。瞬时通知即为其触发时刻。
        /// 给时间轴编辑器统一读写位置用。
        /// </summary>
        public Int64 BeginTime { get; set; }
        /// <summary>
        /// 通知的结束时刻(毫秒)。瞬时通知等于BeginTime且忽略写入。
        /// </summary>
        public Int64 EndTime { get; set; }
    }
}
