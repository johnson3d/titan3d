using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode.Particle
{
    public interface IParticleShape
    {
        CreateObject GetCreateObject();

        string GetClassName();
    }
}
