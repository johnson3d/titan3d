using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.MotionMatching
{
    public class TrajectoryPoint
    {
        public TrajectoryPoint()
        {

        }
        public TrajectoryPoint(long time,Vector3 position,Vector3 velocity)
        {
            Time = time;
            Position = position;
            Velocity = velocity;
        }
        public long Time { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
    }
}
