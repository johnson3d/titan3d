using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EditorCommon.Command
{
    public class CustomCommand : ICommand, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public event EventHandler CanExecuteChanged;
        public void RaizeCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool CanExecute(object parameter) { return false; }

        public virtual void Execute(object parameter) { }

        protected string mText = "";
        public virtual string Text
        {
            get => mText;
            set
            {
                mText = value;
                OnPropertyChanged("Text");
            }
        }
    }

    public class CustomCommands
    {
        public readonly static UndoCommand Undo = new UndoCommand();
        public readonly static RedoCommand Redo = new RedoCommand();
    }
}
