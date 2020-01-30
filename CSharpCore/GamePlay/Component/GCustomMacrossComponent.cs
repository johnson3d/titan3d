using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GCustomMacrossComponentInitializer), "用户宏图组件", "Macross", "CustomMacrossComponent")]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class GCustomMacrossComponent : GComponent
    {
        [Rtti.MetaClass]
        public class GCustomMacrossComponentInitializer : GComponentInitializer
        {


        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            return await base.SetInitializer(rc, host, hostContainer, v);
        }
    }
}
