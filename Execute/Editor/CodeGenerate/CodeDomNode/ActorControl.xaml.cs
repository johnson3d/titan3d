using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CodeDomNode
{
    public sealed partial class ActorControl
    {
        partial void InitConstruction()
        {
            this.InitializeComponent();
            mActorValueLinkHandle = ActorLinkHandle;

            this.Loaded += ActorControl_Loaded;
        }

        private void ActorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var param = CSParam as ActorControlConstructionParams;
            var actor = EngineNS.CEngine.Instance.GameEditorInstance.World.FindActor(param.ActorId);
            if(actor != null && actor.SpecialName != param.ActorName)
            {
                param.ActorName = actor.SpecialName;
                NodeName = param.ActorName;
            }
        }

        private void Button_Search_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var param = CSParam as ActorControlConstructionParams;
            var actor = EngineNS.CEngine.Instance.GameEditorInstance.World.FindActor(param.ActorId);
            if(actor == null)
            {
                EditorCommon.MessageBox.Show($"未在当前场景中找到对象{param.ActorName}");
                return;
            }
            var ei = EngineNS.CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
            ei.SelectWorldActor(actor);
        }
    }
}
