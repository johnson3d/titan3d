using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace EditorCommon.UndoRedo
{
    public interface ICommand
    {
        string InfoMsg { get; }
        void Redo();
        void Undo();
    }

    public class ActionCommand : ICommand
    {
        string mInfoMsg = "";
        public string InfoMsg
        {
            get { return mInfoMsg; }
        }

        object mExecuteData = null;
        Action<object> mExecuteAction = null;

        object mUnExecuteData = null;
        Action<object> mUnExecuteAction = null;
        public ActionCommand(object executeData, Action<object> execute, object unExecuteData, Action<object> unExecute, string infoMsg)
        {
            mInfoMsg = infoMsg;
            mExecuteData = executeData;
            mExecuteAction = execute;
            mUnExecuteData = unExecuteData;
            mUnExecuteAction = unExecute;
        }
        public void Redo()
        {
            mExecuteAction?.Invoke(mExecuteData);
        }
        public void Undo()
        {
            mUnExecuteAction?.Invoke(mUnExecuteData);
        }
    }

    public class UndoRedoManager : EngineNS.Editor.IEditorInstanceObject
    {
        static UndoRedoManager smInstance = new UndoRedoManager();
        public static UndoRedoManager Instance
        {
            get
            {
                var name = typeof(UndoRedoManager).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = new UndoRedoManager();
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }

        public void FinalCleanup()
        {
            mUndoRedoDataDic.Clear();
        }

        public int UndoCount = 100;

        class UndoRedoData
        {
            public string mKey;
            public ConcurrentStack<ICommand> mRedoStack = new ConcurrentStack<ICommand>();
            public ConcurrentStack<ICommand> mUndoStack = new ConcurrentStack<ICommand>();
        }
        Dictionary<string, UndoRedoData> mUndoRedoDataDic = new Dictionary<string, UndoRedoData>();

        public void AddCommand(string key, ICommand command)
        {
            if (command == null || string.IsNullOrEmpty(key))
                return;

            UndoRedoData data;
            if(!mUndoRedoDataDic.TryGetValue(key, out data))
            {
                data = new UndoRedoData();
                data.mKey = key;
                mUndoRedoDataDic[key] = data;
            }
            //command.Redo();
            data.mUndoStack.Push(command);
            if(data.mUndoStack.Count > UndoCount)
            {
                ICommand[] cmds = new ICommand[UndoCount];
                data.mUndoStack.TryPopRange(cmds, 0, UndoCount);
                data.mUndoStack.PushRange(cmds);
            }
            data.mRedoStack.Clear();

            EditorCommon.Command.CustomCommands.Undo.RaizeCanExecuteChanged();
        }
        public void AddCommand(string key, object redoData, Action<object> redoAction, object undoData, Action<object> undoAction, string infoMsg)
        {
            var cmd = new ActionCommand(redoData, redoAction, undoData, undoAction, infoMsg);
            AddCommand(key, cmd);
        }
        public void ClearCommands(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            UndoRedoData data;
            if(mUndoRedoDataDic.TryGetValue(key, out data))
            {
                data.mUndoStack.Clear();
                data.mRedoStack.Clear();
            }

            EditorCommon.Command.CustomCommands.Undo.RaizeCanExecuteChanged();
            EditorCommon.Command.CustomCommands.Redo.RaizeCanExecuteChanged();
        }
        public string GetTopUndoString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";
            UndoRedoData data;
            if (!mUndoRedoDataDic.TryGetValue(key, out data))
                return "";
            ICommand cmd;
            if(!data.mUndoStack.TryPeek(out cmd))
                return "";
            return cmd.InfoMsg;
        }
        public bool CanUndo(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            UndoRedoData data;
            if (!mUndoRedoDataDic.TryGetValue(key, out data))
                return false;
            if (data.mUndoStack.Count == 0)
                return false;
            return true;
        }
        public bool Undo(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            UndoRedoData data;
            if (!mUndoRedoDataDic.TryGetValue(key, out data))
                return false;

            if (data.mUndoStack.Count == 0)
                return false;

            ICommand cmd;
            if (!data.mUndoStack.TryPop(out cmd))
                return false;
            data.mRedoStack.Push(cmd);

            cmd?.Undo();

            EditorCommon.Command.CustomCommands.Undo.RaizeCanExecuteChanged();
            EditorCommon.Command.CustomCommands.Redo.RaizeCanExecuteChanged();
            return true;
        }
        public string GetTopRedoString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";
            UndoRedoData data;
            if (!mUndoRedoDataDic.TryGetValue(key, out data))
                return "";
            ICommand cmd;
            if (!data.mRedoStack.TryPeek(out cmd))
                return "";
            return cmd.InfoMsg;
        }
        public bool CanRedo(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            UndoRedoData data;
            if (!mUndoRedoDataDic.TryGetValue(key, out data))
                return false;
            if (data.mRedoStack.Count == 0)
                return false;
            return true;
        }
        public bool Redo(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            UndoRedoData data;
            if (!mUndoRedoDataDic.TryGetValue(key, out data))
                return false;

            if (data.mRedoStack.Count == 0)
                return false;

            ICommand cmd;
            if (!data.mRedoStack.TryPop(out cmd))
                return false;
            data.mUndoStack.Push(cmd);

            cmd.Redo();

            EditorCommon.Command.CustomCommands.Undo.RaizeCanExecuteChanged();
            EditorCommon.Command.CustomCommands.Redo.RaizeCanExecuteChanged();
            return true;
        }
    }
}
