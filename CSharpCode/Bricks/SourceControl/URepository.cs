using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.SourceControl
{
    public class URepository
    {
        public string CurrentBranch { get; }
        public virtual void GetBranches(List<string> branches)
        {

        }
        public virtual void SetCurrentBranch(string name)
        {

        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        System.Type SourceControlType = typeof(Bricks.SourceControl.Git.UGitRepository);
        Bricks.SourceControl.URepository mSourceControl;
        public Bricks.SourceControl.URepository SourceControl
        {
            get
            {
                if (mSourceControl == null)
                {
                    mSourceControl = Rtti.UTypeDescManager.CreateInstance(SourceControlType) as Bricks.SourceControl.URepository;
                }
                return mSourceControl;
            }
        }
    }
}