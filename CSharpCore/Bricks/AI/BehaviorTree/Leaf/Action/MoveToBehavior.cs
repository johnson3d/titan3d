using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AI.BehaviorTree.Leaf.Action
{
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class MoveToBehavior : Bricks.AI.BehaviorTree.Leaf.Action.ActionBehavior
    {
        public Vector3 Target { get; set; } = Vector3.Zero;
        public Func<Vector3> TargetPositionEvaluateFunc { get; set; } = null;
        GamePlay.Actor.GActor mHost;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public GamePlay.Actor.GActor Host
        {
            get { return mHost; }
            set
            {
                mHost = value;
                mMovement = mHost.GetComponent<GMovementComponent>();
                mPathFollowing.Character = mMovement;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float ArriveDistance
        {
            get { return mPathFollowing.ArriveDistance; }
            set
            {
                mPathFollowing.ArriveDistance = value;
            }
        }
        public Func<float> ArriveDistanceEvaluateFunc { get; set; } = null;
        Bricks.AI.SteeringBehaviors.KinematicPathFollowing mPathFollowing = new Bricks.AI.SteeringBehaviors.KinematicPathFollowing();
        //bool mCaculatePath = false;
        Vector3 mOldPos = Vector3.Zero;
        GMovementComponent mMovement;
        public override BehaviorStatus Update(long timeElapse, GamePlay.Actor.GCenterData context)
        {
            if (TargetPositionEvaluateFunc != null)
                Target = TargetPositionEvaluateFunc.Invoke();
            if (ArriveDistanceEvaluateFunc != null)
            {
                ArriveDistance = ArriveDistanceEvaluateFunc.Invoke();
            }
            if (mTickFunc != null)
                mTickFunc(timeElapse, context);
            if (Host == null)
                return BehaviorStatus.Failure;
            if (Host.Scene.NavQuery == null)
            {
                mPathFollowing.PathPoint = 1;
                mPathFollowing.Path = new Vector3[] { Target };
                mPathFollowing.IsArrive = false;
                mPathFollowing.CurrentIndex = 0;
               // mCaculatePath = true;
                mOldPos = Target;
            }
            if (mOldPos != Target)
            {
                int fullCount = Host.Scene.NavQuery.FindStraightPath(Host.Placement.Location, Target, mPathFollowing.Path, ref mPathFollowing.PathPoint);
                if (fullCount != 0 && fullCount != mPathFollowing.PathPoint)
                {
                    mPathFollowing.Path = new Vector3[fullCount];
                    Host.Scene.NavQuery.FindStraightPath(Host.Placement.Location, Target, mPathFollowing.Path, ref mPathFollowing.PathPoint);
                }
                mPathFollowing.IsArrive = false;
                mPathFollowing.CurrentIndex = 0;
                //mCaculatePath = true;
                mOldPos = Target;
            }

            var trans = mPathFollowing.DoSteering();
            if (trans.Force == Vector3.Zero)
            {
                //mMovement.Velocity = trans.Force;
                mMovement.DesireDirection = Vector3.Zero;
                //mCaculatePath = false;
                mPathFollowing.IsArrive = true;
                return Bricks.AI.BehaviorTree.BehaviorStatus.Success;
            }
            else
            {
                mMovement.DesireDirection = trans.Force.NormalizeValue;
                mMovement.DesireOrientation = mMovement.DesireDirection;
                trans.Force.Normalize();
                trans.Force.Y = 0;
                //mMovement.Orientation = trans.Force.NormalizeValue;
                return Bricks.AI.BehaviorTree.BehaviorStatus.Running;
            }
        }
    }
}
