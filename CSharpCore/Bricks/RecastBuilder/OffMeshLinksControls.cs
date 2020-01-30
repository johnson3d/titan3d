using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RecastBuilder
{
    public class OffMeshLinksControls
    {
        public delegate void DelefateOffMeshLinksControlsChange(bool value);
        public event DelefateOffMeshLinksControlsChange OffMeshLinksControlsChange;

        public OffMeshLinksControls()
        {
        }

        bool mEnable = false;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Create_Off_Mesh_Links
        {
            get
            {
                return mEnable;
            }

            set
            {
                mEnable = value;
                if (OffMeshLinksControlsChange != null)
                    OffMeshLinksControlsChange(value);
            }
        }

        public bool Bidirectional
        {
            get;
            set;
        }


    }

}
