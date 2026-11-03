注意事项
- 所有关于位置的计算，最好是DVector3的，保证大世界坐标精度
- 项目里不要自己new任何TtNode的派生类，而是使用SpawnNode和SpawnSceneActor，确保构造的完整性和顺序 
- 所有初始化的行为，async函数尽量利用await调用，如果最后一定要在一个非async函数中调用，可以使用TtTask<T>.GetResultUntilCompleted()来强制等待结果
- 如果TtTask不需要强制立即执行，可以用TtTask.AddWaitTask((task)=>{})来等待完成后Callback
- 利用类似[EngineNS.Bricks.CodeBuilder.ContextMenu("MonsterSpawner", "Game\\Survivor\\MonsterSpawner", TtNode.EditorKeyword)]来描述Node，可以在场景编辑器中动态产生和编辑
- 潜在需要EditorRun后修改的逻辑，可以通过给TtNode设置Behavior的cs代码实现，参考survivor\test_bhv01.cs