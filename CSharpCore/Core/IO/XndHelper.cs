using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public class XndHelper
    {
        public static XndNode Copy2WritableNode(XndNode src)
        {
            var dst = new XndNode();
            dst.SetName(src.GetName());
            var attrs = src.GetAttribs();
            foreach(var i in attrs)
            {
                var att = dst.AddAttrib(i.GetName());
                CopyAttrib(att, i);
            }

            var nodes = src.GetNodes();
            foreach(var i in nodes)
            {
                var node = Copy2WritableNode(i);
                dst.AddNode(node);
            }
            return dst;
        }
        public delegate void FDoConflict(XndAttrib lh, XndAttrib rh);
        public static bool CopyAttrib(XndAttrib lh, XndAttrib rh)
        {
            if (lh.IsWritable == false)
                return false;
            lh.BeginWrite();
            var data = new byte[rh.Length];
            rh.BeginRead();

            unsafe
            {
                fixed (byte* ptr = &data[0])
                {
                    rh.Read((IntPtr)ptr, (int)data.Length);
                    lh.Write((IntPtr)ptr, (int)data.Length);
                }
            }

            rh.EndRead();
            lh.EndWrite();
            return true;
        }
        public static bool MergeNode(XndNode lh, XndNode rh, FDoConflict DoConflict)
        {
            if (lh.IsWritable == false)
                return false;
            var atts = rh.GetAttribs();
            foreach(var i in atts)
            {
                var att = lh.FindAttrib(i.GetName());
                if(att!=null)
                {
                    DoConflict(att, i);
                }
                else
                {
                    att = lh.AddAttrib(i.GetName());
                    CopyAttrib(att, i);
                }
            }
            var nodes = rh.GetNodes();
            foreach(var i in nodes)
            {
                var node = lh.FindNode(i.GetName());
                if(node==null)
                {
                    node = lh.AddNode(i.GetName(), (long)i.GetClassId(), 0);
                }
                MergeNode(node, i, DoConflict);
            }
            return true;
        }
    }
}
