using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCMD
{
    class AssetsPacker
    {
        public static void PackAList(string src, string tar)
        {
            var fileList = new List<EngineNS.IO.CPakFile.VPakPair>();
            string line;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(src))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    var segs = line.Split(' ');
                    if (segs.Length != 3)
                        continue;

                    uint flags = 0;
                    switch (segs[2].ToLower())
                    {
                        case "normal":
                            flags = 1;
                            break;
                        case "zip":
                            flags = 2;
                            break;
                        default:
                            break;
                    }
                    //生成MD5码
                    string md5String = "";
                    try
                    {
                        var MD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                        System.IO.FileStream fs = new System.IO.FileStream(segs[0], System.IO.FileMode.Open, System.IO.FileAccess.Read);
                        byte[] code = MD5.ComputeHash(fs);
                        md5String = System.Text.Encoding.ASCII.GetString(code);
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        continue;
                    }

                    var vpp = EngineNS.IO.CPakFile.VPakPair.CreatePair(segs[0], segs[1], md5String, flags);
                    fileList.Add(vpp);
                }
            }

            var flArray = fileList.ToArray();
            var ret = EngineNS.IO.CPakFile.BuildPakFile(flArray, tar);

            foreach (var i in fileList)
            {
                i.Dispose();
            }
            fileList.Clear();
        }
    }
}
