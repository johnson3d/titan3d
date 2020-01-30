using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerateSystem.Controls
{
    public sealed partial class CommentNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        string mOldComment = "";
        string mComment = "";
        partial void SetComment_WPF(string cmt);
        public override string Comment
        {
            get { return mComment; }
            set
            {
                mComment = value;
                SetComment_WPF(mComment);
                IsDirty = true;
                OnPropertyChanged("Comment");
            }
        }

        partial void InitConstruction();
        public CommentNode(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
        }

        partial void Save_WPF(EngineNS.IO.XndNode xndNode);
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            Save_WPF(xndNode);
            base.Save(xndNode, newGuid);
        }
        partial void Load_WPF(EngineNS.IO.XndNode xndNode);
        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            await base.Load(xndNode);
            Load_WPF(xndNode);
        }
    }
}
