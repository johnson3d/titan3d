using EngineNS.Macross;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public class USharedData
    {
    }

    //for example: UPhysxNode.InitializeNode: find UActor from parents, UActor.CenterData.GetOrNewSharedData<UPhysxSharedData>().Friction = 1;
    //developer can save CenterData.GetOrNewSharedData<UPhysxSharedData>() in UPhysxNode for access shared data quickly
    //if another node expects to access this data, UActor.CenterData.GetOrNewSharedData<UPhysxSharedData>() will return it.
    public class UPhysxSharedData : USharedData
    {
        public float Friction;
    }


    public class UCenterData
    {
        public Dictionary<Rtti.UTypeDesc, USharedData> SharedDatas { get; } = new Dictionary<Rtti.UTypeDesc, USharedData>();
        public bool HasSharedData(Rtti.UTypeDesc type)
        {
            lock (SharedDatas)
            {
                return SharedDatas.ContainsKey(type);
            }
        }
        public bool HasSharedData<T>() where T : USharedData
        {
            lock (SharedDatas)
            {
                return SharedDatas.ContainsKey(UTypeDescGetter<T>.TypeDesc);
            }
        }
        public USharedData GetOrNewSharedData(Rtti.UTypeDesc type)
        {
            lock(SharedDatas)
            {
                USharedData data;
                if (SharedDatas.TryGetValue(type, out data))
                    return data;
                data = Rtti.UTypeDescManager.CreateInstance(type) as USharedData;
                if (data == null)
                    return null;
                SharedDatas.Add(type, data);
                return data;
            }
        }
        public T GetOrNewSharedData<T>() where T : USharedData
        {
            return GetOrNewSharedData(UTypeDescGetter<T>.TypeDesc) as T;
        }
    }
}
