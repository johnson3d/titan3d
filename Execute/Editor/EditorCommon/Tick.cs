using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon
{
    public interface ITickObject
    {
        void Tick(Int64 elapsedMillisecond);
    }

    public class TickManager : EngineNS.Editor.IEditorInstanceObject
    {
        public static TickManager Instance
        {
            get
            {
                var name = typeof(TickManager).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = new TickManager();
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }

        public void FinalCleanup()
        {
        }

        List<ITickObject> mTickItemList = new List<ITickObject>();
        public void AddTickNode(ITickObject node)
        {
            if (!mTickItemList.Contains(node))
                mTickItemList.Add(node);
        }
        public void RemoveTickNode(ITickObject node)
        {
            mTickItemList.Remove(node);
        }
        public void Tick(Int64 elapsedMillisecond)
        {
            for (int i = 0; i < mTickItemList.Count; i++)
            {
                mTickItemList[i].Tick(elapsedMillisecond);
            }
        }
    }
}
