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
        public Dictionary<FHashText, TtAttachBuffer> CachedAttachments = new Dictionary<FHashText, TtAttachBuffer>();
        public TtAttachBuffer FindAttachement(TtRenderGraphPin pin)
        {
            return FindAttachement(pin.Attachement.AttachmentName);
        }
        public TtAttachBuffer FindAttachement(in FHashText name)
        {
            TtAttachBuffer result;
            if (CachedAttachments.TryGetValue(name, out result))
            {
                return result;
            }
            return null;
        }
        public TtAttachBuffer GetAttachement(TtRenderGraphPin pin)
        {
            return GetAttachement(pin.Attachement.AttachmentName, pin.Attachement);
        }
        public TtAttachBuffer GetAttachement(in FHashText name, TtAttachmentDesc desc)
        {
            TtAttachBuffer result;
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
        public TtAttachBuffer ImportAttachment(TtRenderGraphPin pin)
        {
            TtAttachBuffer result;
            if (CachedAttachments.TryGetValue(pin.Attachement.AttachmentName, out result))
                return result;
            result = new TtAttachBuffer();
            result.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            CachedAttachments.Add(pin.Attachement.AttachmentName, result);
            return result;
        }
        public bool MoveAttachement(in FHashText from, in FHashText to)
        {
            TtAttachBuffer attachment;
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
            TtAttachBuffer result;
            if (CachedAttachments.TryGetValue(name, out result))
            {
                result.FreeBuffer();
                CachedAttachments.Remove(name);
            }
        }
    }
}
