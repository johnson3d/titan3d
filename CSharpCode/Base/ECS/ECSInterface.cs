using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.ECS
{
    public interface IEntity
    {
        public TtEntityManager EntityManager { get; set; }
        public int Id { get; set; }
        public TtComponentValues<T> GetComponentValues<T>() where T : struct;
        public void OnAddToManager();
        public void OnRemoveFromManager();
    }
    public interface IComponentValues
    {
        public Type GetComponentType();
        public void SureSize(int size);
    }
    public class TtComponentValues<T> : IComponentValues where T : struct
    {
        public T[] Values;
        public void SureSize(int size)
        {
            if (Values == null || Values.Length < size)
            {
                var nV = new T[size];
                if (Values!=null)
                {
                    Array.Copy(Values, nV, Values.Length);
                }
                Values = nV;
            }
        }
        public Type GetComponentType()
        {
            return typeof(T);
        }
        public ref T GetValue(int id)
        {
            if (id < 0 || id >= Values.Length)
                throw new IndexOutOfRangeException($"Component ID {id} is out of range.");
            return ref Values[id];
        }
        public void SetValue(int id, in T value)
        {
            if (id < 0 || id >= Values.Length)
                throw new IndexOutOfRangeException($"Component ID {id} is out of range.");
            Values[id] = value;
        }
    }
    public interface ISystem
    {
        public void Process(TtEntityManager manager, float deltaTime);
    }
    public class TtEntityManager : IDisposable
    {
        public List<WeakReference<IEntity>> Entities { get; private set; } = new List<WeakReference<IEntity>>();
        private int EntityCount = 0;
        private int PrevAddId = 0;
        public int GrowthStep = 10;
        public void Initialize()
        {
            Entities.Clear();
            EntityCount = 0;
        }
        public virtual void Dispose()
        {
            Entities.Clear();
            EntityCount = 0;
            PrevAddId = 0;
        }
        public bool AddEntity(IEntity entity)
        {
            if (TtEngine.Instance.Config.UseECS == false)
                return false;
            lock(this)
            {
                if(entity.EntityManager!=null)
                {
                    if (entity.EntityManager != this)
                        return false;
                    return true;
                }

                entity.Id = -1; // Reset ID to ensure it is assigned correctly
                if (AddToManager(entity))
                    return true;
                
                PrevAddId = 0; // Reset PrevAddId to start from the beginning
                if (AddToManager(entity))
                    return true;

                Entities.AddRange(new WeakReference<IEntity>[GrowthStep]);
                foreach (var c in ComponentValues)
                {
                    c.Value.SureSize(Entities.Count);
                }
                if (AddToManager(entity))
                    return true;

                System.Diagnostics.Debug.Assert(entity.Id >= 0 && entity.Id < Entities.Count, "Entity ID out of range");
                return false;
            }
        }
        private bool AddToManager(IEntity entity)
        {
            for (; PrevAddId < Entities.Count; PrevAddId++)
            {
                if (Entities[PrevAddId]==null)
                {
                    entity.EntityManager = this;
                    entity.Id = PrevAddId;
                    Entities[PrevAddId] = new WeakReference<IEntity>(entity);
                    entity.OnAddToManager(); // Notify the entity that it has been added
                    EntityCount++;
                    return true;
                }
                else if (Entities[PrevAddId].TryGetTarget(out var save) == false)
                {
                    entity.EntityManager = this;
                    entity.Id = PrevAddId;
                    Entities[PrevAddId].SetTarget(entity);
                    entity.OnAddToManager(); // Notify the entity that it has been added
                    return true;
                }
            }
            return false;
        }
        public bool RemoveEntity(int id)
        {
            lock(this)
            {
                if (id<0 || id>=Entities.Count)
                {
                    return false;
                }
                if (Entities[id] != null)
                {
                    if(Entities[id].TryGetTarget(out var entity))
                    {
                        entity.OnRemoveFromManager(); // Notify the entity that it is being removed
                        entity.EntityManager = null; // Clear the entity manager reference
                        entity.Id = -1; // Reset ID to indicate removal
                    }

                    Entities[id].SetTarget(null);
                    EntityCount--;
                }
                return true;
            }
        }
        public T GetEntity<T>(int id) where T : class , IEntity
        {
            if (Entities[id]!=null && Entities[id].TryGetTarget(out var result))
                return result as T;
            return null;
        }
        public Dictionary<Type, IComponentValues> ComponentValues { get; private set; } = new Dictionary<Type, IComponentValues>();
        public void RegisterComponent<T>() where T : struct
        {
            if (!ComponentValues.ContainsKey(typeof(T)))
            {
                ComponentValues[typeof(T)] = new TtComponentValues<T>();
            }
        }
        public void RegisterComponent<T>(TtComponentValues<T> values) where T : struct
        {
            ComponentValues[typeof(T)] = values;
        }
        public TtComponentValues<T> FindComponentValues<T>() where T : struct
        {
            if (ComponentValues.TryGetValue(typeof(T), out var values))
            {
                return values as TtComponentValues<T>;
            }
            return null;
        }

        public List<ISystem> Systems { get; private set; } = new List<ISystem>();
        public void Process(float deltaTime)
        {
            foreach (var sys in Systems)
            {
                sys.Process(this, deltaTime);
            }
        }
    }
}
