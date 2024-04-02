using System;
using System.Collections.Generic;
using EngineNS.UI.Canvas;

namespace EngineNS.UI.Controls.Containers
{
    public partial class TtPopup : TtContainer
    {
        protected TtUIHost mUIHost;
        protected TtUIHost mParentHost;
        public TtPopup(TtUIHost host)
        {
            mUIHost = host;
        }
        public TtUIHost UIHost
        {
            get => mUIHost;
        }
        public void OpenPopup(TtUIHost parentHost)
        {
            mParentHost = parentHost;
            mParentHost.AddPopupUIHost(mUIHost);
        }
        public void ClosePopup()
        {
            mParentHost = null;
            mParentHost.RemovePopupUIHost(mUIHost);
        }
    }
}
