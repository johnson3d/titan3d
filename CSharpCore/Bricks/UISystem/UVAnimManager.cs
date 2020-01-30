using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem
{
    public class UVAnimManager
    {
        public void Cleanup()
        {
            lock(UVAnims)
            {
                foreach(var uvAnim in UVAnims.Values)
                {
                    uvAnim.Cleanup();
                }
                UVAnims.Clear();
            }
        }

        public Dictionary<RName, UVAnim> UVAnims
        {
            get;
        } = new Dictionary<RName, UVAnim>(new RName.EqualityComparer());
        public bool RemoveUVAnimFromDic(RName name)
        {
            lock(UVAnims)
            {
                return UVAnims.Remove(name);
            }
        }

        public async Task<UVAnim> GetUVAnimCloneAsync(CRenderContext rc, RName name)
        {
            var uvAnim = await GetUVAnimAsync(rc, name);
            if (uvAnim == null)
                return null;
            var retVal = new UVAnim();
            retVal.CopyFromTemplate(uvAnim);
            return retVal;
        }
        public async Task<UVAnim> GetUVAnimAsync(CRenderContext rc, RName name)
        {
            if (name == null || name.IsExtension(CEngineDesc.UVAnimExtension) == false)
                return null;
            UVAnim result;
            bool found = false;
            lock(UVAnims)
            {
                if (UVAnims.TryGetValue(name, out result) == false)
                {
                    result = new UVAnim();
                    UVAnims.Add(name, result);
                }
                else
                    found = true;
            }
            if(found)
            {
                var context = await result.AwaitLoad();
                if (context != null && context.Result == null)
                    return null;
                return result;
            }

            if (false == await result.LoadUVAnimAsync(rc, name))
                return null;
            return result;
        }

        public UVAnim CreateUVAnim(RName textureRName, RName matInsRName)
        {
            var result = new UVAnim()
            {
                TextureRName = textureRName,
                MaterialInstanceRName = matInsRName,
            };
            return result;
        }
    }

    public class UVAnimManagerProcesser : CEngineAutoMemberProcessor
    {
        public override async Task<object> CreateObject()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return new UVAnimManager();
        }
        public override void Tick(object obj)
        {
        }
        public override void Cleanup(object obj)
        {
            var uvAnimManager = obj as UVAnimManager;
            uvAnimManager?.Cleanup();
        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        [CEngineAutoMemberAttribute(typeof(EngineNS.UISystem.UVAnimManagerProcesser))]
        public EngineNS.UISystem.UVAnimManager UVAnimManager
        {
            get;
            set;
        }
    }
}