using EngineNS.Macross;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public class TtSharedData
    {
    }

    //for example: UPhysxNode.InitializeNode: find UActor from parents, UActor.CenterData.GetOrNewSharedData<UPhysxSharedData>().Friction = 1;
    //developer can save CenterData.GetOrNewSharedData<UPhysxSharedData>() in UPhysxNode for access shared data quickly
    //if another node expects to access this data, UActor.CenterData.GetOrNewSharedData<UPhysxSharedData>() will return it.
    public class TtPhysxSharedData : TtSharedData
    {
        public float Friction;
    }


    public class TtCenterData
    {
        public Dictionary<Rtti.TtTypeDesc, TtSharedData> SharedDatas { get; } = new Dictionary<Rtti.TtTypeDesc, TtSharedData>();
        public bool HasSharedData(Rtti.TtTypeDesc type)
        {
            lock (SharedDatas)
            {
                return SharedDatas.ContainsKey(type);
            }
        }
        public bool HasSharedData<T>() where T : TtSharedData
        {
            lock (SharedDatas)
            {
                return SharedDatas.ContainsKey(TtTypeDescGetter<T>.TypeDesc);
            }
        }
        public TtSharedData GetOrNewSharedData(Rtti.TtTypeDesc type)
        {
            lock(SharedDatas)
            {
                TtSharedData data;
                if (SharedDatas.TryGetValue(type, out data))
                    return data;
                data = Rtti.TtTypeDescManager.CreateInstance(type) as TtSharedData;
                if (data == null)
                    return null;
                SharedDatas.Add(type, data);
                return data;
            }
        }
        public T GetOrNewSharedData<T>() where T : TtSharedData
        {
            return GetOrNewSharedData(TtTypeDescGetter<T>.TypeDesc) as T;
        }
    }
}
