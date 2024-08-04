using EngineNS.IO;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class AGameItemAssemblyDesc : UAssemblyDesc
        {
            public AGameItemAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:GameItems AssemblyDesc Created");
            }
            ~AGameItemAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:GameItems AssemblyDesc Destroyed");
            }
            public override string Name { get => "GameItems"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static AGameItemAssemblyDesc AssmblyDesc = new AGameItemAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.GameItems
{
    public class UPluginLoader
    {
        public static UGameItemPlugin? mPluginObject = new UGameItemPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }

    public class UGameItemPlugin : Bricks.AssemblyLoader.IPlugin
    {
        public AGameItemDescriptorManager ItemDescriptorManager = new AGameItemDescriptorManager();
        public void OnLoadedPlugin()
        {
            //ItemDescriptorManager.Initialize();
            var editor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
            if (editor != null)
            {
                editor.ContentBrowser.OnTypeChanged();
            }
        }
        public void OnUnloadPlugin()
        {
            //UPluginDescriptor.mPluginObject = null;
            var editor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
            if (editor != null)
            {
                editor.ContentBrowser.OnTypeChanged();
            }
        }
    }

    [Rtti.Meta]
    public class AGameItemDescriptorAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return AGameItemDescriptor.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Item";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            return ((UGameItemPlugin)UPluginLoader.mPluginObject).ItemDescriptorManager.FindDescriptor(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "vms", null);
        //}
        protected override Color4b GetBorderColor()
        {
            return Color4b.LightYellow;
        }
    }

    [Rtti.Meta]
    [AGameItemDescriptor.ADescriptor]
    [IO.CommonCreate]
    [IO.AssetCreateMenu(MenuName = "Game/Item")]
    public class AGameItemDescriptor : IO.IAsset
    {
        #region IAsset
        public const string AssetExt = ".item";
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new AGameItemDescriptorAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }
            IO.TtFileManager.SaveObjectToXml(name.Address, this);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
        public class ADescriptorAttribute : URNameTagObjectAttribute
        {
            public override object GetTagObject(RName rn)
            {
                return ((UGameItemPlugin)UPluginLoader.mPluginObject).ItemDescriptorManager.FindDescriptor(rn);
            }
        }
        [Rtti.Meta]
        public string Name { get; set; }
        [Rtti.Meta]
        public int MaxStack { get; set; } = 1;
        public virtual AGameItem CreateItem()
        {
            var item = new AGameItem();
            item.DescriptorName = AssetName;
            if (this.MaxStack == 1)
                item.Uid = Guid.NewGuid();
            else
                item.Uid = Guid.Empty;
            return item;
        }
    }
    public class AGameItemDescriptorManager
    {
        public Dictionary<RName, AGameItemDescriptor> ItemDescriptors { get; } = new Dictionary<RName, AGameItemDescriptor>();
        public AGameItemDescriptor FindDescriptor(RName id)
        {
            AGameItemDescriptor result;
            if (ItemDescriptors.TryGetValue(id, out result))
                return result;
            return null;
        }
        public bool RegDescriptor(AGameItemDescriptor desc)
        {
            AGameItemDescriptor result;
            if (ItemDescriptors.TryGetValue(desc.AssetName, out result))
                return false;
            ItemDescriptors.Add(desc.AssetName, desc);
            return true;
        }

        public bool Initialize(RName location)
        {
            var files = IO.TtFileManager.GetFiles(location.Address, "*.item", false);
            foreach (var i in files)
            {
                var descriptor = IO.TtFileManager.LoadXmlToObject(i) as AGameItemDescriptor;
                if (descriptor == null)
                    continue;
                RegDescriptor(descriptor);
            }
            return true;
        }
    }

    [Rtti.Meta]
    public class AGameItem
    {
        public AGameItemDescriptor Descriptor
        {
            get
            {
                if (DescriptorName == null)
                    return null;
                return DescriptorName.GetTagObject<AGameItemDescriptor>();
            }
        }
        [Rtti.Meta]
        public RName DescriptorName { get; set; }
        [Rtti.Meta]
        public Guid Uid { get; set; }
    }
    public class AGameItemBox
    {
        public AGameItem Item { get; set; }
        public int Stack { get; set; } = 0;
        public int MaxStack
        {
            get
            {
                if (Item == null)
                    return 0;
                return Item.Descriptor.MaxStack;
            }
        }
        public int FreeStack
        {
            get
            {
                return MaxStack - Stack;
            }
        }
        public static AGameItemBox CreateBox(AGameItem item)
        {
            var result = new AGameItemBox();
            result.Item = item;
            result.Stack = 1;
            return result;
        }
        public static AGameItemBox CreateBox(AGameItemDescriptor item)
        {
            var result = new AGameItemBox();
            result.Item = item.CreateItem();
            result.Stack = 1;
            return result;
        }
    }
    public class AGameItemInventory
    {
        public AGameItemBox[] Items;
        public int Capacity
        {
            get
            {
                if (Items == null)
                    return 0;
                return Items.Length;
            }
        }
        public int Count
        {
            get
            {
                int index = 0;
                foreach (var item in Items)
                {
                    if (item != null)
                        index++;
                }
                return index;
            }
        }
        public bool ResizeInvetory(int num)
        {
            if (num < Count)
                return false;
            var newItems = new AGameItemBox[num];
            if (Items != null)
            {
                int index = 0;
                foreach (var i in Items)
                {
                    if (i != null)
                        Items[index++] = i;
                }
            }
            Items = newItems;
            return true;
        }
        public void SwapItem(int location, ref AGameItemBox item)
        {
            if (item == null)
                return;
            var save = Items[location];
            Items[location] = item;
            item = save;
        }
        public bool PutItem(int location, ref AGameItemBox item)
        {
            if (item == null || location < 0 || location >= Items.Length)
                return false;
            if (Items[location] != null)
            {
                if (Items[location].Item.Descriptor != item.Item.Descriptor || Items[location].FreeStack < item.Stack)
                {
                    return false;
                }
                else
                {
                    Items[location].Stack += item.Stack;
                    item = null;
                }
            }
            else
            {
                Items[location] = item;
                item = null;
            }
            return true;
        }
        public AGameItemBox TakeItem(int location, int num = 1)
        {
            if (Items[location].Item == null || location < 0 || location >= Items.Length)
                return null;
            if (Items[location].FreeStack < num)
                return null;

            if (Items[location].Stack == num)
            {
                var result = AGameItemBox.CreateBox(Items[location].Item);
                result.Stack = num;
                Items[location].Item = null;
                Items[location].Stack = 0;
                return result;
            }
            else
            {
                var result = AGameItemBox.CreateBox(Items[location].Item.Descriptor);
                result.Stack = num;
                Items[location].Stack -= num;
                return result;
            }
        }

        public AGameItemBox TakeItem(RName item, int num)
        {
            return null;
        }
        public bool PutItem(ref AGameItemBox item)
        {
            return false;
        }
    }
}
