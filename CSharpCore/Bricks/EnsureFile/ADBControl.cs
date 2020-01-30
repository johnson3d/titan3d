using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace EnsureFile
{
    public class ADBControl
    {
        public string mADBAddress = "";//"\"C:\\Program Files (x86)\\Android\\android-sdk\\platform-tools\\adb.exe\"";

        public string ADBAddress
        {
            get
            {
                return mADBAddress;
            }
            set
            {

                mADBAddress = "\"" + value.Replace("\\", "/") + "\"";
            }
        }// TODO 先自己测试用的. 不是正常版本值

        const string AndroidSdcard = "/storage/emulated/0/";

        public string PackName
        {
            get;
            set;
        } = AndroidSdcard + "Game/";
        private Process mProcess;
        public ADBControl()
        {
            InitADB();
        }

        public void InitADB()
        {
            mProcess = new Process();
            mProcess.StartInfo.CreateNoWindow = true;
            mProcess.StartInfo.FileName = "cmd.exe";
            mProcess.StartInfo.UseShellExecute = false;
            mProcess.StartInfo.RedirectStandardError = true;
            mProcess.StartInfo.RedirectStandardInput = true;
            mProcess.StartInfo.RedirectStandardOutput = true;
        }

        public string BeginControl()
        {
            if (mProcess == null || ADBAddress.Equals(""))
                return "No ADB Process";
            //System.Diagnostics.Process.Start(startInfo);
            mProcess.Start();
            mProcess.StandardInput.AutoFlush = true;
            string outStr = ReadStandardOutputLine();
            Console.WriteLine(outStr);
            outStr = outStr.Replace(System.IO.Directory.GetCurrentDirectory(), "");
            return outStr;
        }

        public string DoControl(string cmd)
        {
            if (mProcess == null || ADBAddress.Equals(""))
                return "No ADB Process";

            Console.WriteLine("begin cmd");
            mProcess.StandardInput.WriteLine(cmd);
            Console.WriteLine("end cmd");

            string outStr = ReadStandardOutputLine();
            Console.WriteLine(outStr);
            
            outStr = outStr.Replace(cmd, "");
            outStr = outStr.Replace(System.IO.Directory.GetCurrentDirectory(), "");
            return outStr;
        }

        public string EndControl()
        {
            if(mProcess == null || ADBAddress.Equals(""))
                return "No ADB Process";

            mProcess.StandardInput.WriteLine("exit");
            mProcess.WaitForExit(10000);
            string outStr = mProcess.StandardOutput.ReadToEnd();
            Console.WriteLine(outStr);

            if (mProcess.StandardError.EndOfStream == false)
            {
                string error = mProcess.StandardError.ReadToEnd();
                Console.WriteLine(error);
            }

            mProcess.Close();
            //Console.WriteLine(outStr);
            outStr = outStr.Replace(System.IO.Directory.GetCurrentDirectory(), "");
            return outStr;
        }

        private string ReadStandardOutputLine(int sleeptime = 200)
        {
            //if (mProcess.StandardOutput.EndOfStream)
            //    return "";

            //读取信息的时候需要停下 不然读不到全部信息 等待优化TODO
            Thread.Sleep(sleeptime);

            var tmp = new StringBuilder();

            //当下一次读取时，Peek可能为-1，但此时缓冲区其实是有数据的。正常的Read一次之后，Peek就又有效了。
            if (mProcess.StandardOutput.Peek() == -1)
                tmp.Append((char)mProcess.StandardOutput.Read());

            while (mProcess.StandardOutput.Peek() > -1)
            {
                tmp.Append((char)mProcess.StandardOutput.Read());
            }
            return tmp.ToString().ToLower();
        }

        public string[] GetDeviceName()
        {
            BeginControl();
            string result = DoControl(ADBAddress + " shell getprop ro.product.model");
            EndControl();

            result = result.Replace("\r\n", "\n");

            string[] arry = result.Split('\n');
            List<string> info = new List<string>();
            for (int i = arry.Length - 1; i >= 0; i--)
            {
                if (arry[i].Equals(" ") == false && arry[i].Equals("") == false)
                {
                    info.Add(arry[i]);
                }
            }

            return info.ToArray();
        }

        public string[] GetDeviceID()
        {
            BeginControl();
            string result = DoControl(ADBAddress + " devices");
            EndControl();
            result = result.ToLower();
            result = result.Replace("list of devices attached", "");
            result = result.Replace("\r\n", "\n");
            string[] arry = result.Split('\n');
            List<string> info = new List<string>();
            for (int i = arry.Length - 1; i >= 0; i--)
            {
                if (arry[i].Equals(" ") == false && arry[i].Equals("") == false)
                {
                    info.Add(arry[i].Replace("device", ""));
                }
            }

            return info.ToArray();
        }
          
        public void CopyFileToAndroid(string[] files, out string message)
        {
            if (files == null)
            {
                message = "";
                return;
            }

            string copyerror = "";

            //adb push <本地文件路径> <设备中的路径>
            string projectpath = EngineNS.CEngine.Instance.FileManager.ProjectRoot;// + "Execute/Games/Batman/Batman.Droid/";
           
            Console.WriteLine("一共" + files.Length.ToString() + "文件");
            int i = 0;
            foreach (var name in files)
            {
                BeginControl();
                DoControl("setlocal");
                //处理文件名
                string temp = name.ToLower();
                temp = temp.Replace("content", "Content");
                i++;
                string result = DoControl(ADBAddress + " push \"" + projectpath + name + "\" \"" + PackName + temp + "\"");
                EndControl();
                Console.WriteLine(i.ToString() + " push \"" + projectpath + name + "\" \"" + PackName + temp + "\"");
                if ( copyerror.Equals("") && result.IndexOf("error") != -1)
                {
                    copyerror = result;
                    break;
                }
            }

            Console.WriteLine("文件拷贝完成！");
            message = copyerror;

            if (copyerror.Equals(""))
            {
                Console.WriteLine("最后使用新的 assetinfos！");
                //最后使用新的 assetinfos
                projectpath = projectpath + "Execute/Games/Batman/Batman.Droid/";
                string assetlist = "Content/assetinfos.xml";
                BeginControl();
                DoControl("setlocal");
                DoControl(ADBAddress + " push " + projectpath + assetlist + " " + PackName + assetlist);
                EndControl();
                Console.WriteLine(" push " + projectpath + assetlist + " " + PackName + assetlist);
            }
            Console.WriteLine("CopyFileToAndroid Done！");
            
        }

        public void CopyFileFromAndroid(List<string> files)
        {
            if (files == null)
                return;

            //adb pull <设备中的路径> <本地文件路径>
            string projectpath = EngineNS.CEngine.Instance.FileManager.ProjectRoot + "Execute/Games/Batman/Batman.Droid/";
            BeginControl();
            foreach (var name in files)
            {

                DoControl(ADBAddress + " pull " + PackName + name + projectpath + name);
            }
            EndControl();
        }

        public void GetAndroidAssetList()
        {
            string name = EngineNS.CEngine.Instance.FileManager.ProjectRoot + "assetinfos.xml";
            if (System.IO.File.Exists(name))
            {
                System.IO.File.Delete(name);
            }

            //adb pull <设备中的路径> <本地文件路径>
            //string projectpath = EngineNS.CEngine.Instance.FileManager.Root + "Execute/Games/Batman/Batman.Droid/";
            BeginControl();
            DoControl(ADBAddress + " pull " + PackName + "Content/assetinfos.xml " + name);
            EndControl();

            Thread.Sleep(1000);
        }
    }
}
