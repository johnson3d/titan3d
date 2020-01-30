using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCClassEditor
{
    public class YCAttributeBase : Attribute
    {
        public YCAttributeBase()
        {
            this.hasInput = false;
        }
        public YCAttributeBase(string _input)
        {
            this.hasInput = true;
            this.input = _input;
        }
        public bool hasInput { get; set; }
        public string input { get; set; }
    }
}
