using EngineNS.GamePlay.Player;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
	public class TtPrefabPoolManager : IDisposable
	{
        public Dictionary<RName, TtPrefabPool> Pools = new Dictionary<RName, TtPrefabPool>();
        public void RegPool(RName prefabName, TtPrefabPool pool)
        {
            lock (Pools)
            {
                Pools.Add(prefabName, pool);
            }
        }
        private bool IsDisposed = false;
        public void Dispose()
        {
            IsDisposed = true;
            lock (Pools)
            {
                foreach (var i in Pools)
                {
                    i.Value.Cleanup();
                }
                Pools.Clear();
            }
        }
        public TtPrefabNode CreatePrefab(RName prefabName)
        {
            if (IsDisposed)
                return null;
            if (Pools.ContainsKey(prefabName))
            {
                return Pools[prefabName].QueryObjectSync();
            }
            else
            {
                var pool = RegPool(prefabName);
                if (pool == null)
                    return null;
                return pool.QueryObjectSync();
            }   
        }
        public void ReleasePrefab(TtPrefabNode prefabNode)
        {
            if (IsDisposed)
                return;
            if (Pools.ContainsKey(prefabNode.PrefabName))
            {
                Pools[prefabNode.PrefabName].ReleaseObject(prefabNode);
            }
        }
        private TtPrefabPool RegPool(RName prefabName)
        {
            if (IsDisposed)
                return null;
            var pool = new TtPrefabPool(prefabName);
            RegPool(prefabName, pool);
            return pool;
        }
    }
	public class TtPrefabPool : TtObjectPool<TtPrefabNode>
	{
        private RName mPrefabName;
        private TtPrefabNode mOriginPrefab = null;
        public TtPrefabPool(RName prefabName)
        {
            mPrefabName = prefabName;
            GrowStep = 10;
        }
        protected override bool IsAsyncCreate => true;
        protected override async Thread.Async.TtTask<TtPrefabNode> CreateObjectAsync()
        {
            if (mOriginPrefab == null)
            {
                var prefab = await TtEngine.Instance.PrefabManager.GetPrefab(mPrefabName);
                if(prefab != null)
                {
                    mOriginPrefab = prefab.Root;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }
            return await mOriginPrefab.CloneNode(TtEngine.Instance.PrefabManager.PrefabWorld) as TtPrefabNode;
        }
        protected override bool OnObjectRelease(TtPrefabNode obj)
        {
            EngineNS.GamePlay.Scene.TtNode.FTreeCopyStat TreeCopyStat = new (); ;
            TtNode.NodeTreeCopyData(obj, mOriginPrefab, ref TreeCopyStat);
            return true;
        }
    }
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    [Rtti.Meta]
    public partial class TtMacrossFunctionLibrary
    {
        [Rtti.Meta]
        public static TtPrefabNode InstantiatePrefab(
            [RName.PGRName(FilterExts = GamePlay.Scene.TtPrefab.AssetExt)]
            RName prefab, 
            TtScene scene)
        {
            EngineNS.GamePlay.Scene.TtNode root = scene;
            var newPrefab = TtEngine.Instance.GameInstance?.PrefabPoolManager.CreatePrefab(prefab);
            newPrefab.Parent = root;
            return newPrefab;
        }
        [Rtti.Meta]
        public static void DestroyPrefab(TtPrefabNode prefab)
        {
            prefab.Parent = null;
            TtEngine.Instance.GameInstance?.PrefabPoolManager.ReleasePrefab(prefab);
        }
    }
}

namespace EngineNS.GamePlay
{
    public partial class TtGameInstance
    {
        public GamePlay.TtPrefabPoolManager PrefabPoolManager { get; } = new GamePlay.TtPrefabPoolManager();
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay
{
	partial class TtMacrossFunctionLibrary
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_InstantiatePrefab_659189569 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtMacrossFunctionLibrary->static TtPrefabNode InstantiatePrefab(RName prefab, TtScene scene)");
		public static unsafe TtPrefabNode macross_InstantiatePrefab (string nodeName, RName prefab, TtScene scene) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":prefab", prefab);
					stackframe.SetWatchVariable(nodeName + ":scene", scene);
				}
			}
			var _return_value = InstantiatePrefab(prefab, scene);
			macross_break_InstantiatePrefab_659189569.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_DestroyPrefab_1775413286 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtMacrossFunctionLibrary->static void DestroyPrefab(TtPrefabNode prefab)");
		public static unsafe void macross_DestroyPrefab (string nodeName, TtPrefabNode prefab) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":prefab", prefab);
				}
			}
			DestroyPrefab(prefab);
			macross_break_DestroyPrefab_1775413286.TryBreak();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross