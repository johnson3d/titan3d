using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class UActionChain
    {
        public class UModifyObject
        {
            public object Target;
            public List<Rtti.UClassMeta.UMetaFieldValue> RestoreData;
            public List<Rtti.UClassMeta.UMetaFieldValue> RedoData;
        }
        public List<UModifyObject> ModifyObjects { get; } = new List<UModifyObject>();
        public virtual bool PushObject(object obj)
        {
            var tmp = new UModifyObject();
            
            tmp.Target = obj;
            var meta = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(obj.GetType().FullName);
            tmp.RestoreData = new List<Rtti.UClassMeta.UMetaFieldValue>();
            meta.CopyObjectMetaField(tmp.RestoreData, tmp.Target);
            
            ModifyObjects.Add(tmp);
            return true;
        }
        public virtual void EndAction()
        {
            foreach (var i in ModifyObjects)
            {
                var meta = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(i.Target.GetType().FullName);
                i.RedoData = new List<Rtti.UClassMeta.UMetaFieldValue>();
                meta.CopyObjectMetaField(i.RedoData, i.Target);
            }
        }
        public virtual void Undo()
        {
            foreach(var i in ModifyObjects)
            {
                var meta = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(i.Target.GetType().FullName);
                meta.SetObjectMetaField(i.Target, i.RestoreData);
            }
        }
        public virtual void Redo()
        {
            foreach (var i in ModifyObjects)
            {
                if (i.RedoData != null)
                {
                    var meta = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(i.Target.GetType().FullName);
                    meta.SetObjectMetaField(i.Target, i.RedoData);
                }
            }
        }
    }
    public class UActionChainManager
    {
        int mMaxStep;
        int mCurrentStep;
        List<UActionChain> Actions;
        public UActionChainManager(int maxStep = 20)
        {
            mMaxStep = maxStep;
            Actions = new List<UActionChain>(maxStep);
            mCurrentStep = -1;
        }
        public UActionChain GetCurrentAction()
        {
            if (Actions.Count == 0)
                return null;
            if (mCurrentStep < 0)
                return null;
            return Actions[mCurrentStep];
        }
        public void PushAction(UActionChain action)
        {
            if (Actions.Count - mCurrentStep - 1 > 0)
            {
                Actions.RemoveRange(mCurrentStep + 1, Actions.Count - mCurrentStep - 1);
            }
            if (Actions.Count >= mMaxStep)
            {
                Actions.RemoveAt(0);
            }
            mCurrentStep = Actions.Count;
            Actions.Add(action);
        }
        public void Undo()
        {
            if (Actions.Count == 0)
                return;
            if (mCurrentStep < 0)
                return;
            Actions[mCurrentStep].Undo();
            mCurrentStep--;
        }
        public void Redo()
        {
            if (Actions.Count == 0)
                return;
            if (mCurrentStep >= Actions.Count - 1)
            {
                return;
            }
            mCurrentStep++;
            Actions[mCurrentStep].Redo();
        }
    }
}
