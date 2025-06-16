using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtMissionData", KeyName = "MissionId", HeadRow = 0, DataStartRow = 3)]
    public class TtMissionData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        public TtMissionData()
        {
            MissionType = nameof(TtMission);
        }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "MissionId")]
        public int MissionId { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "MissionName")]
        public string MissionName { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "MissionType")]
        public string MissionType { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "MissionConditions")]
        public List<int> MissionConditions { get; set; } = new List<int>();

        [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtGoodsCondition", KeyName = "ItemId", HeadRow = 0, DataStartRow = 3)]
        public class TtGoodsCondition : EngineNS.Bricks.DataSet.TtDataProvider
        {
            [EngineNS.Rtti.Meta]
            [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemId")]
            public int ItemId { get; set; }
            [EngineNS.Rtti.Meta]
            [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Count")]
            public int Count { get; set; }
        }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "GoodsAcceptConditions")]
        public List<TtGoodsCondition> GoodsAcceptConditions { get; set; } = new List<TtGoodsCondition>();
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "GoodsFinishConditions")]
        public List<TtGoodsCondition> GoodsFinishConditions { get; set; } = new List<TtGoodsCondition>();
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "AwardGoods")]
        public List<TtGoodsCondition> AwardGoods { get; set; } = new List<TtGoodsCondition>();
    }
    public class TtMissionDataManager : EngineNS.Bricks.DataSet.TtDataManager<TtMissionData>
    {

    }
    public class TtMission
    {
        public TtMissionData Data;
        public virtual bool CanAccept(TtMissionData data, TtCharacterStateNode character)
        {
            foreach (var i in data.MissionConditions)
            {
                if (character.MissionInventory.IsFinishedMission(i) == false)
                    return false;
            }
            foreach (var i in data.GoodsAcceptConditions)
            {
                int count = 0;
                var pos = character.GoodsInventory.FindItem(i.ItemId, 0);
                while (pos >= 0)
                {
                    var item = character.GoodsInventory.PeekItem(pos);
                    if (item.Data.Duration > 0)
                        count += item.Data.Duration;
                    else
                        count += 1;
                    if (count >= i.Count)
                        break;
                    pos = character.GoodsInventory.FindItem(i.ItemId, pos + 1);
                }
                if (count < i.Count)
                    return false;
            }
            return true;
        }
        public static bool AcceptMission(TtMissionData data, TtCharacterStateNode character)
        {
            var type = EngineNS.Rtti.TtTypeDesc.TypeOf($"Survivor.{data.MissionName}@Survivor");
            if (type == null)
            {
                return false;
            }
            var mission = EngineNS.Rtti.TtTypeDescManager.CreateInstance(type) as TtMission;
            mission.Data = data;
            if (mission.CanAccept(data, character) == false)
                return false;
            return character.MissionInventory.PushMission(mission);
        }
        public virtual bool IsFinished(TtCharacterStateNode character)
        {
            foreach (var i in Data.GoodsFinishConditions)
            {
                int count = 0;
                var pos = character.GoodsInventory.FindItem(i.ItemId, 0);
                while (pos >= 0)
                {
                    var item = character.GoodsInventory.PeekItem(pos);
                    if (item.Data.Duration > 0)
                        count += item.Data.Duration;
                    else
                        count += 1;
                    if (count >= i.Count)
                        break;
                    pos = character.GoodsInventory.FindItem(i.ItemId, pos + 1);
                }
                if (count < i.Count)
                    return false;
            }
            return true;
        }
        public bool FinishMission(TtCharacterStateNode character)
        {
            if (IsFinished(character) == false)
                return false;

            character.MissionInventory.Missions.Remove(this);
            character.MissionInventory.FinishedMissions.Add(this);

            Award(character);
            return true;
        }
        public virtual void Award(TtCharacterStateNode character)
        {
            foreach (var i in Data.AwardGoods)
            {
                for (int j = 0; j < i.Count; j++)
                {
                    var data = TtDatabase.Instance.GetItemData(i.ItemId);
                    var goods = TtItem.CreateItem(data);
                    if (character.GoodsInventory.AutoPutIn(goods) == false)
                    {
                        //可以放到一个无限存储空间，只能取，不能做其他操作，避免因为背包满了丢奖励
                        character.ReadOnlyInventory.AutoPutIn(goods);
                    }
                }
            }
        }
        public virtual void TickMission(TtCharacterStateNode character)
        {

        }
    }

    public class TtMissionInventory
    {
        public List<TtMission> FinishedMissions = new List<TtMission>();
        public List<TtMission> Missions = new List<TtMission>();
        public bool IsFinishedMission(int missionId)
        {
            foreach (var i in FinishedMissions)
            {
                if (i.Data.MissionId == missionId)
                    return true;
            }
            return false;
        }
        public bool HasMission(int missionId)
        {
            foreach (var i in Missions)
            {
                if (i.Data.MissionId == missionId)
                    return true;
            }
            return false;
        }
        public bool PushMission(TtMission mission)
        {
            foreach(var item in Missions)
            {
                if (item.Data.MissionId == mission.Data.MissionId)
                    return false;
            }
            Missions.Add(mission);
            return true;
        }
        public void Tick(TtCharacterStateNode character)
        {
            foreach (var i in Missions)
            {
                i.TickMission(character);
            }
        }
    }
}
