using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator
{
    public class ICell
    {
        public List<IPlant> Plants = new List<IPlant>();
    }
    public class World
    {
        public float SolarConstant = 1000;//太阳常数，单位面积接受的太阳能
        public List<IPlant> Plants = new List<IPlant>();
        public List<IAnimal> Animals = new List<IAnimal>();

        public ICell[,] Cells = new ICell[20, 20];
        public Vector2 WorldSize;
        public float CellX;
        public float CellZ;
        public Vector3 RandomPosition()
        {
            return new Vector3((float)FRange.Gen.NextDouble() * WorldSize.X, 0, (float)FRange.Gen.NextDouble() * WorldSize.Y);
        }
        public ICell GetCell(Vector3 pos)
        {
            var x = (int)(pos.X % CellX);
            var z = (int)(pos.Z % CellZ);
            return Cells[z, x];
        }
        public void InitWorld(Vector2 size, int numPlant, int numAnimal)
        {
            WorldSize = size;

            for (int i = 0; i < numPlant; i++)
            {
                NewPlant(IPlantType.EType.Grass, RandomPosition());
            }
            for (int i = 0; i < numAnimal; i++)
            {

            }
        }
        public void Simulate()
        {
            foreach(var i in Plants)
            {
                i.Tick(0.05f, this);
            }
        }
        public IPlant NewPlant(IPlantType.EType t, Vector3 pos)
        {
            var result = IPlantType.NewUnit(IPlantType.EType.Grass);
            result.Position = pos;
            result.StayCell = GetCell(pos);

            Plants.Add(result);
            return result;
        }
        public void RemoveUnit(IUnit u)
        {

        }
    }
}
