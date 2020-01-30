using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EditorCommon.Command
{
    public class UndoCommand : CustomCommand
    {
        public override string Text
        {
            get => "Undo";
        }
        public override bool CanExecute(object parameter)
        {
            return EditorCommon.UndoRedo.UndoRedoManager.Instance.CanUndo((string)parameter);
        }

        public override void Execute(object parameter)
        {
            EditorCommon.UndoRedo.UndoRedoManager.Instance.Undo((string)parameter);
        }
    }

    public class RedoCommand : CustomCommand
    {
        public override string Text
        {
            get => "Redo";
        }
        public override bool CanExecute(object parameter)
        {
            return EditorCommon.UndoRedo.UndoRedoManager.Instance.CanRedo((string)parameter);
        }

        public override void Execute(object parameter)
        {
            EditorCommon.UndoRedo.UndoRedoManager.Instance.Redo((string)parameter);
        }
    }
}
