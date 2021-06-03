using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator
{
    public struct FRange
    {
        public static System.Random Gen = new System.Random();
        public float MinValue;
        public float MaxValue;
        public float GetRandomValue()
        {
            return (float)(Gen.NextDouble() * (MaxValue - MinValue));
        }
    }
    public enum EUnitType
    {
        Element,//Power,//SolarEnergy,FossilEnergy,
        Plant,
        Animal,
    }
    public enum EElementType
    {
        Fiber = 0,//纤维
        Protein,//蛋白质
        Fat,//脂肪
        Metal,//金属
        Organic,//有机物
        Vitamin,//维生素
        Count,
    }
    public class IUnitType
    {
        public FRange EnergyConsume;
        public FRange EnergyReserve;
        public FRange[] CurrentElements = new FRange[(int)EElementType.Count];
        public FRange LifeSpan;
    }
    public class IUnit
    {
        public ICell StayCell;
        public Vector3 Position;
        public float EnergyConsume;//基础代谢
        public float EnergyReserve;//能量储备
        public float[] CurrentElements = new float[(int)EElementType.Count];
        public float LifeSpan;//自然寿命

        public virtual void InitUnit(IUnitType src)
        {

        }
        public virtual void Tick(float elapse, World world)
        {
            LifeSpan -= elapse;
            if (LifeSpan < 0)
            {
                world.RemoveUnit(this);
            }
            EnergyReserve -= EnergyConsume;
        }
    }
    public class IPlantType : IUnitType
    {
        public enum EType
        {
            Grass,
            Bush,
            Tree,
            //...
        }
        public EType Type;
        public FRange SolarEnergyConverter;
        public FRange[] SoilElements = new FRange[(int)EElementType.Count];

        public static IPlant NewUnit(EType t)
        {
            var src = Manager.Instance.GetSource(t);
            var result = new IPlant();
            result.InitUnit(src);
            return result;
        }
        public class Manager
        {
            public static Manager Instance = new Manager();
            public Dictionary<EType, IPlantType> Types = new Dictionary<EType, IPlantType>();
            public IPlantType GetSource(EType t)
            {
                return Types[t];
            }
        }
    }
    public class IPlant : IUnit
    {
        public IPlantType SrcType;
        public float SolarEnergyConverter = 1;//太阳能转换效率
        public float[] SoilElements = new float[(int)EElementType.Count];//土壤元素清单，初始化后，要有动植物死亡腐烂后补充

        public override void Tick(float elapse, World world)
        {
            base.Tick(elapse, world);

            EnergyReserve += world.SolarConstant * SolarEnergyConverter;
        }
    }
    public class IAnimalType : IUnitType
    {
        public enum EType
        {
            Rabit,
            Wolf,
            Tiger,
            Human,
        }
        public FRange FoodConverter;
        public FRange StoreExcrement;
        public FRange MaxExcrement;
        public List<IPlantType.EType> PlantFoods = new List<IPlantType.EType>();
        public List<EType> AnimalFoods = new List<EType>();
        public static IAnimal NewUnit(EType t)
        {
            var src = Manager.Instance.GetSource(t);
            var result = new IAnimal();
            result.InitUnit(src);
            return result;
        }
        public class Manager
        {
            public static Manager Instance = new Manager();
            public Dictionary<EType, IAnimalType> Types = new Dictionary<EType, IAnimalType>();
            public IAnimalType GetSource(EType t)
            {
                return Types[t];
            }
        }
    }
    public class IAnimal : IUnit
    {
        public IAnimalType SrcType;
        public float FoodConverter = 1;//食物能量转换率，没有转换的成为排泄物
        public float StoreExcrement;//积累排泄物，超过阈值就要排出
        public float MaxExcrement;
    }
}
