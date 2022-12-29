using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Plugins.GameItems
{
    [Rtti.Meta]
    public class AItemRequirement
    {
        [Rtti.Meta]
        public List<RName> Items { get; set; } = new List<RName>();
        [Rtti.Meta]
        public int RequireCount { get; set; }
    }

    [Rtti.Meta]
    public class AItemMakeSuccess
    {
        [Rtti.Meta]
        public RName Item { get; set; }
        [Rtti.Meta]
        public int Num { get; set; }
        [Rtti.Meta]
        public float RateOfSuccess { get; set; } = 1.0f;
    }


    [Rtti.Meta]
    public class AItemMaker
    {
        [Rtti.Meta]
        public List<AItemRequirement> InputItems { get; set; } = new List<AItemRequirement>();
        [Rtti.Meta]
        public List<AItemRequirement> Catalyzers { get; set; } = new List<AItemRequirement>();
        [Rtti.Meta]
        public List<AItemMakeSuccess> OutputItems { get; set; } = new List<AItemMakeSuccess>();
        
        public virtual List<AGameItemBox> MakeItems(AGameItemInventory inventory, Support.URandom rd)
        {
            var consumes = new List<AGameItemBox>();
            var catalyzers = new List<AGameItemBox>();
            bool bMakeSuccessed = false;
            try
            {
                foreach (var i in Catalyzers)
                {
                    bool bFinded = false;
                    foreach (var j in i.Items)
                    {
                        var box = inventory.TakeItem(j, i.RequireCount);
                        if (box != null)
                        {
                            catalyzers.Add(box);
                            bFinded = true;
                            break;
                        }
                    }

                    if (bFinded == false)
                        return null;
                }

                foreach (var i in InputItems)
                {
                    bool bFinded = false;
                    foreach (var j in i.Items)
                    {
                        var box = inventory.TakeItem(j, i.RequireCount);
                        if (box != null)
                        {
                            consumes.Add(box);
                            bFinded = true;
                            break;
                        }
                    }

                    if (bFinded == false)
                        return null;
                }

                var result = new List<AGameItemBox>();
                foreach (var i in OutputItems)
                {
                    if (rd.GetProbability(i.RateOfSuccess))
                    {
                        int NumOfCreate = i.Num;
                        while (true)
                        {
                            var box = AGameItemBox.CreateBox(i.Item.GetTagObject<AGameItem>());
                            if (box.MaxStack >= NumOfCreate)
                            {
                                box.Stack = NumOfCreate;
                                NumOfCreate = 0;
                                result.Add(box);
                                break;
                            }
                            else
                            {
                                box.Stack = box.MaxStack;
                                NumOfCreate -= box.Stack;
                            }
                        }
                    }
                }
                bMakeSuccessed = true;
                return result;
            }
            catch(Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null;
            }
            finally
            {
                foreach(var i in catalyzers)
                {
                    AGameItemBox box = i;
                    inventory.PutItem(ref box);
                }
                catalyzers.Clear();

                if (bMakeSuccessed == false)
                {
                    foreach (var i in consumes)
                    {
                        AGameItemBox box = i;
                        inventory.PutItem(ref box);
                    }
                }
                consumes.Clear();
            }
        }
    }
}
