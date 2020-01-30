using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macross.Data
{
    public class MacrossData
    {
        public EngineNS.ECSType CSType = EngineNS.ECSType.Common;

        public List<MacrossVariable> Variables
        {
            get;
            private set;
        } = new List<MacrossVariable>();
    }
}
