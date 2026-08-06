namespace EngineNS.DesignMacross
{
    public interface IOperationCommandData
    {

    }
    /// <summary>
    /// 统一Undo/Redo架构适配器: 旧TtCommandHistory的API保持不变, 内部委托给编辑器级TtEditorHistory,
    /// 使DesignMacross的命令与统一历史栈(工具栏按钮/Ctrl+Z快捷键/History面板)合流。
    /// </summary>
    public class TtCommandHistory
    {
        public EngineNS.Editor.Infrastructure.TtEditorHistory History { get; }
        public TtCommandHistory()
        {
            History = new EngineNS.Editor.Infrastructure.TtEditorHistory();
        }
        public TtCommandHistory(EngineNS.Editor.Infrastructure.TtEditorHistory history)
        {
            History = history ?? new EngineNS.Editor.Infrastructure.TtEditorHistory();
        }
        public void Undo()
        {
            History.Undo();
        }
        public void Redo()
        {
            History.Redo();
        }
        public void CreateAndExtuteCommand(string commandName, Action<IOperationCommandData> doAction, Action<IOperationCommandData> undoAction)
        {
            History.ExecuteCommand(new EngineNS.Editor.Infrastructure.TtDelegateCommand(commandName,
                () => doAction?.Invoke(null),
                () => undoAction?.Invoke(null)));
        }
        public void CreateAndExtuteCommand(string commandName, IOperationCommandData doData, Action<IOperationCommandData> doAction, IOperationCommandData undoData, Action<IOperationCommandData> undoAction)
        {
            History.ExecuteCommand(new EngineNS.Editor.Infrastructure.TtDelegateCommand(commandName,
                () => doAction?.Invoke(doData),
                () => undoAction?.Invoke(undoData)));
        }
        /// <summary>
        /// 操作已在外部执行完毕, 仅入栈记录
        /// </summary>
        public void CreateCommand(string commandName, Action<IOperationCommandData> doAction, Action<IOperationCommandData> undoAction)
        {
            History.PushCommand(new EngineNS.Editor.Infrastructure.TtDelegateCommand(commandName,
                () => doAction?.Invoke(null),
                () => undoAction?.Invoke(null)));
        }
        public void CreateCommand(string commandName, IOperationCommandData doData, Action<IOperationCommandData> doAction, IOperationCommandData undoData, Action<IOperationCommandData> undoAction)
        {
            History.PushCommand(new EngineNS.Editor.Infrastructure.TtDelegateCommand(commandName,
                () => doAction?.Invoke(doData),
                () => undoAction?.Invoke(undoData)));
        }
    }
}
