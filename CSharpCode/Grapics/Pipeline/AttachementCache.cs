using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtAttachmentCache
    {
        public void ResetCache(bool bFree = true)
        {
            if (bFree)
            {
                foreach (var i in CachedAttachments)
                {
                    i.Value.FreeBuffer();
                }
            }

            CachedAttachments.Clear();
        }
        public Dictionary<FHashText, UAttachBuffer> CachedAttachments = new Dictionary<FHashText, UAttachBuffer>();
        public UAttachBuffer FindAttachement(TtRenderGraphPin pin)
        {
            return FindAttachement(pin.Attachement.AttachmentName);
        }
        public UAttachBuffer FindAttachement(in FHashText name)
        {
            UAttachBuffer result;
            if (CachedAttachments.TryGetValue(name, out result))
            {
                return result;
            }
            return null;
        }
        public UAttachBuffer GetAttachement(in FHashText name, UAttachmentDesc desc)
        {
            UAttachBuffer result;
            if (CachedAttachments.TryGetValue(name, out result))
            {
                //System.Diagnostics.Debug.Assert(result.BufferDesc.IsMatchSize(in desc.BufferDesc));
                return result;
            }
            else
            {
                //result = new UAttachBuffer();
                //result.CreateBufferViews(in desc.BufferDesc);
                result = UEngine.Instance.GfxDevice.AttachBufferManager.Alloc(desc.BufferDesc);
                CachedAttachments.Add(name, result);
                return result;
            }
        }
        public UAttachBuffer ImportAttachment(TtRenderGraphPin pin)
        {
            UAttachBuffer result;
            if (CachedAttachments.TryGetValue(pin.Attachement.AttachmentName, out result))
                return result;
            result = new UAttachBuffer();
            result.LifeMode = UAttachBuffer.ELifeMode.Imported;
            CachedAttachments.Add(pin.Attachement.AttachmentName, result);
            return result;
        }
        public bool MoveAttachement(in FHashText from, in FHashText to)
        {
            UAttachBuffer attachment;
            if (CachedAttachments.TryGetValue(from, out attachment))
            {
                CachedAttachments.Remove(from);
                CachedAttachments.Add(to, attachment);
                return true;
            }
            return false;
        }
        public void RemoveAttachement(in FHashText name)
        {
            UAttachBuffer result;
            if (CachedAttachments.TryGetValue(name, out result))
            {
                result.FreeBuffer();
                CachedAttachments.Remove(name);
            }
        }
    }
}
