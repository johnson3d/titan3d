using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeDomNode.Particle;
using EngineNS.Bricks.Particle;

namespace ParticleEditor
{
    public class PropertyChanged
    {
        public PropertyChanged(ParticleMacrossLinkControl linkcontrol)
        {
            LinkControl = linkcontrol;
        }

        public List<IParticleGradient> ParticleGradients = new List<IParticleGradient>();
        Dictionary<ParticleSystemControl, Dictionary<string, object>> ParticleSystemPropertyInfos = new Dictionary<ParticleSystemControl, Dictionary<string, object>>();
        public Dictionary<string, object> AddParticleSystemPropertyInfo(ParticleSystemControl sc)
        {
            Dictionary<string, object> value;
            if (ParticleSystemPropertyInfos.TryGetValue(sc, out value))
                return value;

            value = new Dictionary<string, object>();
            ParticleSystemPropertyInfos.Add(sc, value);
            return value;
        }

        public void CopyParticleSystemProperty(CGfxParticleSystem sys)
        {
            foreach (var p in ParticleSystemPropertyInfos)
            {
                var name = p.Key.Id.ToString().Replace("-", "_");
                if (name.Equals(sys.Name))
                {
                    foreach (var v in p.Value)
                    {
                        var srcProInfo = sys.GetType().GetProperty(v.Key);
                        if (srcProInfo != null)
                        {
                            srcProInfo.SetValue(sys, v.Value);
                        }
                    }
                   
                    break;
                }
            }
        }

        Dictionary<IParticleShape, Dictionary<string, object>> ParticleShapePropertyInfos = new Dictionary<IParticleShape, Dictionary<string, object>>();
        public Dictionary<string, object> AddParticleShapePropertyInfo(IParticleShape ps)
        {
            Dictionary<string, object> value;
            if (ParticleShapePropertyInfos.TryGetValue(ps, out value))
                return value;

            value = new Dictionary<string, object>();
            ParticleShapePropertyInfos.Add(ps, value);
            return value;
        }

        public void CopyParticleShapeProperty(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShape sp)
        {
            //var ParticleShapeNode = sp as CodeGenerateSystem.Base.BaseNodeControl;

            foreach (var p in ParticleShapePropertyInfos)
            {
                var ParticleShapeNode = p.Key as CodeGenerateSystem.Base.BaseNodeControl;
                var name = ParticleShapeNode.Id.ToString().Replace("-", "_");
                if (name.Equals(sp.Name))
                {
                    foreach (var v in p.Value)
                    {
                        var srcProInfo = sp.GetType().GetProperty(v.Key);
                        if (srcProInfo != null)
                        {
                            srcProInfo.SetValue(sp, v.Value);
                        }
                    }

                    break;
                }
            }
        }

        public void Clear()
        {
            ParticleSystemPropertyInfos.Clear();
            ParticleShapePropertyInfos.Clear();
        }

        public ParticleMacrossLinkControl LinkControl;

        public Object Host;

        public void ParticleSystemPropertyChanged(CGfxParticleSystem sys)
        {

            for (int i = 0; i < sys.SubParticleSystems.Count; i++)
            {
                CopyParticleSystemProperty(sys.SubParticleSystems[i]);
       
                for (int j = 0; j < sys.SubParticleSystems[i].TempSubStates.Count; j++)
                {
                    CopyParticleShapeProperty(sys.SubParticleSystems[i].TempSubStates[j]);
                }
            }


            //var srcProInfo = sysvalue.GetType().GetProperty(PropertyName);
            //if (srcProInfo != null)
            //{
            //    srcProInfo.SetValue(sysvalue, NewValue);
            //}
        }

        public void SetHost(Object obj)
        {
            Host = obj;
        }

        public void OnParticleSystemPropertyChanged(string propertyName, object newValue, object oldValue)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            var sc = Host as ParticleSystemControl;
            if (sc == null)
                return;

            Dictionary<string, object> value;
            if (ParticleSystemPropertyInfos.TryGetValue(sc, out value) == false)
            {
                value = AddParticleSystemPropertyInfo(sc);
            }


            Object obj;
            if (value.TryGetValue(propertyName, out obj))
            {
                value[propertyName] = newValue;
            }
            else
            {
                value.Add(propertyName, newValue);
            }

            //LinkControl.ParticleComponent.ParticleSystemPropertyChangedEvent += ParticleSystemPropertyChanged;
            //LinkControl.ParticleComponent.ResetMacross(((GParticleComponent.GParticleComponentInitializer)(LinkControl.ParticleComponent.Initializer)).MacrossName);
            //LinkControl.ParticleComponent.ParticleSystemPropertyChangedEvent -= ParticleSystemPropertyChanged;
        }

        public void OnParticleShapePropertyChanged(string propertyName, object newValue, object oldValue)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            var ps = Host as IParticleShape;
            if (ps == null)
                return;

            Dictionary<string, object> value;
            if (ParticleShapePropertyInfos.TryGetValue(ps, out value) == false)
            {
                value = AddParticleShapePropertyInfo(ps);
            }


            Object obj;
            if (value.TryGetValue(propertyName, out obj))
            {
                value[propertyName] = newValue;
            }
            else
            {
                value.Add(propertyName, newValue);
            }

            //LinkControl.ParticleComponent.ParticleSystemPropertyChangedEvent += ParticleSystemPropertyChanged;
            //LinkControl.ParticleComponent.ResetMacross(((GParticleComponent.GParticleComponentInitializer)(LinkControl.ParticleComponent.Initializer)).MacrossName);
            //LinkControl.ParticleComponent.ParticleSystemPropertyChangedEvent -= ParticleSystemPropertyChanged;
        }

        public void OnPropertyChanged(string propertyName, object newValue, object oldValue)
        {

            if (LinkControl == null)
                return;

            if (LinkControl.ParticleComponent == null)
                return;

            var camera = LinkControl.ParticleComponent.ParticleModifier.ParticleSys.UseCamera; //TODO..

            OnParticleSystemPropertyChanged(propertyName, newValue, oldValue);
            OnParticleShapePropertyChanged(propertyName, newValue, oldValue);
            var initializer = LinkControl.ParticleComponent.Initializer as GParticleComponent.GParticleComponentInitializer;
            if (initializer == null)
                return;
            LinkControl.ParticleComponent.ResetMacross(initializer.MacrossName);
            LinkControl.ParticleComponent.ParticleModifier.ParticleSys.UseCamera = camera;
            var subsyss = LinkControl.ParticleComponent.ParticleModifier.ParticleSys.SubParticleSystems;
            for (int i = 0; i < subsyss.Count; i++)
            {
                subsyss[i].UseCamera = camera;
            }

            for (int i = 0; i < ParticleGradients.Count; i++)
            {
                ParticleGradients[i].SyncValues(LinkControl.ParticleComponent.ParticleModifier.ParticleSys);
            }

            LinkControl.ResetParticleSystemInfos();
            //ParticleGradients.Clear();
        }
    }
}
