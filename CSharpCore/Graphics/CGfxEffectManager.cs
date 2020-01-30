using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics
{
    public partial class CGfxEffect : Thread.Async.IWaitLoader
    {
        public CGfxEffect()
        {

        }
        public CGfxEffect(CGfxEffectDesc desc)
        {
            mDesc = desc;
        }
        enum EEffectState
        {
            Invalid,
            Creating,
            Valid,
        }
        EEffectState State = EEffectState.Invalid;
        public bool IsValid
        {
            get { return State == EEffectState.Valid; }
        }
        public UInt32 Version = 1;
        public delegate void FOnEffectCreated(bool successed);
        private List<FOnEffectCreated> mOnCreatedList = new List<FOnEffectCreated>();
        public void OnWaitLoaderSetFinished(int waitCount)
        {
            Version++;
            this.State = EEffectState.Valid;
        }
        public bool PreUse(FOnEffectCreated after = null)
        {
            if (State == EEffectState.Valid)
            {
                if (after != null)
                    after(true);
                return true;
            }
            else if (State == EEffectState.Creating)
            {
                if(after!=null)
                    mOnCreatedList.Add(after);
            }
            else if (State == EEffectState.Invalid)
            {
                State = EEffectState.Creating;
                if (after != null)
                    mOnCreatedList.Add(after);

                var un = _CreateEffect();
            }
            return false;
        }
        private async System.Threading.Tasks.Task _CreateEffect()
        {
            bool IsValid = await CreateEffectAsync(CEngine.Instance.RenderContext, this.Desc, CIPlatform.Instance.PlatformType);
            if (IsValid == false)
            {
                State = EEffectState.Invalid;
            }
            for (int i = 0; i < mOnCreatedList.Count; i++)
            {
                mOnCreatedList[i](IsValid);
            }
            mOnCreatedList.Clear();
        }
        internal void Editor_RefreshEffect(CRenderContext rc, bool saveShader)
        {
            string savedInfo = "";
            var strs = Desc.MdfQueueShaderPatch.GetShaderDefines();
            if (strs != null)
            {
                foreach (var i in strs)
                {
                    savedInfo += i;
                }
            }
            strs = Desc.MdfQueueShaderPatch.GetShaderIncludes();
            if (strs != null)
            {
                foreach (var i in strs)
                {
                    savedInfo += i;
                }
            }

            savedInfo += Desc.EnvShaderPatch.GetShaderDefinesString();

            if (State == EEffectState.Creating)
            {
                //PreUse((successed) =>
                //{
                //    State = EEffectState.Invalid;

                //    CreateEffectByD11Editor(rc, Desc, saveShader);
                //    //if(Desc.GetHash64()!= savedHash)
                //    {
                //        System.Diagnostics.Debug.Assert(false);
                //    }
                //});
                //System.Diagnostics.Debug.Assert(false);
                return;
            }

            CreateEffectByD11Editor(rc, Desc, CIPlatform.Instance.PlatformType, saveShader);
            Desc.UpdateHash64(true);

            string newInfo = "";
            strs = Desc.MdfQueueShaderPatch.GetShaderDefines();
            if (strs != null)
            {
                foreach (var i in strs)
                {
                    newInfo += i;
                }
            }
            strs = Desc.MdfQueueShaderPatch.GetShaderIncludes();
            if (strs != null)
            {
                foreach (var i in strs)
                {
                    newInfo += i;
                }
            }

            newInfo += Desc.EnvShaderPatch.GetShaderDefinesString();

            if (newInfo != savedInfo)
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }
    }

    public class CGfxEffectManager
    {
        public Dictionary<Hash64, CGfxEffect> Effects
        {
            get;
        } = new Dictionary<Hash64, CGfxEffect>(new Hash64.EqualityComparer());
        protected CGfxEffect mDefaultEffect;
        public CGfxEffect DefaultEffect
        {
            get { return mDefaultEffect; }
        }

        public void UnRegEffect_Editor(RName matName)
        {
            lock (Effects)
            {
                List<Hash64> removeList = new List<Hash64>();
                foreach (var item in Effects)
                {
                    if (item.Value.Desc.MtlShaderPatch.Name == matName)
                    {
                        removeList.Add(item.Key);

                        var fileName = EngineNS.CEngine.Instance.FileManager.ProjectContent + "Cache/ShaderInfo/" + item.Value.Desc.GetHash64() + ".xml";
                        EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);
                        fileName = EngineNS.CEngine.Instance.FileManager.ProjectContent + "Cache/Shader/" + item.Value.Desc.GetHash64() + ".shader";
                        EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);
                    }
                }
                foreach (var key in removeList)
                {
                    Effects.Remove(key);
                }
            }
        }
        public void RegEffect(Hash64 hash, CGfxEffect effect)
        {
            lock (Effects)
            {
                if(effect.Desc.GetHash64()!=hash)
                {
                    hash = effect.Desc.GetHash64();
                }
                Effects[hash] = effect;
            }
        }
        public CGfxEffect TryGetEffect(ref Hash64 hash)
        {
            lock (Effects)
            {
                CGfxEffect result;
                if (Effects.TryGetValue(hash, out result) == true)
                {
                    if (hash != result.Desc.GetHash64())
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    return result;
                }
            }
            return null;
        }
        public CGfxEffect GetEffect(CRenderContext rc, CGfxEffectDesc desc)
        {
            lock (Effects)
            {
                CGfxEffect result;
                var saved = desc.String;
                var hash = desc.GetHash64();
                if (Effects.TryGetValue(hash, out result) == true)
                {
                    if (hash != result.Desc.GetHash64())
                    {
                        Effects.Remove(hash);
                        hash = result.Desc.GetHash64();
                        Effects[hash] = result;
                        //System.Diagnostics.Debug.Assert(false);
                    }
                    return result;
                }
                else
                {
                    result = new CGfxEffect(desc);
                    result.PreUse();
                    Effects.Add(desc.GetHash64(), result);
                    return result;
                }
            }
        }
        public async System.Threading.Tasks.Task<bool> Init(CRenderContext rc)
        {
            if (CEngine.Instance.MaterialManager.DefaultMaterial == null ||
                CEngine.Instance.ShadingEnvManager.DefaultShadingEnv == null)
                return false;
            var desc = CGfxEffectDesc.CreateDesc(CEngine.Instance.MaterialManager.DefaultMaterial,
                    new Mesh.CGfxMdfQueue(),
                    CEngine.Instance.ShadingEnvManager.DefaultShadingEnv.EnvCode);
            mDefaultEffect = GetEffect(rc, desc);
            await mDefaultEffect.AwaitLoad();

            if (mDefaultEffect.CacheData.CBID_View == UInt32.MaxValue)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "Shader", $"DefaultEffect({desc.EnvShaderPatch.ShaderName}) CBID_View is invalid");
                return false;
            }
            if (mDefaultEffect.CacheData.CBID_Camera == UInt32.MaxValue)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "Shader", $"DefaultEffect({desc.EnvShaderPatch.ShaderName}) CBID_Camera is invalid");
                return false;
            }
            return true;
        }

        public void RefreshEffects(CRenderContext rc, RName material)
        {
            try
            {
                using (var i = Effects.Values.GetEnumerator())
                {
                    while (i.MoveNext())
                    {
                        if (i.Current.Desc.MtlShaderPatch.Name == material)
                        {
                            i.Current.Editor_RefreshEffect(rc, true);
                        }
                    }
                }
            }
            catch
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Effect", $"RefreshEffects {material} exception");
            }
        }
    }
}
