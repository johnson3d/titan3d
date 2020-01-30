using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.RenderPolicy
{
    public partial class CGfxRP_EditorMobile
    {
        public List<Editor.GSnapshotCreator> mSnapshots = new List<Editor.GSnapshotCreator>();
        partial void TickRender_Snapshots()
        {
            lock (mSnapshots)
            {
                for (int i = 0; i < mSnapshots.Count; i++)
                {
                    mSnapshots[i].RenderTick(null);
                    //mSnapshots[i].RenderTick(null);
                }
                mSnapshots.Clear();
            }
        }
    }
}
