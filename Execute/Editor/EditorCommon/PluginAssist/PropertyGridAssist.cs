using System.Collections.Generic;

namespace EditorCommon.PluginAssist
{
    public class PropertyGridAssist
    {
        static Dictionary<string, object[]> SelectedObjectDataDic = new Dictionary<string, object[]>();
        static Dictionary<string, EditorCommon.Resources.ResourceInfo> SelectedResourceInfoDic = new Dictionary<string, EditorCommon.Resources.ResourceInfo>();

        public static void SetSelectedResourceInfo(string key, EditorCommon.Resources.ResourceInfo resInfo)
        {
            SelectedResourceInfoDic[key] = resInfo;
        }
        public static EditorCommon.Resources.ResourceInfo GetSelectedResourceInfo(string key)
        {
            EditorCommon.Resources.ResourceInfo resInfo = null;
            SelectedResourceInfoDic.TryGetValue(key, out resInfo);
            return resInfo;
        }
        public static void SetSelectedObjectData(string key, object[] data)
        {
            if (key == "Action" || key == "Blend_Action1D" || key == "Blend_Action2D")
                AddAction(key, data);
            else
                SelectedObjectDataDic[key] = data;
        }
        public static object[] GetSelectedObjectData(string key)
        {
            object[] outValue;
            if (key == "Action" || key == "Blend_Action1D" || key == "Blend_Action2D")
                outValue = GetAction(key);
            else
                SelectedObjectDataDic.TryGetValue(key, out outValue);

            return outValue;
        }
        static void AddAction(string key, object[] data)
        {
            if (SelectedObjectDataDic.ContainsKey("Action"))
                SelectedObjectDataDic.Remove("Action");
            if (SelectedObjectDataDic.ContainsKey("Blend_Action1D"))
                SelectedObjectDataDic.Remove("Blend_Action1D");
            if (SelectedObjectDataDic.ContainsKey("Blend_Action2D"))
                SelectedObjectDataDic.Remove("Blend_Action2D");
            SelectedObjectDataDic[key] = data;
        }
        static object[] GetAction(string key)
        {
            object[] outValue;
            if (SelectedObjectDataDic.TryGetValue("Action", out outValue))
            {
                return outValue;
            }
            if (SelectedObjectDataDic.TryGetValue("Blend_Action1D", out outValue))
            {
                return outValue;
            }
            if (SelectedObjectDataDic.TryGetValue("Blend_Action2D", out outValue))
            {
                return outValue;
            }
            return null;
        }
    }
}
