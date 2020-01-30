using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Input
{
    public interface IInputable
    {
        void OnRegisterInput();
        void OnUnRegisterInput();
    }
}
