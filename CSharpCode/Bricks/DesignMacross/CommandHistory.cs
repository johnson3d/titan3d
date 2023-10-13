using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross
{
    public interface IOperationCommandData
    {

    }
    public interface IOperationCommand
    {
        public string Name { get; set; }

        public void UndoOperation();
        public void RedoOperation();
    }
    public class TtOperationCommand : IOperationCommand
    {
        public TtOperationCommand(string commandName, 
            IOperationCommandData redoData, 
            Action<IOperationCommandData> redoAction, 
            IOperationCommandData undoData, 
            Action<IOperationCommandData> undoAction)
        {
            mName = commandName;
            mDoData = redoData;
            mDoAction = redoAction;
            mUndoData = undoData;
            mUndoAction = undoAction;
        }

        IOperationCommandData mDoData;
        public IOperationCommandData DoData { get => mDoData; }
        IOperationCommandData mUndoData;
        public IOperationCommandData UndoData { get => mUndoData; }
        Action<IOperationCommandData> mDoAction;
        public Action<IOperationCommandData> Do { get => mDoAction; }
        Action<IOperationCommandData> mUndoAction;
        public Action<IOperationCommandData> Undo { get => mUndoAction; }
        string mName;
        public string Name 
        { 
            get => mName; 
            set => mName = value;
        }

        public void UndoOperation()
        {
            Undo(UndoData);
        }
        public void RedoOperation()
        {
            Do(DoData);
        }
    }
    public class TtCommandHistory
    {
        Stack<IOperationCommand> DoStack = new Stack<IOperationCommand>();
        Stack<IOperationCommand> UndoStack = new Stack<IOperationCommand>();
        public void Undo()
        {
            if (UndoStack.Count == 0)
                return;

            var command = UndoStack.Pop();
            command.UndoOperation();
            DoStack.Push(command);
        }
        public void Redo()
        {
            if (DoStack.Count == 0)
                return;

            var command = DoStack.Pop();
            command.RedoOperation();
            UndoStack.Push(command);
        }
        public void CreateAndExtuteCommand(string commandName,Action<IOperationCommandData> doAction, Action<IOperationCommandData> undoAction)
        {
            var command = new TtOperationCommand(commandName, null, doAction, null, undoAction);
            DoStack.Push(command);
            command.RedoOperation();
        }
        public void CreateAndExtuteCommand(string commandName, IOperationCommandData doData, Action<IOperationCommandData> doAction, IOperationCommandData undoData, Action<IOperationCommandData> undoAction)
        {
            var command = new TtOperationCommand(commandName, doData, doAction, undoData, undoAction);
            DoStack.Push(command);
            command.RedoOperation();
        }
        public void CreateCommand(string commandName, Action<IOperationCommandData> doAction, Action<IOperationCommandData> undoAction)
        {
            var command = new TtOperationCommand(commandName, null, doAction, null, undoAction);
            DoStack.Push(command);
        }
        public void CreateCommand(string commandName, IOperationCommandData doData, Action<IOperationCommandData> doAction, IOperationCommandData undoData, Action<IOperationCommandData> undoAction)
        {
            var command = new TtOperationCommand(commandName, doData, doAction, undoData, undoAction);
            DoStack.Push(command);
        }
    }
}
