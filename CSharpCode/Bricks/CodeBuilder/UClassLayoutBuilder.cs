using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public class UVarAttribute
    {
        public string Name { get; set; }
        public NxRHI.EShaderVarType Type { get; set; }
        public int Columns { get; set; }
        public uint Offset { get; set; }
    }
    public class UClassLayoutBuilder : Graphics.Pipeline.IGuiModule
    {
        bool mIsStruct = true;
        public bool IsStruct { get => mIsStruct; set => mIsStruct = value; }
        string mTypeName = "BuiltType";
        public string TypeName { get => mTypeName; set => mTypeName = value; }
        public List<UVarAttribute> NamedAttributes { get; } = new List<UVarAttribute>();
        public uint Size;
        public void AddAttribute(string name, NxRHI.EShaderVarType type)
        {
            var tmp = new UVarAttribute();
            tmp.Name = name;
            tmp.Type = type;
            NamedAttributes.Add(tmp);
        }
        public int FindAttribute(string name)
        {
            for (int i = 0; i<NamedAttributes.Count; i++)
            {
                if (NamedAttributes[i].Name == name)
                    return i;
            }
            return -1;
        }
        public UVarAttribute GetAttribute(int index)
        {
            return NamedAttributes[index];
        }
        public void BuildAttributes(uint AlignSize = 16)
        {
            uint packOffset = 0;
            uint offsetTmp = 0;
            for (int i = 0; i < NamedAttributes.Count; i++)
            {
                uint size = 0;
                size = (uint)(CoreSDK.GetShaderVarTypeSize(NamedAttributes[i].Type) * NamedAttributes[i].Columns);

                if (size >= 16)
                {
                    NamedAttributes[i].Offset = packOffset;
                    if (size % AlignSize == 0)
                    {
                        packOffset += size;
                    }
                    else
                    {
                        packOffset += (size / AlignSize + 1) * AlignSize;
                    }
                    offsetTmp = 0;
                }
                else
                {
                    NamedAttributes[i].Offset = packOffset + offsetTmp;
                    if (offsetTmp + size == AlignSize)
                    {
                        packOffset += AlignSize;
                        offsetTmp = 0;
                    }
                    else if (offsetTmp + size < AlignSize)
                    {
                        offsetTmp += size;
                    }
                    else
                    {
                        packOffset += AlignSize;
                        NamedAttributes[i].Offset = packOffset;
                        offsetTmp = 0;
                    }
                }
            }
            Size = packOffset + offsetTmp;
        }

        public EGui.Controls.PropertyGrid.PropertyGrid LayoutPG = new EGui.Controls.PropertyGrid.PropertyGrid();
        public UClassLayoutBuilder()
        {
            LayoutPG.Target = this;
            var noused = LayoutPG.Initialize();
        }
        public void OnDraw()
        {
            //if (ImGuiAPI.Begin(TypeName, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
            //    ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                var sz = new Vector2();
                if (ImGuiAPI.Button("Add", in sz))
                {
                    var tmp = new UVarAttribute();
                    AddAttribute($"Member {NamedAttributes.Count}", NxRHI.EShaderVarType.SVT_Float);
                }
                if (ImGuiAPI.Button("Remove", in sz))
                {

                }

                ImGuiAPI.Separator();

                LayoutPG.OnDraw(false, false, false);

                ImGuiAPI.Separator();
            }
            //ImGuiAPI.End();
        }
    }
}
