using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.IO
{
    public class XmlSDK
    {
        const string ModuleNC = CoreObjectBase.ModuleNC;
        //RapidXML
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XmlHolder.NativePointer RapidXml_LoadFileA(string filename);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static void RapidXmlA_Delete(XmlHolder.NativePointer p);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static CStringObject.NativePointer RapidXmlA_GetXMLString(XmlHolder.NativePointer p);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static XmlNode.NativePointer RapidXmlA_RootNode(XmlHolder.NativePointer p);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static XmlHolder.NativePointer RapidXmlA_NewXmlHolder();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr RapidXmlA_append_node(XmlHolder.NativePointer xmlHolder, XmlNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XmlHolder.NativePointer RapidXmlA_ParseXML(string xmlString);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr RapidXmlA_GetStringFromXML(XmlHolder.NativePointer xmlHolder);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void RapidXmlA_FreeString(IntPtr str);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void RapidXmlA_SaveXML(XmlHolder.NativePointer xmlHolder, string fileName);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XmlNode.NativePointer RapidXmlNodeA_allocate_node(XmlHolder.NativePointer xmlHolder, string name, string value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XmlAttrib.NativePointer RapidXmlNodeA_allocate_attribute(XmlHolder.NativePointer xmlHolder, string name, string value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void RapidXmlNodeA_append_node(XmlNode.NativePointer node, XmlNode.NativePointer childNode);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void RapidXmlNodeA_append_attribute(XmlNode.NativePointer node, XmlAttrib.NativePointer childAttr);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void RapidXmlNodeA_remove_node(XmlNode.NativePointer xmlNode, XmlNode.NativePointer childXmlNode);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void RapidXmlNodeA_remove_attribute(XmlNode.NativePointer xmlNode, XmlAttrib.NativePointer childXmlAttr);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XmlNode.NativePointer RapidXmlNodeA_first_node(XmlNode.NativePointer node, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XmlAttrib.NativePointer RapidXmlNodeA_first_attribute(XmlNode.NativePointer node, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XmlNode.NativePointer RapidXmlNodeA_next_sibling(XmlNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr RapidXmlNodeA_name(XmlNode.NativePointer node, ref int pNeedFreeStr);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr RapidXmlNodeA_value(XmlNode.NativePointer node, ref int pNeedFreeStr);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr RapidXmlNodeA_GetStringFromNode(XmlNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void RapidXmlNodeA_FreeString(IntPtr str);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr RapidXmlAttribA_name(XmlAttrib.NativePointer attr, ref int pNeedFreeStr);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr RapidXmlAttribA_value(XmlAttrib.NativePointer attr, ref int pNeedFreeStr);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XmlAttrib.NativePointer RapidXmlAttribA_next_sibling(XmlAttrib.NativePointer attr);
    }
    public interface IXmlElement
    {
        string Name
        {
            get;
        }
        string Value
        {
            get;
        }
    }
    public class XmlAttrib : IXmlElement
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        NativePointer mCoreObject;
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }

        public XmlAttrib(NativePointer self)
        {
            mCoreObject = self;
        }
        ~XmlAttrib()
        {
            mCoreObject.Pointer = IntPtr.Zero;
        }

        public string Name
        {
            get
            {
                unsafe
                {
                    int pNeedFreeStr = 0;
                    var retName = XmlSDK.RapidXmlAttribA_name(mCoreObject, ref pNeedFreeStr);
                    string str = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(retName);
                    if (pNeedFreeStr == 1)
                        XmlSDK.RapidXmlNodeA_FreeString(retName);
                    return str;
                }
            }
        }

        public string Value
        {
            get
            {
                unsafe
                {
                    int pNeedFreeStr = 0;
                    var retName = XmlSDK.RapidXmlAttribA_value(mCoreObject, ref pNeedFreeStr);
                    string str = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(retName);
                    if (pNeedFreeStr == 1)
                        XmlSDK.RapidXmlNodeA_FreeString(retName);
                    return str;
                }
            }
        }
    }
    public class XmlNode : IXmlElement
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        NativePointer mCoreObject;
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }

        public string Name
        {
            get
            {
                unsafe
                {
                    int pNeedFreeStr = 0;
                    var retName = XmlSDK.RapidXmlNodeA_name(CoreObject, ref pNeedFreeStr);
                    string str = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(retName);
                    if (pNeedFreeStr == 1)
                        XmlSDK.RapidXmlNodeA_FreeString(retName);
                    return str;
                }
            }
        }

        public string Value
        {
            get
            {
                unsafe
                {
                    int pNeedFreeStr = 0;
                    var retName = XmlSDK.RapidXmlNodeA_value(CoreObject, ref pNeedFreeStr);
                    string str = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(retName);
                    if (pNeedFreeStr == 1)
                        XmlSDK.RapidXmlNodeA_FreeString(retName);
                    return str;
                }
            }
        }

        public XmlHolder mHolder = null;

        public XmlNode(XmlNode.NativePointer node)
        {
            mCoreObject = node;
        }

        ~XmlNode()
        {
            mCoreObject.Pointer = IntPtr.Zero;
        }

        public delegate bool FNodeFinderCondition(XmlNode node, ref bool bCancel);
        public List<XmlNode> FindNodes(string name, FNodeFinderCondition condition = null)
        {
            unsafe
            {
                List<XmlNode> nodeList = new List<XmlNode>();

                if (mCoreObject.Pointer == IntPtr.Zero)
                    return nodeList;

                XmlNode.NativePointer node = XmlSDK.RapidXmlNodeA_first_node(mCoreObject, name);
 
                while (mCoreObject.Pointer != IntPtr.Zero && node.Pointer != IntPtr.Zero)
                {
                    int pNeedFreeStr = 0;
                    var strPtr = XmlSDK.RapidXmlNodeA_name(node, ref pNeedFreeStr);
                    var nodeName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(strPtr);
                    if (pNeedFreeStr == 1)
                        XmlSDK.RapidXmlNodeA_FreeString(strPtr);
                    if (name == nodeName)
                    {
                        XmlNode nd = new XmlNode(node);
                        nd.mHolder = mHolder;

                        bool bCancel = false;
                        if (condition != null)
                        {
                            if (condition(nd, ref bCancel))
                                nodeList.Add(nd);
                        }
                        else
                        {
                            nodeList.Add(nd);
                        }
                        if (bCancel)
                            break;
                    }
                    node = XmlSDK.RapidXmlNodeA_next_sibling(node);
                }

                return nodeList;
            }
        }
        public delegate bool FOnFindNode(XmlNode node);
        public XmlNode FindNode(FOnFindNode onFind)
        {
            unsafe
            {
                if (CoreObject.Pointer == IntPtr.Zero)
                    return null;

                var node = XmlSDK.RapidXmlNodeA_first_node(CoreObject, null);
                while (node.Pointer != IntPtr.Zero)
                {
                    var nd = new XmlNode(node);
                    nd.mHolder = mHolder;
                    if (onFind(nd))
                        return nd;
                    node = XmlSDK.RapidXmlNodeA_next_sibling(node);
                }
                return null;
            }
        }
        public XmlNode FindNode(string name)
        {
            unsafe
            {
                if (CoreObject.Pointer == IntPtr.Zero)
                    return null;

                var node = XmlSDK.RapidXmlNodeA_first_node(CoreObject, name);
                while (node.Pointer != IntPtr.Zero)
                {
                    int pNeedFreeStr = 0;
                    var strPtr = XmlSDK.RapidXmlNodeA_name(node, ref pNeedFreeStr);
                    var nodeName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(strPtr);
                    if (pNeedFreeStr == 1)
                        XmlSDK.RapidXmlNodeA_FreeString(strPtr);
                    if (name == nodeName)
                    {
                        var nd = new XmlNode(node);
                        nd.mHolder = mHolder;
                        return nd;
                    }
                    node = XmlSDK.RapidXmlNodeA_next_sibling(node);
                }

                return null;
            }
        }

        public XmlAttrib FindAttrib(string name)
        {
            unsafe
            {
                if (CoreObject.Pointer == IntPtr.Zero)
                    return null;

                var attr = XmlSDK.RapidXmlNodeA_first_attribute(CoreObject, name);
                if (attr.Pointer == IntPtr.Zero)
                    return null;

                return new XmlAttrib(attr);
            }
        }

        public List<XmlNode> GetNodes()
        {
            unsafe
            {
                var nodeList = new List<XmlNode>();

                if (CoreObject.Pointer == IntPtr.Zero)
                    return nodeList;

                for (var node = XmlSDK.RapidXmlNodeA_first_node(CoreObject, null);
                    node.Pointer != IntPtr.Zero;
                    node = XmlSDK.RapidXmlNodeA_next_sibling(node))
                {
                    var nd = new XmlNode(node);
                    nd.mHolder = mHolder;
                    nodeList.Add(nd);
                }

                return nodeList;
            }
        }

        public List<XmlAttrib> GetAttribs()
        {
            unsafe
            {
                var attrList = new List<XmlAttrib>();

                if (CoreObject.Pointer == IntPtr.Zero)
                    return attrList;

                for (var attr = XmlSDK.RapidXmlNodeA_first_attribute(CoreObject, null);
                    attr.Pointer != IntPtr.Zero;
                    attr = XmlSDK.RapidXmlAttribA_next_sibling(attr))
                {
                    var nd = new XmlAttrib(attr);
                    attrList.Add(nd);
                }

                return attrList;
            }
        }

        public XmlNode AddNode(string name, string value, XmlHolder holder)
        {
            unsafe
            {
                if (CoreObject.Pointer == IntPtr.Zero)
                    return null;

                var node = XmlSDK.RapidXmlNodeA_allocate_node(holder.CoreObject, name, value);
                XmlSDK.RapidXmlNodeA_append_node(CoreObject, node);
                var result = new XmlNode(node);
                if (holder == null)
                    result.mHolder = mHolder;
                else
                    result.mHolder = holder;
                return result;
            }
        }

        public XmlAttrib AddAttrib(string name, string value)
        {
            unsafe
            {
                //if (name == null)
                //    name = "";
                //if (value == null)
                //    value = "";

                var attr = XmlSDK.RapidXmlNodeA_allocate_attribute(mHolder.CoreObject, name, value);
                XmlSDK.RapidXmlNodeA_append_attribute(CoreObject, attr);
                return new XmlAttrib(attr);
            }
        }

        public XmlAttrib AddAttrib(string name)
        {
            unsafe
            {
                var attr = XmlSDK.RapidXmlNodeA_allocate_attribute(mHolder.CoreObject, name, "");
                XmlSDK.RapidXmlNodeA_append_attribute(CoreObject, attr);
                return new XmlAttrib(attr);
            }
        }

        public void RemoveNode(XmlNode node)
        {
            XmlSDK.RapidXmlNodeA_remove_node(CoreObject, node.CoreObject);
        }

        public void RemoveAttrib(XmlAttrib attr)
        {
            XmlSDK.RapidXmlNodeA_remove_attribute(CoreObject, attr.CoreObject);
        }
    }

    [EngineNS.Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class XmlHolder : IDisposable
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        NativePointer mCoreObject;
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }

        public XmlHolder()
        {

        }
        ~XmlHolder()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (CoreObject.Pointer != IntPtr.Zero)
            {
                XmlSDK.RapidXmlA_Delete(CoreObject);
                mCoreObject.Pointer = IntPtr.Zero;
            }
        }

        public XmlNode RootNode
        {
            get
            {
                if (CoreObject.Pointer == IntPtr.Zero)
                    return null;
                unsafe
                {
                    var node = XmlSDK.RapidXmlA_RootNode(CoreObject);
                    if (node.Pointer == IntPtr.Zero)
                        return null;

                    var result = new XmlNode(node);
                    result.mHolder = this;
                    return result;
                }
            }
        }

        public string GetTextString()
        {
            var ptr = XmlSDK.RapidXmlA_GetXMLString(mCoreObject);
            var cstr = new CStringObject(ptr);
            return cstr.Text;
        }
        public static XmlHolder LoadXML(System.String file)
        {
            unsafe
            {
                // 计算绝对路径
                file = CEngine.Instance.FileManager._GetAbsPathFromRelativePath(file);
                //IntPtr strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(file);
                //file = file.ToLower();
                var pHolder = XmlSDK.RapidXml_LoadFileA(file);
                if (pHolder.Pointer == IntPtr.Zero)
                    return null;

                var holder = new XmlHolder();
                holder.mCoreObject = pHolder;
                return holder;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static IO.Serializer.Serializer CreateObjectFromXML([EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Describe)]RName file)
        {
            using (var xml = LoadXML(file.Address))
            {
                if (xml == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"LoadXML failed {file}");
                    return null;
                }
                var type = Rtti.RttiHelper.GetTypeFromSaveString(xml.RootNode.Name);
                if (type == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"LoadObjectFromXML failed Type = {xml.RootNode.Name}");
                    return null;
                }
                var obj = System.Activator.CreateInstance(type) as IO.Serializer.Serializer;
                if (obj == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"LoadObjectFromXML CreateInstance failed Type = {xml.RootNode.Name}");
                    return null;
                }
                obj.ReadObjectXML(xml.RootNode);
                return obj;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static IO.Serializer.Serializer LoadObjectFromXML([EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Describe)]RName file, IO.Serializer.Serializer obj)
        {
            using (var xml = LoadXML(file.Address))
            {
                if (xml == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"LoadXML failed {file}");
                    return null;
                }
                if (obj == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"LoadXML ob == null {file}");
                    return null;
                }
                obj.ReadObjectXML(xml.RootNode);
                return obj;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static void SaveObjectToXML(IO.Serializer.Serializer obj, [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Describe)]RName file)
        {
            var typeName = Rtti.RttiHelper.GetTypeSaveString(obj.GetType());
            var saver = IO.XmlHolder.NewXMLHolder(typeName, "");
            obj.WriteObjectXML(saver.RootNode);
            IO.XmlHolder.SaveXML(file.Address, saver);
        }
        public static void SaveXML(System.String file, XmlHolder node, bool bClearStrPtr = true)
        {
            // 计算绝对路径
            var fileName = CEngine.Instance.FileManager._GetAbsPathFromRelativePath(file);

            unsafe
            {
                XmlSDK.RapidXmlA_SaveXML(node.CoreObject, fileName);
            }
        }
        public static XmlHolder NewXMLHolder(System.String name, System.String value)
        {
            unsafe
            {
                var xmlHolder = new XmlHolder();
                xmlHolder.mCoreObject = XmlSDK.RapidXmlA_NewXmlHolder();

                var root = XmlSDK.RapidXmlNodeA_allocate_node(xmlHolder.CoreObject, name, "");
                XmlSDK.RapidXmlA_append_node(xmlHolder.CoreObject, root);
                return xmlHolder;
            }
        }

        public static XmlHolder ParseXML(System.String xmlString)
        {
            var pHolder = XmlSDK.RapidXmlA_ParseXML(xmlString);
            if (pHolder.Pointer == IntPtr.Zero)
                return null;

            var xmlHolder = new XmlHolder();
            xmlHolder.mCoreObject = pHolder;
            return xmlHolder;
        }
        public static void GetXMLString(ref string xmlStr, XmlHolder node)
        {
            unsafe
            {
                var str = XmlSDK.RapidXmlA_GetStringFromXML(node.CoreObject);
                xmlStr = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(str);
                XmlSDK.RapidXmlA_FreeString(str);
            }
        }
        public static void GetXMLStringFromNode(ref string xmlStr, XmlNode node)
        {
            unsafe
            {
                var str = XmlSDK.RapidXmlNodeA_GetStringFromNode(node.CoreObject);
                xmlStr = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(str);
                XmlSDK.RapidXmlNodeA_FreeString(str);
            }
        }
    }
}
