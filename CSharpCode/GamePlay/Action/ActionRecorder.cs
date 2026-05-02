using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Action
{
    public class UActionRecorder
    {
        public int MaxNumOfRecords = 10;
        public List<UAction> Records { get; } = new List<UAction>();
        public bool IsPlayRecord { get; set; } = false;

        protected UAction mCurrentAction;
        public virtual UAction CurrentAction
        {
            get => mCurrentAction;
            set => mCurrentAction = value;
        }

        public UAction NewAction()
        {
            var result = new UAction();

            result.Recorder = this;

            CurrentAction = result;
            return result;
        }
        public void CloseAction()
        {
            if (CurStep == Records.Count && CurStep >= MaxNumOfRecords)
            {
                Records.RemoveAt(0);
                Records.Add(CurrentAction);
                CurStep = Records.Count;
                CurrentAction = null;
                return;
            }
            if (CurrentAction != null)
            {
                if (CurStep < Records.Count)
                {
                    Records.RemoveRange(CurStep, Records.Count - CurStep);
                }
                Records.Add(CurrentAction);
                CurStep = Records.Count;
                CurrentAction = null;
            }
        }
        public void ClearRecords()
        {
            Records.Clear();
        }
        public virtual bool IsRecordAction(object host, string name, object oldValue, object newValue)
        {//这里特殊处理诸如坐标轴移动中间的属性修改不需要记录的问题
            return true;
        }
        public virtual void OnChanged(UAction.UPropertyModifier modifier)
        {

        }

        protected int CurStep = 0;
        public bool Undo()
        {
            if (CurStep == 0)
                return false;
            CurStep--;
            IsPlayRecord = true;
            Records[CurStep].Undo();
            IsPlayRecord = false;
            return true;
        }
        public bool Redo()
        {
            if (CurStep >= Records.Count)
                return false;
            IsPlayRecord = true;
            Records[CurStep].Redo();
            IsPlayRecord = false;
            CurStep++;
            return true;
        }
    }
}
