using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross
{
    public interface IOperationCommand
    {
        public string Name { get; set; }
        public void Excute();
        public Action Do { get; set; }
        public Action Undo { get; set; }
    }
    public class TtOperationCommand : IOperationCommand
    {
        public TtOperationCommand(string commandName,Action doAction, Action undoAction)
        {
            Name = commandName;
            Do = doAction;
            Undo = undoAction;
        }

        public Action Do { get; set ; }
        public Action Undo { get; set; }
        public string Name { get; set; }

        public void Excute()
        {
            Do();
        }
    }
    public class TtCommandHistory
    {
        Stack<IOperationCommand> DoStack = new Stack<IOperationCommand>();
        Stack<IOperationCommand> UndoStack = new Stack<IOperationCommand>();
        public void Undo()
        {
            var command = DoStack.Pop();
            command.Undo();
            UndoStack.Push(command);
        }
        public void Redo()
        {
            var command = UndoStack.Pop();
            command.Do();
            DoStack.Push(command);
        }
        public void CreateAndExtuteCommand(string commandName,Action doAction, Action undoAction)
        {
            var command = new TtOperationCommand(commandName, doAction, undoAction);
            DoStack.Push(command);
            command.Excute();
        }
    }
}
