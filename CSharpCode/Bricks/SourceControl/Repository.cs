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
