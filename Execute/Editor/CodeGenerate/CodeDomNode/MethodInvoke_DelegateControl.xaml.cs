using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CodeGenerateSystem.Base;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    public sealed partial class MethodInvoke_DelegateControl
    {
        partial void InitConstruction()
        {
            this.InitializeComponent();
            mChildNodeContainer = StackPanel_InputPins;
        }

        private void Button_AddPin_Click(object sender, RoutedEventArgs e)
        {
            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;
            if (param.DelegateMethodInfo == null)
                InitDelegateMethodInfo();

            var paramInfo = new MethodParamInfoAssist();
            paramInfo.FieldDirection = System.CodeDom.FieldDirection.In;
            paramInfo.IsParamsArray = false;
            paramInfo.ParameterType = typeof(object);
            paramInfo.ParamName = "";
            param.InputParmas.Add(paramInfo);

            var inputParam = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = param.CSType,
                HostNodesContainer = param.HostNodesContainer,
                ConstructParam = "",
                ConstructType = MethodInvokeNode.enParamConstructType.Delegate,
                ParamInfo = paramInfo,
            };
            var child = new MethodInvokeParameterControl(inputParam);
            child.RemoveButton.Click += (object removeBtnSender, RoutedEventArgs removeBtnE) =>
            {
                var noUse = RemoveInParam(child);
            };
            AddChildNode(child, StackPanel_InputPins);
        }

        partial void OnLoad_WPF()
        {
            for (int i = 0; i < mChildNodes.Count; i++)
            {
                var paramNode = mChildNodes[i] as MethodInvokeParameterControl;
                if (paramNode == null)
                    continue;

                paramNode.RemoveButton.Click += (object removeBtnSender, RoutedEventArgs removeBtnE) =>
                {
                    var noUse = RemoveInParam(paramNode);
                };
            }
        }
        protected override void CollectionErrorMsg()
        {
            foreach(var child in mChildNodes)
            {
                var ivkNode = child as MethodInvokeParameterControl;
                if(!ivkNode.ParamPin.HasLink)
                {
                    HasError = true;
                    ErrorDescription += "有参数未连接";
                }
            }
        }
        async System.Threading.Tasks.Task RemoveInParam(MethodInvokeParameterControl paramNode)
        {
            // 删除子图中的连线
            await RemoveGraphInParam(paramNode);
            RemoveChildNode(paramNode);
        }

        public async System.Threading.Tasks.Task AddGraphInParam(MethodInvokeParameterControl ctrl, CodeGenerateSystem.Base.LinkInfo info)
        {
            if (HostNodesContainer.IsLoading)
                return;
            if (mLinkedNodesContainer == null)
            {
                await InitializeLinkedNodesContainer();
            }

            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;
            var func = new CustomMethodInfo.FunctionParam();
            func.HostMethodInfo = param.DelegateMethodInfo;
            func.ParamName = info.m_linkFromObjectInfo.HostNodeControl.GCode_GetValueName(info.m_linkFromObjectInfo, null);
            func.ParamType = new VariableType(info.m_linkFromObjectInfo.HostNodeControl.GCode_GetType(info.m_linkFromObjectInfo, null), param.CSType);

            int insertIndex = -1;
            for(int i=0; i<mChildNodes.Count; i++)
            {
                if(mChildNodes[i] == ctrl)
                {
                    insertIndex++;
                    if(insertIndex >= param.InParamIndexes.Count)
                    {
                        insertIndex = -1;

                        param.DelegateMethodInfo.InParams.Add(func);
                        param.InParamIndexes.Add(ctrl.Id);
                        param.DelegateMethodInfo._OnAddedInParam(func);
                    }
                    else
                    {
                        param.DelegateMethodInfo.InParams.Insert(insertIndex, func);
                        param.InParamIndexes.Insert(insertIndex, ctrl.Id);
                        param.DelegateMethodInfo._OnInsertInParam(insertIndex, func);
                    }
                    break;
                }
                else
                {
                    insertIndex = param.InParamIndexes.IndexOf(mChildNodes[i].Id);
                }
            }

            //for(int i=0; i< mLinkedNodesContainer.CtrlNodeList.Count; i++)
            //{
            //    var node = mLinkedNodesContainer.CtrlNodeList[i];
            //    if(node is MethodCustom)
            //    {
            //        var methodNode = node as MethodCustom;
            //        if(insertIndex < 0)
            //            await methodNode.OnAddedInParam(func);
            //        else
            //        {
            //            await methodNode.OnInsertInParam(insertIndex, func);
            //        }
            //        continue;
            //    }
            //    //else if(node is ReturnCustom)
            //    //{
            //    //    var retNode = node as ReturnCustom;
            //    //    retNode.ResetMethodInfo(param.DelegateMethodInfo);
            //    //}
            //}
        }
        public async System.Threading.Tasks.Task RemoveGraphInParam(MethodInvokeParameterControl ctrl)
        {
            if (mLinkedNodesContainer == null)
                await InitializeLinkedNodesContainer();

            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;
            for(int i=param.InParamIndexes.Count-1; i>=0; i--)
            {
                if(param.InParamIndexes[i] == ctrl.Id)
                {
                    var paramIdx = i + param.CustomParamStartIndexInDelegateMethodInfo;
                    var func = param.DelegateMethodInfo.InParams[paramIdx];
                    param.DelegateMethodInfo.InParams.RemoveAt(paramIdx);

                    //for (int nodeIdx = 0; nodeIdx < mLinkedNodesContainer.CtrlNodeList.Count; nodeIdx++)
                    //{
                    //    var node = mLinkedNodesContainer.CtrlNodeList[nodeIdx] as MethodCustom;
                    //    if (node == null)
                    //        continue;

                    //    await node.OnRemovedInParam(paramIdx, func);
                    //}
                    param.InParamIndexes.RemoveAt(i);
                    param.DelegateMethodInfo._OnRemovedInParam(paramIdx, func);
                }
            }
        }

        private void Button_EditGraph_Click(object sender, RoutedEventArgs e)
        {
            var noUse = OpenGraph();
        }
        async System.Threading.Tasks.Task InitializeLinkedNodesContainer()
        {
            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;
            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;

            if(mLinkedNodesContainer == null)
            {
                var data = new CodeGenerateSystem.Base.SubNodesContainerData()
                {
                    ID = Id,
                    Title = HostNodesContainer.TitleString + "/" + param.ParamInfo.ParamName + ":" + this.Id.ToString(),
                };
                mLinkedNodesContainer = await assist.GetSubNodesContainer(data);
                if (!data.IsCreated)
                    return;
            }
            // 读取graph
            var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
            var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(tempFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            bool bLoaded = false;
            if (linkXndHolder != null)
            {
                var linkNode = linkXndHolder.Node.FindNode("SubLinks");
                var idStr = Id.ToString();
                foreach (var node in linkNode.GetNodes())
                {
                    if (node.GetName() == idStr)
                    {
                        await mLinkedNodesContainer.Load(node);
                        bLoaded = true;
                        break;
                    }
                }
            }
            if (bLoaded)
            {
                await ResetNodesMethodInfo();
            }
            else
            {
                InitDelegateMethodInfo();

                var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                {
                    CSType = param.CSType,
                    HostNodesContainer = mLinkedNodesContainer,
                    ConstructParam = "",
                    MethodInfo = param.DelegateMethodInfo,
                    IsShowProperty = false,
                };
                var node = mLinkedNodesContainer.AddOrigionNode(typeof(CodeDomNode.MethodCustom), csParam, 0, 0) as CodeDomNode.MethodCustom;
                node.IsDeleteable = false;
                

                var retCSParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
                {
                    CSType = param.CSType,
                    HostNodesContainer = mLinkedNodesContainer,
                    ConstructParam = "",
                    MethodInfo = param.DelegateMethodInfo,
                    ShowPropertyType = ReturnCustom.ReturnCustomConstructParam.enShowPropertyType.ReturnValue,
                };
                var retNode = mLinkedNodesContainer.AddOrigionNode(typeof(CodeDomNode.ReturnCustom), retCSParam, 300, 0) as CodeDomNode.ReturnCustom;
                retNode.IsDeleteable = false;
            }
        }
        async System.Threading.Tasks.Task OpenGraph()
        {
            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;
            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            var data = new CodeGenerateSystem.Base.SubNodesContainerData()
            {
                ID = Id,
                Title = HostNodesContainer.TitleString + "/" + param.ParamInfo.ParamName + ":" + this.Id.ToString(),
            };
            mLinkedNodesContainer = await assist.ShowSubNodesContainer(data);
            if(data.IsCreated)
            {
                await InitializeLinkedNodesContainer();
            }
            else
            {
                await ResetNodesMethodInfo();
            }
        }
        async System.Threading.Tasks.Task ResetNodesMethodInfo()
        {
            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;

            // 保证methodInfo是同一个地址
            for (int i = 0; i < mLinkedNodesContainer.CtrlNodeList.Count; i++)
            {
                var node = mLinkedNodesContainer.CtrlNodeList[i];
                if (node is MethodCustom)
                {
                    var methodNode = node as MethodCustom;
                    //var nodeParam = methodNode.CSParam as MethodCustom.MethodCustomConstructParam;
                    //nodeParam.MethodInfo = param.DelegateMethodInfo;
                    await methodNode.ResetMethodInfo(param.DelegateMethodInfo);
                }
                else if (node is ReturnCustom)
                {
                    var retNode = node as ReturnCustom;
                    //var nodeParam = retNode.CSParam as ReturnCustom.ReturnCustomConstructParam;
                    //nodeParam.MethodInfo = param.DelegateMethodInfo;
                    await retNode.ResetMethodInfo(param.DelegateMethodInfo);
                }
            }
        }

        public override void Clear()
        {
            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;
            var assist = HostNodesContainer.HostControl as Macross.NodesControlAssist;
            assist.RemoveSubNodesContainer(this.Id);

            base.Clear();
        }

        async System.Threading.Tasks.Task GenerateCode(CodeTypeDeclaration codeClass, CodeGenerateSystem.Base.GenerateCodeContext_Class classContext)
        {
            if (mLinkedNodesContainer == null)
                await InitializeLinkedNodesContainer();

            var methodContext = new CodeGenerateSystem.Base.GenerateCodeContext_Method(classContext, null);
            methodContext.IsDelegateInvokeMethod = true;
            methodContext.DelegateMethodName = MethodName;

            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            Macross.NodesControlAssist.ProcessData processData;
            if(assist.ProcessDataDic.TryGetValue(mLinkedNodesContainer.GUID, out processData))
            {
                methodContext.MethodGenerateData = new CodeGenerateSystem.Base.MethodGenerateData();
                foreach (var item in processData.FunctionVariableCategoryItems)
                {
                    var pro = item.PropertyShowItem as Macross.VariableCategoryItemPropertys;
                    if (pro == null)
                        continue;
                    var paramData = new CodeGenerateSystem.Base.MethodLocalParamData()
                    {
                        ParamType = pro.VariableType.GetActualType(),
                        ParamName = pro.VariableName,
                    };
                    methodContext.MethodGenerateData.LocalParams.Add(paramData);
                }
            }

            foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
            {
                if (ctrl is CodeDomNode.MethodCustom)
                {
                    await ctrl.GCode_CodeDom_GenerateCode(codeClass, null, null, methodContext);
                }
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as MethodInvoke_DelegateControl;
            var copyedNodeParam = copyedNode.CSParam as MethodInvoke_DelegateControlConstructionParams;
            for (int i = 0; i < copyedNode.mChildNodes.Count; i++)
            {
                var paramNode = copyedNode.mChildNodes[i] as MethodInvokeParameterControl;
                if (paramNode == null)
                    continue;

                var paramNodeParam = paramNode.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                paramNodeParam.ParamInfo = copyedNodeParam.InputParmas[i];
                paramNode.ParamPin.SetBinding(CodeGenerateSystem.Controls.LinkInControl.NameStringProperty, new Binding("UIDisplayParamName") { Source = paramNodeParam.ParamInfo });

                paramNode.RemoveButton.Click += (object removeBtnSender, RoutedEventArgs removeBtnE) =>
                {
                    var noUse = copyedNode.RemoveInParam(paramNode);
                };
            }
            return copyedNode;
        }
    }
}
