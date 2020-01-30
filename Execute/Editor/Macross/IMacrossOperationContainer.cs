using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macross
{
    public interface ILocalVaiableProcesser
    {
        void ShowLocalVariables();
        void HideLocalVariables();
    }
    public interface INodesContainerDicKey
    {
        EngineNS.ECSType CSType { get; }
        Guid Id { get; }
        string Name { get; }
        string ShowName { get; }
        CategoryItem.enCategoryItemType CategoryItemType { get; }

        void ProcessOnNodesContainerShow(NodesControlAssist.ProcessData processData);
        void ProcessOnNodesContainerHide(NodesControlAssist.ProcessData processData);
    }
    public interface IMacrossOperationContainer
    {
        NodesControlAssist NodesCtrlAssist { get; }
        MacrossPanelBase MacrossOpPanel { get; }
        EngineNS.ECSType CSType { get; set; }
        string UndoRedoKey { get; }
        Macross.ResourceInfos.MacrossResourceInfo CurrentResourceInfo { get; set; }
        void OnSelectedLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo);
        void OnDoubleCliclLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo);
        void OnSelectNodeControl(CodeGenerateSystem.Base.BaseNodeControl node);
        void OnSelectNull(CodeGenerateSystem.Base.BaseNodeControl node);
        void OnUnSelectNodes(List<CodeGenerateSystem.Base.BaseNodeControl> nodes);
        void ShowItemPropertys(object itme);
        void RemoveOverrideMethod(string methodKeyName);
        string GetGraphFileName(string graphName);
        Task BindMainGrid(CategoryItem key);
        Task<NodesControlAssist> CreateNodesContainer(INodesContainerDicKey graphKey,bool IsShow = true);
        Task<NodesControlAssist> ShowNodesContainer(INodesContainerDicKey graphKey);
        Task<NodesControlAssist> GetNodesContainer(INodesContainerDicKey key, bool createAndLoadWhenNotFound,bool loadAll = false);
        Task<NodesControlAssist> GetNodesContainer(Guid containerId, bool createAndLoadWhenNotFound, bool loadAll = false);
        Macross.INodesContainerDicKey GetNodesContainerKey(Guid keyId);
        bool RemoveNodesContainer(INodesContainerDicKey key);
        CodeGenerateSystem.Base.BaseNodeControl FindControl(Guid id);
        void DeleteNode(CodeGenerateSystem.Base.BaseNodeControl node);
        void GenerateClassDefaultValues(System.CodeDom.CodeStatementCollection codeStatementCollection);
    }

}
