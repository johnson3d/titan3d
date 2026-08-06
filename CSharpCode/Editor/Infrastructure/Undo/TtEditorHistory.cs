using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Infrastructure
{
    /// <summary>
    /// 每个编辑器独立持有的操作历史栈。命令线性排列, CurrentStep指向已生效命令数,
    /// Undo/Redo在栈上移动游标, 新命令入栈时丢弃游标之后的redo段。
    /// </summary>
    public class TtEditorHistory
    {
        readonly List<TtEditorCommand> mCommands = new List<TtEditorCommand>();
        int mCurStep = 0;
        // 保存点: 与CurrentStep一致时认为历史无未保存修改, -1表示保存点已被容量裁剪掉
        int mSavePoint = 0;
        bool mIsApplying = false;
        int mTransactionDepth = 0;
        TtTransactionCommand mPendingTransaction = null;

        /// <summary>
        /// 历史栈最大步数, 跨出后丢弃最老的记录。跟随每个编辑器实例, 各编辑器可按需调整, 缺省64
        /// </summary>
        public int MaxHistorySteps { get; set; } = 64;

        public event Action OnHistoryChanged;
        public IReadOnlyList<TtEditorCommand> Commands => mCommands;
        public int CurrentStep => mCurStep;
        /// <summary>
        /// Undo/Redo回放期间为true, 用于阻止属性setter副作用再次入栈
        /// </summary>
        public bool IsApplying => mIsApplying;
        public bool CanUndo => mCurStep > 0;
        public bool CanRedo => mCurStep < mCommands.Count;
        public bool IsDirtyFromHistory => mCurStep != mSavePoint;

        /// <summary>
        /// 执行命令并入栈
        /// </summary>
        public void ExecuteCommand(TtEditorCommand cmd)
        {
            if (cmd == null || mIsApplying)
                return;
            mIsApplying = true;
            try
            {
                cmd.Do();
            }
            finally
            {
                mIsApplying = false;
            }
            PushCommand(cmd);
        }
        /// <summary>
        /// 命令已在外部执行完毕, 仅入栈记录。会先尝试与栈顶未封口命令合并
        /// </summary>
        public void PushCommand(TtEditorCommand cmd)
        {
            if (cmd == null || mIsApplying)
                return;
            if (mPendingTransaction != null)
            {
                mPendingTransaction.Commands.Add(cmd);
                return;
            }
            // 丢弃redo段
            if (mCurStep < mCommands.Count)
            {
                mCommands.RemoveRange(mCurStep, mCommands.Count - mCurStep);
                if (mSavePoint > mCurStep)
                    mSavePoint = -1;
            }
            if (mCurStep > 0 && mCommands[mCurStep - 1].TryMerge(cmd))
            {
                OnHistoryChanged?.Invoke();
                return;
            }
            mCommands.Add(cmd);
            mCurStep = mCommands.Count;
            // 容量裁剪
            var maxSteps = Math.Max(2, MaxHistorySteps);
            while (mCommands.Count > maxSteps)
            {
                mCommands.RemoveAt(0);
                mCurStep--;
                if (mSavePoint >= 0)
                    mSavePoint--;
            }
            OnHistoryChanged?.Invoke();
        }
        public bool Undo()
        {
            if (CanUndo == false)
                return false;
            SealTopCommand();
            mIsApplying = true;
            try
            {
                mCommands[mCurStep - 1].Undo();
            }
            finally
            {
                mIsApplying = false;
            }
            mCurStep--;
            OnHistoryChanged?.Invoke();
            return true;
        }
        public bool Redo()
        {
            if (CanRedo == false)
                return false;
            mIsApplying = true;
            try
            {
                mCommands[mCurStep].Do();
            }
            finally
            {
                mIsApplying = false;
            }
            mCurStep++;
            OnHistoryChanged?.Invoke();
            return true;
        }
        /// <summary>
        /// 历史面板点击条目多步跳转, step取值[0, Commands.Count]
        /// </summary>
        public void JumpTo(int step)
        {
            step = Math.Clamp(step, 0, mCommands.Count);
            while (mCurStep > step)
            {
                if (Undo() == false)
                    break;
            }
            while (mCurStep < step)
            {
                if (Redo() == false)
                    break;
            }
        }
        /// <summary>
        /// 封口栈顶命令使其不再合并后续修改(拖拽/连续输入结束时调用)
        /// </summary>
        public void SealTopCommand()
        {
            if (mCurStep > 0)
                mCommands[mCurStep - 1].Seal();
        }
        /// <summary>
        /// 保存资产成功后调用, 联动脏标记
        /// </summary>
        public void SetSavePoint()
        {
            mSavePoint = mCurStep;
            OnHistoryChanged?.Invoke();
        }
        public void Clear()
        {
            mCommands.Clear();
            mCurStep = 0;
            mSavePoint = 0;
            mPendingTransaction = null;
            mTransactionDepth = 0;
            OnHistoryChanged?.Invoke();
        }

        /// <summary>
        /// 开启事务: 期间PushCommand的命令收集为一条复合记录, EndTransaction时统一入栈。
        /// 支持嵌套(如删除多选节点时外层事务包含每个RemoveNode自身的事务), 只有最外层End时入栈。
        /// 用于gizmo拖动、多选批量修改、节点删除级联断线等复合操作
        /// </summary>
        public void BeginTransaction(string name)
        {
            if (mIsApplying)
                return;
            if (mTransactionDepth == 0)
                mPendingTransaction = new TtTransactionCommand(name);
            mTransactionDepth++;
        }
        public void EndTransaction()
        {
            if (mIsApplying)
                return;
            if (mTransactionDepth == 0)
                return;
            mTransactionDepth--;
            if (mTransactionDepth > 0)
                return;
            var transaction = mPendingTransaction;
            mPendingTransaction = null;
            if (transaction == null || transaction.Commands.Count == 0)
                return;
            if (transaction.Commands.Count == 1)
            {
                // 只有一条子命令时不必包一层
                PushCommand(transaction.Commands[0]);
                return;
            }
            transaction.Seal();
            PushCommand(transaction);
        }
    }
}
