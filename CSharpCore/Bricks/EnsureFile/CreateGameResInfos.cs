using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnsureFile
{
    public class CreateGameResInfos
    {
        public bool CheckGameResInfosSD()
        {
            string filename = EngineNS.CEngine.Instance.FileManager.ProjectContent + "assetinfos.xml";
            return EngineNS.CEngine.Instance.FileManager.DirectoryExists(filename);
        }

        public void CheckAndCopyAseetinfos()
        {
#if PAndroid
             if (CheckGameResInfosSD())
                return;

            Java.IO.File path = new Java.IO.File(EngineNS.CEngine.Instance.FileManager.ProjectContent);
            if (!path.Exists())
            {
                path.Mkdirs();
            }

            //path.Close();
            string filename = EngineNS.CEngine.Instance.FileManager.ProjectContent + "assetinfos.txt";
            Java.IO.File file = new Java.IO.File(EngineNS.CEngine.Instance.FileManager.ProjectContent, "assetinfos.xml");
            try
            {
                file.CreateNewFile();
                //file.Close();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            Java.IO.FileWriter fw = new Java.IO.FileWriter(EngineNS.CEngine.Instance.FileManager.ProjectContent + "assetinfos.xml");

            byte[] xmlcode = FileManager.ReadFile("Content/assetinfos.xml");
            string str = System.Text.Encoding.ASCII.GetString(xmlcode);
           
            fw.Write(str, 0, str.Length);
            fw.Flush();
            fw.Close();
#endif

        }
    }
}
