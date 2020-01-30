using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class VAssemblyManager
    {
        static VAssemblyManager smInstance = new VAssemblyManager();
        public static VAssemblyManager Instance
        {
            get { return smInstance; }
        }

        public void Log(string str)
        {
            System.Diagnostics.Trace.WriteLine(str);
        }

        public void Log_Warning(string str)
        {
            Log("<W>" + str);
        }

        public void Log_Error(string str)
        {
            Log("<Err>" + str);
        }

        private VAssemblyManager()
        {
            
        }

        Dictionary<string, VAssembly> mAssembly_FullNameDic = new Dictionary<string, VAssembly>();
        Dictionary<Type, VAssembly> mAssembly_TypeDic = new Dictionary<Type, VAssembly>();
        Dictionary<string, VAssembly> mAssembly_AbsFileDic = new Dictionary<string, VAssembly>();
        public void ClearCache()
        {
            mAssembly_FullNameDic.Clear();
            mAssembly_TypeDic.Clear();
            mAssembly_AbsFileDic.Clear();
        }

        public VAssembly FindAssemblyFromType(Type sourceType)
        {
            if (sourceType == null)
                return null;
            VAssembly outAssembly;
            if (mAssembly_TypeDic.TryGetValue(sourceType, out outAssembly))
                return outAssembly;
            if(mAssembly_FullNameDic.TryGetValue(sourceType.Assembly.FullName, out outAssembly))
            {
                mAssembly_TypeDic[sourceType] = outAssembly;
                return outAssembly;
            }

            return null;
        }
        public VAssembly GetAssemblyFromType(Type sourceType)
        {
            if (sourceType == null)
                return null;
            VAssembly outAssembly;
            if (mAssembly_TypeDic.TryGetValue(sourceType, out outAssembly))
                return outAssembly;

            if (mAssembly_FullNameDic.TryGetValue(sourceType.Assembly.FullName, out outAssembly))
            {
                mAssembly_TypeDic[sourceType] = outAssembly;
                return outAssembly;
            }

            outAssembly = EngineNS.Rtti.RttiHelper.GetAnalyseAssemblyByFullName(sourceType.Assembly.FullName);
            if (outAssembly != null)
            {
                mAssembly_TypeDic[sourceType] = outAssembly;
                return outAssembly;
            }

            outAssembly = new VAssembly(sourceType.Assembly, ECSType.Common);
            outAssembly.KeyName = sourceType.Assembly.GetName().Name;// + ".dll";
            mAssembly_TypeDic[sourceType] = outAssembly;
            mAssembly_FullNameDic[outAssembly.FullName] = outAssembly;
            if (!outAssembly.Assembly.IsDynamic)
                mAssembly_AbsFileDic[outAssembly.Assembly.Location.Replace("\\", "/")] = outAssembly;
            return outAssembly;
            //var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            //for(int i=0; i<assemblys.Length; i++)
            //{
            //    try
            //    {
            //        var types = assemblys[i].GetTypes();
            //        for (int typeIdx = 0; typeIdx < types.Length; typeIdx++)
            //        {
            //            ////////////////////////////////////////////////////////
            //            if(types[typeIdx].Name.Contains("ObservableCollection"))
            //            {

            //            }
            //            ////////////////////////////////////////////////////////
            //            if (types[typeIdx].Equals(sourceType))
            //            {
            //                outAssembly = new VAssembly(assemblys[i], ECSType.Common);
            //                outAssembly.KeyName = assemblys[i].GetName().Name + ".dll";
            //                mAssembly_TypeDic[sourceType] = outAssembly;
            //                mAssembly_FullNameDic[outAssembly.FullName] = outAssembly;
            //                return outAssembly;
            //            }
            //        }
            //    }
            //    catch (System.Exception ex)
            //    {
            //        System.Diagnostics.Trace.WriteLine(ex.ToString());
            //    }
            //}

            //return null;
        }

        public VAssembly GetAssembly(System.Reflection.Assembly assembly, ECSType csType, string keyName = "")
        {
            if (assembly == null)
                return null;

            var vAssem = EngineNS.Rtti.RttiHelper.GetAnalyseAssemblyByFullName(assembly.FullName);
            if(vAssem != null)
            {
                mAssembly_FullNameDic[vAssem.FullName] = vAssem;
                if (!assembly.IsDynamic)
                    mAssembly_AbsFileDic[assembly.Location.Replace("\\", "/")] = vAssem;
                return vAssem;
            }
            if (mAssembly_FullNameDic.TryGetValue(assembly.FullName, out vAssem))
                return vAssem;
            vAssem = new VAssembly(assembly, csType);
            if (string.IsNullOrEmpty(keyName))
                vAssem.KeyName = assembly.GetName().Name;// + ".dll";
            else
                vAssem.KeyName = keyName;
            mAssembly_FullNameDic[assembly.FullName] = vAssem;
            if(!assembly.IsDynamic)
                mAssembly_AbsFileDic[assembly.Location.Replace("\\", "/")] = vAssem;
            return vAssem;
        }

        static Profiler.TimeScope ScopeLoadAssm = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), nameof(LoadAssembly));
        public VAssembly LoadAssembly(string assemblyAbsFile, EngineNS.ECSType csType, bool fromBytes = false, bool useCashe = true, string keyName = "", bool useVPDB = true)
        {
            ScopeLoadAssm.Begin();
            var ret = LoadAssembly_Impl(assemblyAbsFile, csType, fromBytes, useCashe, keyName, useVPDB);
            ScopeLoadAssm.End();
            return ret;
        }
        public VAssembly LoadAssembly_Impl(string assemblyAbsFile, EngineNS.ECSType csType, bool fromBytes=false, bool useCashe = true, string keyName = "", bool useVPDB = true)
        {
            if (CEngine.Instance!=null && !CEngine.Instance.FileManager.FileExists(assemblyAbsFile))
                return null;

            assemblyAbsFile = assemblyAbsFile.Replace("\\", "/");

            if (useCashe)
            {
                VAssembly retValue;
                if (mAssembly_AbsFileDic.TryGetValue(assemblyAbsFile, out retValue))
                    return retValue;

                var assemblys = System.AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblys.Length; i++)
                {
                    if (assemblys[i].IsDynamic)
                        continue;

                    var path = assemblys[i].Location.Replace("\\", "/");
                    if (string.Equals(path, assemblyAbsFile))
                    {
                        var retAs = GetAssembly(assemblys[i], csType, keyName);
                        if (!string.IsNullOrEmpty(keyName))
                            retAs.KeyName = keyName;
                        return retAs;
                    }
                }
            }

            {
                try
                {
                    var assemblyBytes = IO.FileManager.ReadFile(assemblyAbsFile);
                    if (assemblyBytes == null)
                        return null;

                    System.Reflection.Assembly assembly = null;
                    if(fromBytes)
                    {
                        string pdbExt = ".pdb";
                        if(useVPDB)
                            pdbExt = ".vpdb";
                        var pdbFile = assemblyAbsFile.Substring(0, assemblyAbsFile.Length-4) + pdbExt;
                        byte[] pdbBytes = null;
                        if(CEngine.Instance.FileManager.FileExists(pdbFile))
                        {
                            pdbBytes = IO.FileManager.ReadFile(pdbFile);
                        }
                    
                        assembly = System.Reflection.Assembly.Load(assemblyBytes, pdbBytes);
                    }
                    else
                    {
                        assembly = System.Reflection.Assembly.LoadFrom(assemblyAbsFile);
                    }
                    var vAssem = new VAssembly(assembly, csType);
                    mAssembly_FullNameDic[assembly.FullName] = vAssem;
                    mAssembly_AbsFileDic[assemblyAbsFile] = vAssem;
                    if (string.IsNullOrEmpty(keyName))
                        vAssem.KeyName = assembly.GetName().Name;
                    else
                        vAssem.KeyName = keyName;
                    return vAssem;
                }
                catch (Exception e)
                {
                    EngineNS.Profiler.Log.WriteException(e);
                    //System.Diagnostics.Trace.WriteLine("VAssemblyManager.LoadAssembly\r\n" + e.ToString());
                }
            }

            return null;
        }
    }
}
