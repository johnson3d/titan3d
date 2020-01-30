using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.SandBox
{
    public class VirtualFile
    { 
        public string Path
        {
            get;
            set;
        }
        public bool Open()
        {
            return false;
        }
        public void Close()
        {

        }
        public void ReadBuffer(IntPtr ptr, UInt32 size)
        {

        }
    }
    public class VirtualEnviroment
    {
        private Dictionary<string, VirtualFile> AllFiles = new Dictionary<string, VirtualFile>();
        public bool InitEnv(RName name, string privateKey)
        {
            return false;
        }
        public VirtualFile GetFile(string path)
        {
            VirtualFile vf;
            if (AllFiles.TryGetValue(path, out vf) == false)
                return null;
            return vf;
        }
    }
}
