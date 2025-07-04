using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Command
{
    /// <summary>
    /// 栈式执行AnimationCommand，依赖于先后关系
    /// </summary>
    public class TtAnimationCommandExecuteStack
    {
        public List<TtParallelExecuteCommandList> CommandLists = new List<TtParallelExecuteCommandList>();
        public void AddCommand(int depth, IAnimationCommand cmd)
        {
            if (depth >= CommandLists.Count)
            {
                int extraDepth = depth * 2 - CommandLists.Count + 1;
                for (int i = 0; i < extraDepth; ++i)
                {
                    CommandLists.Add(new TtParallelExecuteCommandList());
                }
            }
            CommandLists[depth].AddCommand(cmd);
        }
        public void Execute()
        {
            for(int i = CommandLists.Count - 1; i >= 0; i--)
            {
                CommandLists[i].Execute();
            }
        }
    }
    /// <summary>
    /// 可并行的AnimationCommand，之间没有关系
    /// </summary>
    public class TtParallelExecuteCommandList
    {
        List<IAnimationCommand> Commands = new List<IAnimationCommand>();
        public void AddCommand(IAnimationCommand cmd)
        {
            Commands.Add(cmd);
        }
        public void Execute()
        {
            foreach(var cmd in Commands)
            {
                cmd?.Execute();
            }
            //TtEngine.Instance.EventPoster.ParallelFor(Commands.Count, static (Thread.Async.TtAsyncTaskStateBase state) =>
            //{
            //    int index = state.IndexOfParallelFor;
            //    var Commands = state.GetForArgument0<List<IAnimationCommand>>();
            //    Commands[index].Execute();
            //}, Commands);
        }
    }

    /// <summary>
    /// Command can find in BlendTree node
    /// </summary>
    public interface IAnimationCommand
    {
        void Execute();
    }
    public interface IAnimationCommandDesc
    {

    }
    public class TtAnimationCommand<S, T> : IAnimationCommand
    {
        public S CenterData { get; set; }
        protected T mOutPose = default;
        public T OutPose { get => mOutPose; set => mOutPose = value; }
        public virtual void Execute()
        {
            //到这里所有测参数计算都已经准备好
        }
    }
}
