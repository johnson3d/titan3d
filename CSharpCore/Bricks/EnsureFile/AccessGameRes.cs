using System;
using System.Collections.Generic;
using System.Text;

namespace EnsureFile
{
    public class AccessGameRes
    {
        static ADBControl _ADBControl;
        static Compare _Compare;
        static CreateGameResInfos _CreateGameResInfos;
        public static void InitADBAddress(string address)
        {
            if (_ADBControl == null)
            {
                _ADBControl = new ADBControl();
            }

            _ADBControl.ADBAddress = address;
        }

        public static void GetNeedFilesInfosByConpare(out string[] NeedFiles)
        {
            if (_Compare == null)
            {
                _Compare = new Compare();
            }

            string sdname = EngineNS.CEngine.Instance.FileManager.ProjectRoot + "assetinfos.xml";
            string name = EngineNS.CEngine.Instance.FileManager.ProjectRoot + "Execute/Games/Batman/Batman.Droid/Content/assetinfos.xml";
            //新的list文件需要下载或者拷贝到SD卡；
            NeedFiles = _Compare.CompareMD5(sdname, name);
        }
        public static void CopyAndoridAsset()
        {
            if (_ADBControl == null)
            {
                _ADBControl = new ADBControl();
            }

            _ADBControl.GetAndroidAssetList();
        }

        public static void CheckAndCreateFirstAssetList()
        {
            if (_CreateGameResInfos == null)
            {
                _CreateGameResInfos = new CreateGameResInfos();
            }
            _CreateGameResInfos.CheckAndCopyAseetinfos();
        }

        public static void UpdateNeedFiles(string[] NeedFiles, out string message)
        {
            if (_ADBControl == null)
            {
                _ADBControl = new ADBControl();
            }
            _ADBControl.CopyFileToAndroid(NeedFiles, out message);
        }

        public static string[] GetDeviceName()
        {
            if (_ADBControl == null)
            {
                _ADBControl = new ADBControl();
            }

            return _ADBControl.GetDeviceName();
        }

        public static string[] GetDeviceID()
        {
            if (_ADBControl == null)
            {
                _ADBControl = new ADBControl();
            }

            return _ADBControl.GetDeviceID();
        }
    }
}
