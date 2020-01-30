using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.DataProvider
{
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public abstract class IDataSet
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public abstract object GetData(int index);
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(1)]
        public object GetDataAs(int index,
            [Editor.Editor_TypeFilterAttribute(typeof(Macross.NullObject))]
            System.Type type)
        {
            return GetData(index);
        }
        public delegate bool FOnVisitDataRow(object data);
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public abstract object FindData(FOnVisitDataRow visitor);
    }
    public class GDataSetManager
    {
        Dictionary<string, IDataSet> DataSets = new Dictionary<string, IDataSet>();
        public void Cleanup()
        {
            lock (DataSets)
            {
                DataSets.Clear();
            }
        }
        public void RegDataSet(Type objType, IDataSet ds)
        {
            lock (DataSets)
            {
                DataSets[objType.FullName] = ds;
            }
        }
        public void UnRegDataSet(Type t)
        {
            lock (DataSets)
            {
                DataSets.Remove(t.FullName);
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public IDataSet GetDataSet(Type t)
        {
            IDataSet ds;
            if (DataSets.TryGetValue(t.FullName, out ds))
                return ds;
            return null;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public GDataSet LoadDataSet(
            [Editor.Editor_TypeFilterAttribute(typeof(Macross.NullObject))]
            Type objType,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Excel)]
            //[EngineNS.Editor.Editor_RNameMExcelType(typeof(T))]：期待以后C#能这么写
            RName name)
        {
            var result = new GDataSet();
            result.LoadDataSet(objType, name, true);
            return result;
        }
        public async System.Threading.Tasks.Task Init()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            //这里要通过macross，把所有DataSet都初始化好，然后加载进来
        }
    }

    public class GDataSetManagerProcessor : CEngineAutoMemberProcessor
    {
        public override async System.Threading.Tasks.Task<object> CreateObject()
        {
            var Services = new GDataSetManager();
            await Services.Init();

            return Services;
        }
        public override void Tick(object obj)
        {
            var Services = obj as GDataSetManager;
            
        }
        public override void Cleanup(object obj)
        {
            var Services = obj as GDataSetManager;
            Services.Cleanup();
        }
    }

    [Editor.Editor_MacrossClassAttribute(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public partial class GDataSet : IDataSet
    {
        public List<object> mDataRows;
        public List<object> DataRows
        {
            get
            {
                return mDataRows;
            }
        }
        public bool LoadExcel(Type objType, RName name)
        {
            bool result = false;
            LoadDataSetFromExcel(objType, name, ref result);
            return result;
        }
        partial void LoadDataSetFromExcel(Type objType, RName name, ref bool result);
        public void Save2Xnd(string address)
        {
            //var xnd = IO.XndHolder.NewXNDHolder();
            //var attr = xnd.Node.AddAttrib("Excel");
            //attr.BeginWrite();
            //attr.Write(mDataRows.Count);
            //foreach (var i in mDataRows)
            //{
            //    attr.WriteMetaObject(i);
            //}
            //attr.EndWrite();

            //IO.XndHolder.SaveXND(name.Address, xnd);
            Save2Xnd(address, mDataRows);
        }
        public static void Save2Xnd(string name, List<object> lst)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            var attr = xnd.Node.AddAttrib("Excel");
            attr.BeginWrite();
            attr.Write(lst.Count);
            foreach (var i in lst)
            {
                attr.WriteMetaObject(i);
            }
            attr.EndWrite();

            IO.XndHolder.SaveXND(name, xnd);
        }
        public bool LoadXnd(string name)
        {
            using (var xnd = IO.XndHolder.SyncLoadXND(name))
            {
                if (xnd == null)
                    return false;
                var attr = xnd.Node.FindAttrib("Excel");
                if (attr == null)
                    return false;

                mDataRows = new List<object>();

                attr.BeginRead();
                int count = 0;
                attr.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    var obj = attr.ReadMetaObject();
                    mDataRows.Add(obj);
                }
                attr.EndRead();
                return true;
            }
        }
        public virtual bool LoadDataSet(
            Type objType,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Excel)]
            //[EngineNS.Editor.Editor_RNameMExcelType(typeof(T))]：期待以后C#能这么写
            RName name,
            bool reg2Manager)
        {
#if !PWindow
            if (LoadXnd(name.Address + ".dataset"))
            {
                return true;
            }
#else
            if (EngineNS.IO.FileManager.UseCooked != null)
            {
                if (LoadXnd(name.Address + ".dataset"))
                {
                    return true;
                }
            }
            bool result = false;
            LoadDataSetFromExcel(objType, name, ref result);
            if (result)
            {
                if (reg2Manager)
                {
                    CEngine.Instance.GameInstance?.DataSetManager?.RegDataSet(objType, this);
                }
                return true;
            }
            else
            {
                if (LoadXnd(name.Address + ".dataset"))
                {
                    if (reg2Manager)
                    {
                        CEngine.Instance.GameInstance?.DataSetManager?.RegDataSet(objType, this);
                    }
                    return true;
                }
            }
#endif
            return false;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public override object GetData(int index)
        {
            if (DataRows == null)
                return null;
            if (index < 0 || index >= DataRows.Count)
                return null;
            return this.DataRows[index];
        }
        public T GetDataRow<T>(int index) where T : class
        {
            if (DataRows == null)
                return null;
            if (index < 0 || index >= DataRows.Count)
                return null;
            return this.DataRows[index] as T;
        }
        public override object FindData(FOnVisitDataRow visitor)
        {
            if (mDataRows == null)
                return null;
            for(int i=0; i< mDataRows.Count; i++)
            {
                if (visitor(mDataRows[i]))
                    return mDataRows[i];
            }
            return null;
        }
    }

    public partial class DataSet<T> : IDataSet
        where T : class, new()
    {
        private List<T> mDataRows;
        //这里放进去的大多数情况是Macross对象
        //因为没有用CEngine.Instance.MacrossDataManager.NewObjectGetter
        //而是用的CreateInstance创建的对象
        //会有macross刷新后，数据对象没有更新的情况，
        //但是我们做了假设，所有DataSet都在GameInstance里面，如果重新编译了Game.dll
        //我们必须重新加载所有DataSet的数据，所以没问题
        public List<T> DataRows
        {
            get
            {
                return mDataRows;
            }
        }

        partial void LoadDataSetFromExcel(RName name, ref bool result);
        public void Save2Xnd(RName name)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            var attr = xnd.Node.AddAttrib("Excel");
            attr.BeginWrite();
            attr.Write(mDataRows.Count);
            foreach (var i in mDataRows)
            {
                attr.WriteMetaObject(i);
            }
            attr.EndWrite();

            IO.XndHolder.SaveXND(name.Address, xnd);
        }
        public bool LoadXnd(RName name)
        {
            using (var xnd = IO.XndHolder.SyncLoadXND(name.Address))
            {
                if (xnd == null)
                    return false;
                var attr = xnd.Node.FindAttrib("Excel");
                if (attr == null)
                    return false;

                mDataRows = new List<T>();

                attr.BeginRead();
                int count = 0;
                attr.Read(out count);
                for(int i=0;i<count;i++)
                {
                    var obj = attr.ReadMetaObject() as T;
                    mDataRows.Add(obj);
                }
                attr.EndRead();
                return true;
            }   
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual bool LoadDataSet(
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Excel)]
            //[EngineNS.Editor.Editor_RNameMExcelType(typeof(T))]：期待以后C#能这么写
            RName name, 
            bool reg2Manager)
        {
#if !PWindow
            if (LoadXnd(RName.GetRName(name.Name + ".dateset")))
            {
                return true;
            }
#else
            bool result = false;
            LoadDataSetFromExcel(name, ref result);
            if (result)
            {
                if (reg2Manager)
                {
                    CEngine.Instance.GameInstance?.DataSetManager?.RegDataSet(typeof(T), this);
                }
                //Save2Xnd(RName.GetRName(name.Name + ".dateset"));
                return true;
            }
            else
            {
                if (LoadXnd(RName.GetRName(name.Name + ".dateset")))
                {
                    return true;
                }
            }
#endif
            return false;
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public override object GetData(int index)
        {
            if (DataRows == null)
                return default(T);
            if (index < 0 || index >= DataRows.Count)
                return default(T);
            return this.DataRows[index];
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public T GetDataRow(int index)
        {
            if (DataRows == null)
                return default(T);
            if(index<0||index>=DataRows.Count)
                return default(T);
            return this.DataRows[index];
        }
        public override object FindData(FOnVisitDataRow visitor)
        {
            if (mDataRows == null)
                return null;
            for (int i = 0; i < mDataRows.Count; i++)
            {
                if (visitor(mDataRows[i]))
                    return mDataRows[i];
            }
            return null;
        }
    }
    [Rtti.MetaClass]
    public class XlslSubObject : IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public int A
        {
            get;
            set;
        } = 1;
        [Rtti.MetaData]
        public float B
        {
            get;
            set;
        } = 2;
    }
    [Rtti.MetaClass]
    public class XlslObject : IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int A
        {
            get;
            set;
        } = 1;
        [Rtti.MetaData]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float B
        {
            get;
            set;
        } = 2;
        [Rtti.MetaData]
        public bool C
        {
            get;
            set;
        } = true;
        [Rtti.MetaData]
        public string D
        {
            get;
            set;
        } = "D-Value";
        [Rtti.MetaData]
        public XlslSubObject E
        {
            get;
            set;
        } = new XlslSubObject();
        [Rtti.MetaData]
        public List<XlslSubObject> F
        {
            get;
            set;
        } = new List<XlslSubObject>();
        [Rtti.MetaData]
        public List<string> G
        {
            get;
            set;
        } = new List<string>();
        public string H
        {
            get;
            set;
        } = "HHH";
    }
    //下面是测试代码，以后所有Macross生成的DataSet都要产生下面类似的代码
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class DataSet_XlslObject : DataSet<XlslObject>
    {
        public override bool LoadDataSet(
            [EngineNS.Editor.Editor_RNameMExcelType(typeof(XlslObject))]
            RName name,
            bool reg2Manager)
        {
            return base.LoadDataSet(name, reg2Manager);
        }
    }
}

namespace EngineNS.GamePlay
{
    public partial class GGameInstance
    {
        [CEngineAutoMemberAttribute(typeof(Bricks.DataProvider.GDataSetManagerProcessor))]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable| Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Bricks.DataProvider.GDataSetManager DataSetManager
        {
            get;
            set;
        }
    }
}
