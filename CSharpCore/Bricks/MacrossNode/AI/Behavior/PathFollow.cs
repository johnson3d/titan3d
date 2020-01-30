using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.AI.BehaviorTree;
using EngineNS.GamePlay.Component;

namespace EngineNS.Bricks.MacrossNode.AI.Behavior
{
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class PathFollow : GamePlay.AI.BehaviorTree.Leaf.Action.ActionBehavior
    {
        private GamePlay.Actor.GActor mTarget;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public GamePlay.Actor.GActor Target
        {
            get { return mTarget; }
            set
            {
                mTarget = value;
                mPathFollowing.Target = mTarget.GetComponent<GMovementComponent>();
            }
        }
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
        EngineNS.GamePlay.AI.SteeringBehaviors.KinematicPathFollowing mPathFollowing = new EngineNS.GamePlay.AI.SteeringBehaviors.KinematicPathFollowing();
        bool mCaculatePath = false;
        Vector3 mOldPos = Vector3.Zero;
        GMovementComponent mMovement;
        public override BehaviorStatus Update(long timeElapse, GamePlay.Actor.GCenterData context)
        {
            if (mTickFunc != null)
                mTickFunc(timeElapse, context);
            if (Target == null || Host == null)
                return BehaviorStatus.Failure;
            if(Host.Scene.NavQuery==null)
                return BehaviorStatus.Failure;
            if (mOldPos != Target.Placement.Location)
            {
                //var path = Host.Scene.NavQuery?.FindStraightPath(Host.Placement.Location, Target.Placement.Location);
                //mPathFollowing.Path = path;
                int fullCount = Host.Scene.NavQuery.FindStraightPath(Host.Placement.Location, Target.Placement.Location, mPathFollowing.Path, ref mPathFollowing.PathPoint);
                if (fullCount!=0 && fullCount != mPathFollowing.PathPoint)
                {
                    mPathFollowing.Path = new Vector3[fullCount];
                    Host.Scene.NavQuery.FindStraightPath(Host.Placement.Location, Target.Placement.Location, mPathFollowing.Path, ref mPathFollowing.PathPoint);
                }
                mPathFollowing.IsArrive = false;
                mPathFollowing.CurrentIndex = 0;
                mCaculatePath = true;
                mOldPos = Target.Placement.Location;
            }

            var trans = mPathFollowing.DoSteering();
            if (trans.Force == Vector3.Zero)
            {
                //mMovement.Velocity = trans.Force;
                mMovement.DesireDirection = Vector3.Zero;
                mCaculatePath = false;
                mPathFollowing.IsArrive = true;
                return EngineNS.GamePlay.AI.BehaviorTree.BehaviorStatus.Success;
            }
            else
            {
                mMovement.DesireDirection = trans.Force.NormalizeValue;
                trans.Force.Normalize();
                trans.Force.Y = 0;
                mMovement.Orientation = trans.Force.NormalizeValue;
                return EngineNS.GamePlay.AI.BehaviorTree.BehaviorStatus.Running;
            }
        }
    }
}
