using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtHitProxy
    {
        public enum EHitproxyType
        {
            None = 0,
            Root = 1,
            FollowParent = 2,
        }
        public uint ProxyId;
        public WeakReference<IProxiable> ProxyObject;
        public bool IsPoxyObject(IProxiable obj)
        {
            IProxiable refObj;
            if (ProxyObject.TryGetTarget(out refObj))
            {
                return refObj == obj;
            }
            return false;
        }
        public Vector4 ConvertHitProxyIdToVector4()
        {
            return new Vector4(((ProxyId >> 24) & 0x000000ff) / 255.0f, ((ProxyId >> 16) & 0x000000ff) / 255.0f, ((ProxyId >> 8) & 0x000000ff) / 255.0f,
                ((ProxyId >> 0) & 0x000000ff) / 255.0f);
        }
        public static UInt32 ConvertCpuTexColorToHitProxyId(Byte4 PixelColor)
        {
            return ((UInt32)PixelColor.R << 24 | (UInt32)PixelColor.G << 16 | (UInt32)PixelColor.B << 8 | (UInt32)PixelColor.A << 0);
        }
    }
    public interface IProxiable
    {
        TtHitProxy HitProxy { get; set; }
        TtHitProxy.EHitproxyType HitproxyType { get; set; }
        bool Selected { get; set; }
        void OnHitProxyChanged();
        void GetHitProxyDrawMesh(List<Graphics.Mesh.TtMesh> meshes);
    }
    public class TtHitproxyManager
    {
        private Dictionary<UInt32, TtHitProxy> Proxies
        {
            get;
        } = new Dictionary<UInt32, TtHitProxy>();
        private UInt32 HitProxyAllocatorId = 0;
        public void Cleanup()
        {

        }
        public TtHitProxy MapProxy(IProxiable proxiable)
        {
            lock (Proxies)
            {
                if (proxiable.HitProxy != null)
                    return proxiable.HitProxy;

                if (HitProxyAllocatorId == uint.MaxValue)
                {
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, "HitProxyAllocatorId == uint.MaxValue");
                    System.Diagnostics.Debug.Assert(false);
                    HitProxyAllocatorId = 0;
                    foreach (var i in Proxies)
                    {
                        IProxiable obj;                        
                        if(i.Value.ProxyObject.TryGetTarget(out obj))
                        {
                            obj.HitProxy.ProxyId = ++HitProxyAllocatorId;
                            obj.OnHitProxyChanged();
                        }
                    }
                }

                var result = new TtHitProxy();
                result.ProxyId = ++HitProxyAllocatorId;
                result.ProxyObject = new WeakReference<IProxiable>(proxiable);
                proxiable.HitProxy = result;
                proxiable.OnHitProxyChanged();
                //proxiable.HitproxyType = proxiable.HitproxyType;

                Proxies.Add(result.ProxyId, result);

                return result;
            }
        }
        public void UnmapProxy(UInt32 id)
        {
            lock (Proxies)
            {
                TtHitProxy proxy;
                if (Proxies.TryGetValue(id, out proxy))
                {
                    IProxiable result;
                    if (proxy.ProxyObject.TryGetTarget(out result))
                    {
                        result.HitProxy = null;
                    }
                    proxy.ProxyId = 0;
                    Proxies.Remove(id);
                }
            }
        }
        public IProxiable FindProxy(UInt32 id)
        {
            lock (Proxies)
            {
                TtHitProxy proxy;
                if (Proxies.TryGetValue(id, out proxy))
                {
                    IProxiable result;
                    if (proxy.ProxyObject.TryGetTarget(out result))
                    {
                        return result;
                    }
                    else
                    {
                        Proxies.Remove(id);
                    }
                }
                return null;
            }
        }
        public void UnmapProxy(IProxiable proxy)
        {
            if (proxy.HitProxy == null)
                return;

            UnmapProxy(proxy.HitProxy.ProxyId);

            proxy.HitProxy = null;
        }
    }
}
