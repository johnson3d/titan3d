using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class Touch
    {
        PooledObject<Android.Views.MotionEvent.PointerProperties> mPointerPropPool = new PooledObject<Android.Views.MotionEvent.PointerProperties>();
        float[] mStartX = new float[Input.InputServer.MaxMultiTouchNumber];
        float[] mStartY = new float[Input.InputServer.MaxMultiTouchNumber];
        Input.Device.TouchDevice.TouchInputEventArgs[] mTouchArgs = new Input.Device.TouchDevice.TouchInputEventArgs[Input.InputServer.MaxMultiTouchNumber];

        public void ProcessTouchEvent(Android.Views.MotionEvent e)
        {
            try
            {
                switch(e.ActionMasked)
                {
                    case Android.Views.MotionEventActions.Down:
                    case Android.Views.MotionEventActions.PointerDown:
                        {
                            var ptProp = mPointerPropPool.QueryObjectSync();
                            e.GetPointerProperties(e.ActionIndex, ptProp);
                            if(ptProp.Id >= Input.InputServer.MaxMultiTouchNumber)
                            {
                                mPointerPropPool.ReleaseObject(ptProp);
                                return;
                            }

                            mStartX[ptProp.Id] = e.GetX(e.ActionIndex);
                            mStartY[ptProp.Id] = e.GetY(e.ActionIndex);

                            var touch = mTouchArgs[ptProp.Id];
                            if(touch == null)
                            {
                                touch = new EngineNS.Input.Device.TouchDevice.TouchInputEventArgs();
                                mTouchArgs[ptProp.Id] = touch;
                            }
                            touch.TouchEvent.State = Input.Device.TouchDevice.enTouchState.Down;
                            touch.TouchEvent.PosX = mStartX[ptProp.Id];
                            touch.TouchEvent.PosY = mStartY[ptProp.Id];
                            touch.TouchEvent.FingerIdx = ptProp.Id;
                            touch.TouchEvent.ToolType = (EngineNS.Input.Device.TouchDevice.enToolType)e.GetToolType(e.ActionIndex);
                            EngineNS.CEngine.Instance.InputServerInstance.OnInputEvnet(touch);
                            mPointerPropPool.ReleaseObject(ptProp);
                        }
                        break;
                    case Android.Views.MotionEventActions.Up:
                    case Android.Views.MotionEventActions.PointerUp:
                        {
                            var ptProp = mPointerPropPool.QueryObjectSync();
                            e.GetPointerProperties(e.ActionIndex, ptProp);
                            if(ptProp.Id >= Input.InputServer.MaxMultiTouchNumber)
                            {
                                mPointerPropPool.ReleaseObject(ptProp);
                                return;
                            }

                            mStartX[ptProp.Id] = e.GetX(e.ActionIndex);
                            mStartY[ptProp.Id] = e.GetY(e.ActionIndex);

                            var touch = mTouchArgs[ptProp.Id];
                            if (touch == null)
                            {
                                touch = new EngineNS.Input.Device.TouchDevice.TouchInputEventArgs();
                                mTouchArgs[ptProp.Id] = touch;
                            }
                            touch.TouchEvent.State = Input.Device.TouchDevice.enTouchState.Up;
                            touch.TouchEvent.PosX = mStartX[ptProp.Id];
                            touch.TouchEvent.PosY = mStartY[ptProp.Id];
                            touch.TouchEvent.FingerIdx = ptProp.Id;
                            touch.TouchEvent.ToolType = (EngineNS.Input.Device.TouchDevice.enToolType)e.GetToolType(e.ActionIndex);
                            EngineNS.CEngine.Instance.InputServerInstance.OnInputEvnet(touch);
                            mPointerPropPool.ReleaseObject(ptProp);
                        }
                        break;
                    case Android.Views.MotionEventActions.Move:
                        {
                            var ptProp = mPointerPropPool.QueryObjectSync();
                            for(int acindex = 0; acindex < e.PointerCount; acindex++)
                            {
                                e.GetPointerProperties(acindex, ptProp);
                                if (ptProp.Id >= Input.InputServer.MaxMultiTouchNumber)
                                    continue;

                                float x = e.GetX(acindex);
                                float y = e.GetY(acindex);
                                var deltaX = x - mStartX[ptProp.Id];
                                var deltaY = y - mStartY[ptProp.Id];
                                mStartX[ptProp.Id] = x;
                                mStartY[ptProp.Id] = y;

                                // 去除微小抖动
                                if (System.Math.Abs(deltaX) < 0.5f && System.Math.Abs(deltaY) < 0.5f)
                                    continue;

                                var touch = mTouchArgs[ptProp.Id];
                                if (touch == null)
                                {
                                    touch = new EngineNS.Input.Device.TouchDevice.TouchInputEventArgs();
                                    mTouchArgs[ptProp.Id] = touch;
                                }
                                touch.TouchEvent.State = Input.Device.TouchDevice.enTouchState.Move;
                                touch.TouchEvent.PosX = x;
                                touch.TouchEvent.PosY = y;
                                touch.TouchEvent.DeltaX = deltaX;
                                touch.TouchEvent.DeltaY = deltaY;
                                touch.TouchEvent.FingerIdx = ptProp.Id;
                                touch.TouchEvent.ToolType = (EngineNS.Input.Device.TouchDevice.enToolType)e.GetToolType(acindex);
                                EngineNS.CEngine.Instance.InputServerInstance.OnInputEvnet(touch);
                            }
                            mPointerPropPool.ReleaseObject(ptProp);
                        }
                        break;
                }
            }
            catch(System.Exception)
            {

            }
        }
    }
}
