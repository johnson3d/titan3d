using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeTools.Macross
{
    class UMacrossContextMenuDefine : UClassCodeBase
    {
        public string[] MenuPaths;
        public string FilterString;
        public string CreateCode;
        public ClassDeclarationSyntax Kls;

        struct GenData
        {
            public int ItemInsertPlace;
        }

        GenData GenMenuPath(int menuPathCount, ref string code, ref int numOfTab)
        {
            var retValue = new GenData();
            var menuPath = string.Join("/", MenuPaths, 0, menuPathCount);
            var mainCheckStr = $"// {menuPath} check insert place";
            var itemInsertStr = $"// {menuPath} insert place";
            retValue.ItemInsertPlace = GetRealIndexOfStr(itemInsertStr, code);
            if (retValue.ItemInsertPlace < 0)
            {
                var valueSetPlace = GetRealIndexOfStr(UMacrossContextMenuManager.FilterTextChangeSetPlace, code);
                var valueTab = GetNumOfTab(valueSetPlace, code);
                var memberInsertPlace = GetRealIndexOfStr("// member insert place", code);
                var memberTab = GetNumOfTab(memberInsertPlace, code);
                var parentMenuName = $"menu_checkVal_{menuPath.Replace("/", "_").Replace(" ", "_")}";
                var valueCode = "";
                UClassCodeBase.AddLine($"menuData.ParentMenuName = \"{menuPath}\";", ref valueCode, in valueTab);
                UClassCodeBase.AddLine($"{parentMenuName} = isVisible?.Invoke(menuData);", ref valueCode, in valueTab);
                code = code.Insert(valueSetPlace, valueCode);
                var valueResetPlace = GetRealIndexOfStr(UMacrossContextMenuManager.FilterTextEmptySetPlace, code);
                valueTab = GetNumOfTab(valueResetPlace, code);
                valueCode = "";
                UClassCodeBase.AddLine($"{parentMenuName} = true;", ref valueCode, in valueTab);
                code = code.Insert(valueResetPlace, valueCode);
                var memCode = "";
                UClassCodeBase.AddLine($"static bool? {parentMenuName} = true;", ref memCode, in memberTab);
                code = code.Insert(memberInsertPlace, memCode);

                if (menuPathCount > 1)
                {
                    retValue = GenMenuPath(menuPathCount - 1, ref code, ref numOfTab);
                }
                string menuParentStr = "";
                var tempValueIdx = 0;
                var tempItemIdx = 0;
                tempValueIdx = menuParentStr.Length;
                UClassCodeBase.AddLine($"if({parentMenuName} != false) {mainCheckStr}", ref menuParentStr, in numOfTab);
                UClassCodeBase.PushBrackets(ref menuParentStr, ref numOfTab);
                {
                    UClassCodeBase.AddLine("EngineNS.ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EngineNS.Bricks.CodeBuilder.MacrossNode.MethodSelector.MethodSelectorStyle.Instance.NameSpaceColor);",
                                           ref menuParentStr, in numOfTab);
                    var varName = (MenuPaths[menuPathCount - 1] + "_show").Replace(" ", "_");
                    UClassCodeBase.AddLine($"var {varName} = EngineNS.ImGuiAPI.TreeNode(\"{MenuPaths[menuPathCount - 1]}\");", ref menuParentStr, in numOfTab);
                    UClassCodeBase.AddLine("EngineNS.ImGuiAPI.PopStyleColor(1);", ref menuParentStr, in numOfTab);
                    UClassCodeBase.AddLine($"if ({varName})", ref menuParentStr, in numOfTab);
                    UClassCodeBase.PushBrackets(ref menuParentStr, ref numOfTab);
                    {
                        tempItemIdx = menuParentStr.Length;
                        UClassCodeBase.AddLine(itemInsertStr + "(" + numOfTab + ")", ref menuParentStr, in numOfTab);
                        UClassCodeBase.AddLine($"menuData.ParentMenuName = \"{menuPath}\";", ref menuParentStr, in numOfTab);
                        UClassCodeBase.AddLine("if(menuNodeShow?.Invoke(menuData) == true)", ref menuParentStr, in numOfTab);
                        UClassCodeBase.PushBrackets(ref menuParentStr, ref numOfTab);
                        {
                            UClassCodeBase.AddLine("retValue = true;", ref menuParentStr, in numOfTab);
                        }
                        UClassCodeBase.PopBrackets(ref menuParentStr, ref numOfTab);
                        UClassCodeBase.AddLine("EngineNS.ImGuiAPI.TreePop();", ref menuParentStr, in numOfTab);
                    }
                    UClassCodeBase.PopBrackets(ref menuParentStr, ref numOfTab);
                }
                UClassCodeBase.PopBrackets(ref menuParentStr, ref numOfTab);
                if (retValue.ItemInsertPlace >= 0)
                {
                    code = code.Insert(retValue.ItemInsertPlace, menuParentStr);
                    retValue.ItemInsertPlace += tempItemIdx;
                }
                else
                {
                    retValue.ItemInsertPlace = code.Length + tempItemIdx;
                    code += menuParentStr;
                }
            }
            else
            {
                var tabStartIdx = code.IndexOf('(', retValue.ItemInsertPlace);
                var tabEndIdx = code.IndexOf(')', tabStartIdx);
                numOfTab = System.Convert.ToInt32(code.Substring(tabStartIdx + 1, tabEndIdx - tabStartIdx - 1));
            }

            return retValue;
        }
        int GetNumOfTab(int startIdx, string code)
        {
            var tabStartIdx = code.IndexOf('(', startIdx);
            var tabEndIdx = code.IndexOf(')', tabStartIdx);
            return System.Convert.ToInt32(code.Substring(tabStartIdx + 1, tabEndIdx - tabStartIdx - 1));
        }
        int GetRealIndexOfStr(string str, string code)
        {
            var idx = code.IndexOf(str);
            if (idx >= 0)
                idx = code.LastIndexOf('\n', idx) + 1;

            return idx;
        }
        static int mSerialId = 0;
        public void GenMenuCode(ref string code, int numOfTab)
        {
            var genData = GenMenuPath(MenuPaths.Length - 1, ref code, ref numOfTab);
            var valueInsertPlace = GetRealIndexOfStr(UMacrossContextMenuManager.FilterTextChangeSetPlace, code);
            var valueResetPlace = GetRealIndexOfStr(UMacrossContextMenuManager.FilterTextEmptySetPlace, code);
            var memberInsertPlace = GetRealIndexOfStr("// member insert place", code);

            var filters = FilterString.Split(',');
            var filterText = "";
            foreach (var f in filters)
            {
                filterText += $"\"{f.ToLower()}\".Contains(menuData.FilterText) || ";
            }
            filterText = filterText.TrimEnd('|', ' ');
            var varName = $"checkVal_{Kls.GetFullName().Replace('.', '_')}_{mSerialId++}";
            //var menuPath = string.Join("/", MenuPaths, 0, MenuPaths.Length - 1);

            // member
            var tempNumofTab = GetNumOfTab(memberInsertPlace, code);
            var memberCode = "";
            UClassCodeBase.AddLine($"static bool {varName} = true;", ref memberCode, in tempNumofTab);

            // variable reset
            tempNumofTab = GetNumOfTab(valueResetPlace, code);
            var varResetCode = "";
            UClassCodeBase.AddLine($"{varName} = true;", ref varResetCode, in tempNumofTab);

            // variable set
            tempNumofTab = GetNumOfTab(valueInsertPlace, code);
            var varCode = "";
            UClassCodeBase.AddLine($"{varName} = {filterText};", ref varCode, in tempNumofTab);
            for (int i = 0; i < MenuPaths.Length - 1; i++)
            {
                var menuPath = "menu_checkVal_" + string.Join("/", MenuPaths, 0, i + 1).Replace(" ", "_").Replace("/", "_");
                UClassCodeBase.AddLine($"{menuPath} = ({menuPath} != false) || {varName};", ref varCode, in tempNumofTab);
            }
            //////UClassCodeBase.AddLine($"{}")

            // node item
            tempNumofTab = GetNumOfTab(genData.ItemInsertPlace, code);
            var menuCode = "";
            UClassCodeBase.AddLine($"if ({varName})", ref menuCode, in tempNumofTab);
            UClassCodeBase.PushBrackets(ref menuCode, ref tempNumofTab);
            {
                UClassCodeBase.AddLine("EngineNS.ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EngineNS.Bricks.CodeBuilder.MacrossNode.MethodSelector.MethodSelectorStyle.Instance.MethodColor);", ref menuCode, in tempNumofTab);
                var nodeName = MenuPaths[MenuPaths.Length - 1];
                var atStartIdx = nodeName.IndexOf('@');
                int atEndIdx = -1;
                var tempNodeName = nodeName;
                var insertStr = "";
                if(atStartIdx >= 0)
                {
                    atEndIdx = nodeName.IndexOf('@', atStartIdx + 1);
                    tempNodeName = nodeName.Remove(atStartIdx, atEndIdx - atStartIdx + 1);
                    insertStr = nodeName.Substring(atStartIdx + 1, atEndIdx - atStartIdx - 1).Replace("serial", "{nodeGraph.GenSerialId()}");
                }
                UClassCodeBase.AddLine($"EngineNS.ImGuiAPI.TreeNodeEx(\"{tempNodeName}\", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet);", ref menuCode, in tempNumofTab);
                UClassCodeBase.AddLine("EngineNS.ImGuiAPI.PopStyleColor(1);", ref menuCode, in tempNumofTab);
                UClassCodeBase.AddLine("if (EngineNS.ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))", ref menuCode, in tempNumofTab);
                UClassCodeBase.PushBrackets(ref menuCode, ref tempNumofTab);
                {
                    if(string.IsNullOrEmpty(CreateCode))
                        UClassCodeBase.AddLine($"var node = new {Kls.GetFullName()}();", ref menuCode, in tempNumofTab);
                    else
                    {
                        var splits = CreateCode.Split('\n');
                        foreach (var split in splits)
                            UClassCodeBase.AddLine(split, ref menuCode, in tempNumofTab);
                    }

                    if (atStartIdx >= 0)
                        nodeName = tempNodeName.Insert(atStartIdx, insertStr);
                    UClassCodeBase.AddLine($"node.Name = $\"{nodeName}\";", ref menuCode, in tempNumofTab);
                    UClassCodeBase.AddLine("node.Graph = macrossEditor;", ref menuCode, in tempNumofTab);
                    UClassCodeBase.AddLine("node.Position = nodeGraph.View2WorldSpace(ref menuData.PosMenu);", ref menuCode, in tempNumofTab);
                    UClassCodeBase.AddLine("nodeGraph.AddNode(node);", ref menuCode, in tempNumofTab);
                    UClassCodeBase.AddLine("retValue = true;", ref menuCode, in tempNumofTab);
                }
                UClassCodeBase.PopBrackets(ref menuCode, ref tempNumofTab);
            }
            UClassCodeBase.PopBrackets(ref menuCode, ref tempNumofTab);
            code = code.Insert(genData.ItemInsertPlace, menuCode);
            code = code.Insert(valueInsertPlace, varCode);
            code = code.Insert(valueResetPlace, varResetCode);
            code = code.Insert(memberInsertPlace, memberCode);
        }
    }

    class UMacrossContextMenuManager : UCodeManagerBase
    {
        public static UMacrossContextMenuManager Instance = new UMacrossContextMenuManager();
        public static readonly string FilterTextEmptySetPlace = "// value reset insert place";
        public static readonly string FilterTextChangeSetPlace = "// value set insert place";

        protected override bool CheckSourceCode(string code)
        {
            if (code.Contains("MacrossContextMenu"))
                return true;
            return false;
        }
        string GetCodeString(ExpressionSyntax syntax)
        {
            if(syntax is BinaryExpressionSyntax)
            {
                var binExp = syntax as BinaryExpressionSyntax;
                var strLeft = GetCodeString(binExp.Left);
                var strRight = GetCodeString(binExp.Right);
                return strLeft + strRight;
            }
            else if(syntax is LiteralExpressionSyntax)
            {
                return ((LiteralExpressionSyntax)syntax).Token.Value.ToString();
            }
            throw new InvalidOperationException($"GetCodeString没有实现{syntax.ToString()}的相关代码！");
        }
        public override void IterateClass(CompilationUnitSyntax root, MemberDeclarationSyntax decl)
        {
            switch(decl.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    {
                        var kls = decl as ClassDeclarationSyntax;
                        var fullname = ClassDeclarationSyntaxExtensions.GetFullName(kls);
                        if(GetClassAttribute(kls))
                        {
                            var klsDefine = FindOrCreate(fullname) as UMacrossContextMenuDefine;
                            klsDefine.Kls = kls;
                            foreach (var i in decl.AttributeLists)
                            {
                                foreach (var j in i.Attributes)
                                {
                                    var attributeName = j.Name.NormalizeWhitespace().ToFullString();
                                    if (attributeName.EndsWith("MacrossContextMenuAttribute") || attributeName.EndsWith("MacrossContextMenu"))
                                    {
                                        var idx = 0;
                                        if(idx < j.ArgumentList.Arguments.Count)
                                            klsDefine.FilterString = ((LiteralExpressionSyntax)j.ArgumentList.Arguments[idx++].Expression).Token.Value.ToString();
                                        if (idx < j.ArgumentList.Arguments.Count)
                                            klsDefine.MenuPaths = ((LiteralExpressionSyntax)j.ArgumentList.Arguments[idx++].Expression).Token.Value.ToString().Split('\\');
                                        if (idx < j.ArgumentList.Arguments.Count)
                                        {
                                            klsDefine.CreateCode = GetCodeString(j.ArgumentList.Arguments[idx++].Expression);
                                        }
                                    }
                                }
                            }
                            foreach (var i in root.Usings)
                            {
                                klsDefine.AddUsing(i.ToString());
                            }
                        }
                    }
                    break;
                case SyntaxKind.NamespaceDeclaration:
                    {
                        var ns = decl as NamespaceDeclarationSyntax;
                        foreach (var i in ns.Members)
                        {
                            IterateClass(root, i);
                        }
                    }
                    break;
            }
        }
        protected override UClassCodeBase CreateClassDefine(string fullname)
        {
            var result = new UMacrossContextMenuDefine();
            result.FullName = fullname;
            return result;
        }
        protected override bool GetClassAttribute(ClassDeclarationSyntax decl)
        {
            foreach(var i in decl.AttributeLists)
            {
                foreach(var j in i.Attributes)
                {
                    var attributeName = j.Name.NormalizeWhitespace().ToFullString();
                    if(attributeName.EndsWith("MacrossContextMenuAttribute") || attributeName.EndsWith("MacrossContextMenu"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public override void WriteCode(string dir)
        {
            int mNumOfTab = 0;
            string classCode = "";
            UClassCodeBase.AddLine("namespace Macross.Generate", ref classCode, in mNumOfTab);
            UClassCodeBase.PushBrackets(ref classCode, ref mNumOfTab);
            {
                UClassCodeBase.AddLine("partial class MacrossContextMenu", ref classCode, in mNumOfTab);
                UClassCodeBase.PushBrackets(ref classCode, ref mNumOfTab);
                {
                    UClassCodeBase.AddLine("/*", ref classCode, in mNumOfTab);///////////////
                    UClassCodeBase.AddLine("static string mFilterTextStore;", ref classCode, in mNumOfTab);
                    UClassCodeBase.AddLine("// member insert place" + "(" + mNumOfTab + ")", ref classCode, in mNumOfTab);

                    var showNodesParams = "EngineNS.Bricks.CodeBuilder.MacrossNode.ClassGraph macrossEditor, ";
                    showNodesParams += "EngineNS.Bricks.CodeBuilder.MacrossContextMenuData menuData, ";
                    showNodesParams += "EngineNS.EGui.Controls.NodeGraph.NodeGraph nodeGraph, ";
                    showNodesParams += "EngineNS.Bricks.CodeBuilder.MacrossContextMenuData.Delegate_MenuIsVisible isVisible, ";
                    showNodesParams += "EngineNS.Bricks.CodeBuilder.MacrossContextMenuData.Delegate_MenuNodeShow menuNodeShow";
                    UClassCodeBase.AddLine($"public static bool ShowNodes({showNodesParams})", ref classCode, in mNumOfTab);
                    UClassCodeBase.PushBrackets(ref classCode, ref mNumOfTab);
                    {
                        UClassCodeBase.AddLine("bool retValue = false;", ref classCode, in mNumOfTab);
                        UClassCodeBase.AddLine("if (mFilterTextStore != menuData.FilterText)", ref classCode, in mNumOfTab);
                        UClassCodeBase.PushBrackets(ref classCode, ref mNumOfTab);
                        {
                            UClassCodeBase.AddLine("mFilterTextStore = menuData.FilterText;", ref classCode, in mNumOfTab);
                            UClassCodeBase.AddLine("if (string.IsNullOrEmpty(menuData.FilterText))", ref classCode, in mNumOfTab);
                            UClassCodeBase.PushBrackets(ref classCode, ref mNumOfTab);
                            {
                                UClassCodeBase.AddLine(UMacrossContextMenuManager.FilterTextEmptySetPlace + "(" + mNumOfTab + ")", ref classCode, in mNumOfTab);
                            }
                            UClassCodeBase.PopBrackets(ref classCode, ref mNumOfTab);
                            UClassCodeBase.AddLine("else", ref classCode, in mNumOfTab);
                            UClassCodeBase.PushBrackets(ref classCode, ref mNumOfTab);
                            {
                                UClassCodeBase.AddLine(UMacrossContextMenuManager.FilterTextChangeSetPlace + "(" + mNumOfTab + ")", ref classCode, in mNumOfTab);
                            }
                            UClassCodeBase.PopBrackets(ref classCode, ref mNumOfTab);

                        }
                        UClassCodeBase.PopBrackets(ref classCode, ref mNumOfTab);

                        foreach (var i in ClassDefines)
                        {
                            ((UMacrossContextMenuDefine)(i.Value)).GenMenuCode(ref classCode, mNumOfTab);
                        }

                        UClassCodeBase.AddLine("menuData.ParentMenuName = \"\";", ref classCode, in mNumOfTab);
                        UClassCodeBase.AddLine("if(menuNodeShow?.Invoke(menuData) == true)", ref classCode, in mNumOfTab);
                        UClassCodeBase.PushBrackets(ref classCode, ref mNumOfTab);
                        {
                            UClassCodeBase.AddLine("retValue = true;", ref classCode, in mNumOfTab);
                        }
                        UClassCodeBase.PopBrackets(ref classCode, ref mNumOfTab);
                        UClassCodeBase.AddLine("return retValue;", ref classCode, in mNumOfTab);
                    }
                    UClassCodeBase.PopBrackets(ref classCode, ref mNumOfTab);
                    UClassCodeBase.AddLine("*/", ref classCode, in mNumOfTab);///////////////
                }
                UClassCodeBase.PopBrackets(ref classCode, ref mNumOfTab);
            }
            UClassCodeBase.PopBrackets(ref classCode, ref mNumOfTab);

            var file = $"{dir}/MacrossContextMenu.macross.cs";
            if(System.IO.File.Exists(file))
            {
                var oldCode = System.IO.File.ReadAllText(file);
                if (oldCode == classCode)
                    return;
            }

            System.IO.File.WriteAllText(file, classCode);
        }
    }
}
