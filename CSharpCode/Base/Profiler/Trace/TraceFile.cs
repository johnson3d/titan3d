using System;
using System.Collections.Generic;

namespace EngineNS.Profiler.Trace
{
    /// <summary>
    /// .ttrace 文件格式: 文件头(格式版本/帧协议版本) + 名字表完整快照 + 帧数 + 逐帧 TtFrame。
    ///
    /// 引擎侧用 TtTraceFileWriter 边产生边写(对齐 UE 的 trace writer, 不缓存整段);
    /// Tracer 侧用 Save/Load 做整段读写。两边共用同一份格式与同一对 Write/Read,
    /// 所以引擎录制出来的文件 tracer 能直接打开, 反之亦然。
    /// </summary>
    public static class TtTraceFile
    {
        public const uint Magic = 0x54525454;   // "TRTT"
        public const int Version = 1;
        public const string Extension = ".ttrace";

        /// <summary>录制文件统一放 cache/tracer 下: cache 根目录还混着 PSO/shader/snapshot 等大量目录</summary>
        public static string GetRecordDir()
        {
            var fm = TtEngine.Instance?.FileManager;
            string root = fm != null ? fm.GetRoot(IO.TtFileManager.ERootDir.Cache) : "./cache/";
            var dir = IO.TtFileManager.CombinePath(root, "tracer/");
            IO.TtFileManager.SureDirectory(dir);
            return dir;
        }

        public static string MakePath()
        {
            // 带毫秒: 同一秒点两次开始/结束不能互相覆盖
            return IO.TtFileManager.CombinePath(GetRecordDir(), $"trace_{DateTime.Now:yyyyMMdd_HHmmss_fff}" + Extension);
        }

        public static bool Save(string path, IList<TtFrame> frames, out string err)
        {
            err = null;
            try
            {
                using (var file = new IO.TtFileWriter(path))
                {
                    using (var writer = new IO.AuxWriter<IO.TtFileWriter>(file))
                    {
                        writer.Write(Magic);
                        writer.Write(Version);
                        writer.Write(TtFrame.ProtocolVersion);

                        int nameCount = TtTraceNameTable.Count;
                        writer.Write(nameCount);
                        for (int i = 0; i < nameCount; i++)
                            writer.Write(TtTraceNameTable.GetName(i));

                        writer.Write(frames.Count);
                        for (int i = 0; i < frames.Count; i++)
                            frames[i].Write(writer);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                err = e.Message;
                return false;
            }
        }

        public static unsafe bool Load(string path, out List<TtFrame> frames, out string err)
        {
            frames = new List<TtFrame>();
            err = null;
            byte[] data;
            try
            {
                data = System.IO.File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                err = e.Message;
                return false;
            }
            if (data.Length < 24)
            {
                err = "文件太小, 不是 ttrace";
                return false;
            }

            fixed (byte* p = data)
            {
                using (var reader = IO.TtMemReader.CreateInstance(p, (ulong)data.Length))
                {
                    using (var ar = new IO.AuxReader<IO.TtMemReader>(reader, null))
                    {
                        uint magic;
                        ar.Read(out magic);
                        if (magic != Magic)
                        {
                            err = "不是 ttrace 文件";
                            return false;
                        }
                        int version;
                        ar.Read(out version);
                        if (version != Version)
                        {
                            err = $"文件格式版本不匹配: 文件 {version}, 当前 {Version}";
                            return false;
                        }
                        int proto;
                        ar.Read(out proto);
                        if (proto != TtFrame.ProtocolVersion)
                        {
                            // 帧协议加过字段时, 老文件按新结构读会整段错位, 不如直接拒绝
                            err = $"帧协议版本不匹配: 文件 {proto}, 当前 {TtFrame.ProtocolVersion}";
                            return false;
                        }

                        int nameCount;
                        ar.Read(out nameCount);
                        if (nameCount < 0 || nameCount > (1 << 22))
                        {
                            err = $"名字表数量异常: {nameCount}";
                            return false;
                        }
                        for (int i = 0; i < nameCount; i++)
                        {
                            ar.Read(out string name);
                            TtTraceNameTable.SetRemote(i, name);
                        }

                        int frameCount;
                        ar.Read(out frameCount);
                        if (frameCount < 0 || frameCount > (1 << 22))
                        {
                            err = $"帧数量异常: {frameCount}";
                            return false;
                        }
                        for (int i = 0; i < frameCount; i++)
                        {
                            var frame = new TtFrame();
                            frame.Read(ar);
                            if (frame.VersionMismatch || frame.Broken)
                            {
                                // 后面的字节已经不可信, 保留读到这里的部分让用户至少能看前半段
                                err = $"第 {i} 帧数据损坏, 已加载前 {frames.Count} 帧";
                                break;
                            }
                            frames.Add(frame);
                        }
                    }
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 引擎侧流式写入器: 帧 Ready 一帧就写一帧, 内存里只挂队列不攒历史。
    ///
    /// 名字增量游标是独立的(mNameCount 从文件头快照的末尾开始), 与网络发送的
    /// LastSentNameCount 互不干扰 —— 两个消费者各自知道自己写到第几个名字了。
    /// 代价是 AttachNameDelta 会覆盖帧对象上的 NewNames, 所以必须先写网络再写文件。
    /// </summary>
    public class TtTraceFileWriter
    {
        IO.TtFileWriter mFile;
        // 装箱存一次: AuxWriter 是 struct, 存成接口避免每次 Write 都复制/装箱
        IO.IWriter mWriter;
        long mFrameCountOffset;
        int mFrameCount;
        int mNameCount;

        public string Path { get; private set; }
        public int FrameCount { get { return mFrameCount; } }
        public bool IsOpen { get { return mFile != null; } }

        public bool Start(string path, out string err)
        {
            err = null;
            Stop();
            try
            {
                mFile = new IO.TtFileWriter(path);
                mWriter = new IO.AuxWriter<IO.TtFileWriter>(mFile);
                Path = path;
                mFrameCount = 0;

                mWriter.Write(TtTraceFile.Magic);
                mWriter.Write(TtTraceFile.Version);
                mWriter.Write(TtFrame.ProtocolVersion);
                mNameCount = TtTraceNameTable.Count;
                mWriter.Write(mNameCount);
                for (int i = 0; i < mNameCount; i++)
                    mWriter.Write(TtTraceNameTable.GetName(i));
                // frameCount 要等录制结束才知道, 先占位, Stop 时 seek 回来填
                mFrameCountOffset = (long)mFile.GetPosition();
                mWriter.Write(0);
                return true;
            }
            catch (Exception e)
            {
                err = e.Message;
                Stop();
                return false;
            }
        }

        public void WriteFrame(TtFrame frame)
        {
            if (mFile == null || frame == null)
                return;
            // 录制期间新出现的名字靠增量补, 文件头只快照了开始那一刻的全表
            TtEventStreamAction.AttachNameDelta(frame, ref mNameCount);
            frame.Write(mWriter);
            mFrameCount++;
        }

        public void Stop()
        {
            if (mFile == null)
                return;
            try
            {
                mFile.Seek((ulong)mFrameCountOffset);
                mWriter.Write(mFrameCount);
            }
            catch
            {
                // 回填失败就等于文件头帧数不对; 内容还在, Load 会按实际字节数读不出多余帧
            }
            // AuxWriter.Dispose 只跑 DisposeAction, 真正 flush/关文件靠 TtFileWriter.Dispose
            mWriter = null;
            mFile.Dispose();
            mFile = null;
        }
    }
}
