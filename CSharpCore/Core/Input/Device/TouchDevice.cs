using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Input.Device
{
    public class TouchDevice : InputDevice
    {
        public enum enToolType
        {
            UNKNOWN = 0,
            FINGER = 1,
            STYLUS = 2,
            MOUSE = 3,
            ERASER = 4,
        }

        public enum enTouchState
        {
            Down = 0,
            Up,
            Move,
            // todo: 手势操作
        }
        public struct TouchEventArgs : IDeviceEventArgs
        {
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public enTouchState State;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float PosX;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float PosY;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public int FingerIdx;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public enToolType ToolType;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float DeltaX
            {
                get;
                set;
            }// 从按下开始到现在移动的距离
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float DeltaY
            {
                get;
                set;
            }// 从按下开始到现在移动的距离
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float PointX => PosX;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float PointY => PosY;
        }
        public class TouchInputEventArgs : DeviceInputEventArgs
        {
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public static TouchInputEventArgs CreateTouchInputEventArgs(enTouchState State,
                float PosX,
                float PosY,
                int FingerIdx,
                float DeltaX,
                float DeltaY)
            {
                //以后要改成pool模式
                var result = new TouchInputEventArgs();
                result.TouchEvent.State = State;
                result.TouchEvent.PosX = PosX;
                result.TouchEvent.PosY = PosY;
                result.TouchEvent.FingerIdx = FingerIdx;
                result.TouchEvent.DeltaX = DeltaX;
                result.TouchEvent.DeltaY = DeltaY;
                return result;
            }

            static Vector2 ResultUV;

            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public static TouchInputEventArgs CreateTouchInputEventArgsFromeScreenXY(
                GamePlay.Actor.GActor showActor,
                float showWidth,
                float showHeight,
                float hitLength,
                enTouchState State,
                int x,
                int y,
                int FingerIdx,
                float DeltaX,
                float DeltaY)
            {
                if (showActor.Scene == null)
                    return null;
                if (CEngine.Instance.GameInstance == null)
                    return null;
                var hitResult = new GamePlay.SceneGraph.VHitResult(Bricks.PhysicsCore.PhyHitFlag.eDEFAULT );
                if (showActor.Scene.PickActor(out hitResult, CEngine.Instance.GameInstance.GameCamera, x, y, hitLength) != showActor)
                    return null;
                var triMeshComp = showActor.GetComponent<Bricks.PhysicsCore.CollisionComponent.GPhysicsMeshCollision>();
                if (triMeshComp == null)
                    return null;
                var triMesh = triMeshComp.HostShape.TriGeom;
                if (triMesh == null)
                    return null;
                if (hitResult.FaceId == -1)
                    return null;

                if (triMeshComp.HostShape == null)
                    return null;

                int faceid = triMeshComp.HostShape.GetTrianglesRemap(hitResult.FaceId);
                if (faceid == -1)
                    return null;

                triMesh.GetUV(faceid, hitResult.U, hitResult.V, out ResultUV);
                var pos = triMesh.GetPos(faceid, hitResult.U, hitResult.V);
                pos = Vector3.TransformCoordinate(pos, showActor.Placement.DrawTransform);
                //以后要改成pool模式
                var result = new TouchInputEventArgs();
                result.TouchEvent.State = State;
                result.TouchEvent.PosX = ResultUV.X * showWidth;
                result.TouchEvent.PosY = ResultUV.Y * showHeight;
                result.TouchEvent.FingerIdx = FingerIdx;
                result.TouchEvent.DeltaX = DeltaX;
                result.TouchEvent.DeltaY = DeltaY;
                return result;
            }
            public TouchEventArgs TouchEvent;
            public TouchInputEventArgs()
            {
                DeviceType = DeviceType.Touch;
            }
        }

        public TouchDevice()
        {
            mName = "Touch";
            mType = DeviceType.Touch;
            mID = Guid.NewGuid();
        }

        public override void OnInputEvent(DeviceInputEventArgs e)
        {
            if(e.DeviceType != Type)
            {
                var arg = e as TouchInputEventArgs;
                var eventArg = arg.TouchEvent;
                switch(eventArg.State)
                {
                    case enTouchState.Down:
                        {
                            TriggerActionMapping(eventArg.State);
                        }
                        break;
                    case enTouchState.Up:
                        {
                            TriggerActionMapping(eventArg.State);
                        }
                        break;
                    case enTouchState.Move:
                        break;
                }
            }
        }

        public void TriggerActionMapping(enTouchState state)
        {
            for(int i=0; i<InputBindings.Count; ++i)
            {
                var binding = InputBindings[i];
                for(int j=0; j < binding.Mappings.Count; ++j)
                {
                    var mapping = binding.Mappings[j];
                    if (mapping is InputValueMapping)
                    {
                        var valueMapping = mapping as InputValueMapping;
                        if (mapping.MappingType == InputMappingType.Action && state == (enTouchState)mapping.Value)
                        {
                            for (int k = 0; k < valueMapping.Funtions.Count; ++k)
                            {
                                valueMapping.Funtions[k]?.Invoke(mapping.Value);
                            }
                        }
                    }
                }
            }
        }
    }
}
