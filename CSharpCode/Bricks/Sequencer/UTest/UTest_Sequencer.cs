using EngineNS.Sequencer;
using EngineNS.Sequencer.Asset;
using System;
using System.Collections.Generic;

namespace EngineNS.UnitTest
{
    /// <summary>
    /// Sequencer 阶段 1 的单元测试。重点是序列化往返: 通道用了 List&lt;long&gt; 与
    /// List&lt;可 blit 结构体&gt;, 轨道/片段列表还是多态的 (元素声明成抽象基类), 这几件事
    /// 都依赖 DataCopyer 的具体分支行为, 光看代码不足以确认, 必须端到端存读一遍。
    /// </summary>
    [TtTest]
    public class UTest_Sequencer
    {
        public void UnitTestEntrance()
        {
            TestFrameRate();
            TestScalarChannel();
            TestQuatChannel();
            TestNameChannel();
            TestSectionContains();
            TestSectionKeyGrouping();
            TestEvalTableAndPreAnimated();
            TestValueAdapters();
            TestReflectedPropertyTrack();
            TestSerializeRoundTrip();
        }

        void TestFrameRate()
        {
            var tickRes = TtFrameRate.DefaultTickResolution;
            var display = TtFrameRate.DefaultDisplayRate;
            TtUnitTestManager.TAssert(tickRes.IsValid, "tickRes.IsValid");
            TtUnitTestManager.TAssert(tickRes.FromSeconds(1.0) == 24000, "1s == 24000 ticks");
            TtUnitTestManager.TAssert(Math.Abs(tickRes.AsSeconds(12000) - 0.5) < 1e-9, "12000 ticks == 0.5s");
            // 24000 能被 30 整除, 一个显示帧正好 800 tick
            TtUnitTestManager.TAssert(tickRes.TicksPerFrame(display) == 800, "TicksPerFrame == 800");
            TtUnitTestManager.TAssert(tickRes.SnapTo(801, display) == 800, "SnapTo 就近向下");
            TtUnitTestManager.TAssert(tickRes.SnapTo(1199, display) == 800, "SnapTo 就近向下(接近上一帧边界)");
            TtUnitTestManager.TAssert(tickRes.SnapTo(1200, display) == 1600, "SnapTo 正中间向上");
            TtUnitTestManager.TAssert(tickRes.ConvertTo(24000, display) == 30, "ConvertTo display");

            // 非整数帧率不能因为存了分子分母就丢精度
            var ntsc = new TtFrameRate(30000, 1001);
            TtUnitTestManager.TAssert(ntsc.FromSeconds(1001.0 / 30000.0) == 1, "ntsc one frame");

            var invalid = new TtFrameRate(0, 0);
            TtUnitTestManager.TAssert(invalid.IsValid == false, "invalid rate");
            TtUnitTestManager.TAssert(invalid.FromSeconds(1.0) == 0, "invalid rate no divide by zero");
        }
        void TestScalarChannel()
        {
            var tickRes = TtFrameRate.DefaultTickResolution;
            var channel = new TtScalarChannel();
            TtUnitTestManager.TAssert(Math.Abs(channel.Evaluate(0, tickRes) - 0.0) < 1e-9, "empty channel gives DefaultValue");

            // 乱序插入必须自己排好序
            channel.AddKey(2400, 20.0);
            channel.AddKey(0, 0.0);
            channel.AddKey(1200, 10.0);
            TtUnitTestManager.TAssert(channel.KeyCount == 3, "key count");
            TtUnitTestManager.TAssert(channel.GetKeyTime(0) == 0 && channel.GetKeyTime(1) == 1200 && channel.GetKeyTime(2) == 2400, "times sorted");

            // 同一时刻重复打帧只改值, 不新增
            channel.AddKey(1200, 11.0);
            TtUnitTestManager.TAssert(channel.KeyCount == 3, "duplicate time not inserted");
            TtUnitTestManager.TAssert(Math.Abs(channel.GetKey(1).Value - 11.0) < 1e-9, "duplicate time overrides value");
            channel.AddKey(1200, 10.0);

            // 关键帧上取值必须精确等于关键帧值, 与插值模式无关
            for (int i = 0; i < channel.KeyCount; ++i)
            {
                var v = channel.Evaluate(channel.GetKeyTime(i), tickRes);
                TtUnitTestManager.TAssert(Math.Abs(v - channel.GetKey(i).Value) < 1e-9, "evaluate on key equals key value");
            }
            // 首尾之外按 Constant 外推
            TtUnitTestManager.TAssert(Math.Abs(channel.Evaluate(-1000, tickRes) - 0.0) < 1e-9, "pre extrap constant");
            TtUnitTestManager.TAssert(Math.Abs(channel.Evaluate(999999, tickRes) - 20.0) < 1e-9, "post extrap constant");

            // 线性插值中点
            var linear = new TtScalarChannel();
            linear.AddKey(0, new FScalarKey(0.0) { InterpMode = EChannelInterpMode.Linear });
            linear.AddKey(1000, new FScalarKey(10.0) { InterpMode = EChannelInterpMode.Linear });
            TtUnitTestManager.TAssert(Math.Abs(linear.Evaluate(500, tickRes) - 5.0) < 1e-9, "linear midpoint");

            // 切线全 0 的三次插值退化成平滑 S 曲线, 中点仍是两端均值
            var cubic = new TtScalarChannel();
            cubic.AddKey(0, new FScalarKey(0.0) { InterpMode = EChannelInterpMode.Cubic });
            cubic.AddKey(1000, new FScalarKey(10.0) { InterpMode = EChannelInterpMode.Cubic });
            TtUnitTestManager.TAssert(Math.Abs(cubic.Evaluate(500, tickRes) - 5.0) < 1e-9, "cubic midpoint with zero tangents");

            // Constant 插值段内保持左端点值
            var step = new TtScalarChannel();
            step.AddKey(0, new FScalarKey(0.0) { InterpMode = EChannelInterpMode.Constant });
            step.AddKey(1000, new FScalarKey(10.0) { InterpMode = EChannelInterpMode.Constant });
            TtUnitTestManager.TAssert(Math.Abs(step.Evaluate(999, tickRes) - 0.0) < 1e-9, "constant holds left value");

            // 自动切线: 中间关键帧取前后差商, 端点为 0
            var tangent = new TtScalarChannel();
            tangent.AddKey(0, 0.0);
            tangent.AddKey(24000, 10.0);
            tangent.AddKey(48000, 20.0);
            tangent.AutoComputeAllTangents(tickRes);
            TtUnitTestManager.TAssert(Math.Abs(tangent.GetKey(0).LeaveTangent) < 1e-9, "端点切线为 0");
            TtUnitTestManager.TAssert(Math.Abs(tangent.GetKey(1).LeaveTangent - 10.0) < 1e-9, "中间切线为 (20-0)/2s = 10");

            // 挪时刻要保持升序并返回新下标
            var move = new TtScalarChannel();
            move.AddKey(0, 0.0);
            move.AddKey(100, 1.0);
            move.AddKey(200, 2.0);
            var newIndex = move.SetKeyTime(0, 150);
            TtUnitTestManager.TAssert(newIndex == 1, "SetKeyTime returns new index");
            TtUnitTestManager.TAssert(move.GetKeyTime(0) == 100 && move.GetKeyTime(1) == 150 && move.GetKeyTime(2) == 200, "SetKeyTime keeps order");
            TtUnitTestManager.TAssert(Math.Abs(move.GetKey(1).Value - 0.0) < 1e-9, "SetKeyTime carries value");
        }
        void TestQuatChannel()
        {
            var channel = new TtQuatChannel();
            TtUnitTestManager.TAssert(channel.Evaluate(0) == Quaternion.Identity, "empty quat channel gives identity");

            var q0 = Quaternion.Identity;
            var q1 = Quaternion.RotationAxis(Vector3.UnitZ, MathHelper.PI * 0.5f);
            channel.AddKey(0, q0);
            channel.AddKey(1000, q1);
            TtUnitTestManager.TAssert(channel.KeyCount == 2, "quat key count");

            var mid = channel.Evaluate(500);
            var expect = Quaternion.Slerp(q0, q1, 0.5f);
            TtUnitTestManager.TAssert(QuatNear(mid, expect), "quat midpoint is slerp");
            TtUnitTestManager.TAssert(QuatNear(channel.Evaluate(0), q0), "quat on first key");
            TtUnitTestManager.TAssert(QuatNear(channel.Evaluate(1000), q1), "quat on last key");
            TtUnitTestManager.TAssert(QuatNear(channel.Evaluate(-500), q0), "quat pre extrap");
            TtUnitTestManager.TAssert(QuatNear(channel.Evaluate(9999), q1), "quat post extrap");

            channel.IsStepped = true;
            TtUnitTestManager.TAssert(QuatNear(channel.Evaluate(999), q0), "stepped quat holds left key");
        }
        static bool QuatNear(in Quaternion a, in Quaternion b)
        {
            return Math.Abs(a.X - b.X) < 1e-5f && Math.Abs(a.Y - b.Y) < 1e-5f
                && Math.Abs(a.Z - b.Z) < 1e-5f && Math.Abs(a.W - b.W) < 1e-5f;
        }
        void TestSectionContains()
        {
            var section = new TtTransformSection() { StartTick = 100, EndTick = 200 };
            TtUnitTestManager.TAssert(section.Contains(100), "闭区间含起点");
            // 闭区间是刻意的: 播放头停在序列末尾时必须还有 Section 命中, 否则最后一帧弹回原值
            TtUnitTestManager.TAssert(section.Contains(200), "闭区间含终点");
            TtUnitTestManager.TAssert(section.Contains(99) == false, "起点之前不命中");
            TtUnitTestManager.TAssert(section.Contains(201) == false, "终点之后不命中");
            TtUnitTestManager.TAssert(section.Duration == 100, "Duration");

            TtUnitTestManager.TAssert(Math.Abs(section.GetEaseWeight(150) - 1.0f) < 1e-6f, "没设淡化时权重恒为 1");
            section.EaseInDuration = 50;
            TtUnitTestManager.TAssert(Math.Abs(section.GetEaseWeight(100) - 0.0f) < 1e-6f, "淡入起点权重 0");
            TtUnitTestManager.TAssert(Math.Abs(section.GetEaseWeight(125) - 0.5f) < 1e-6f, "淡入中点权重 0.5");
            TtUnitTestManager.TAssert(Math.Abs(section.GetEaseWeight(160) - 1.0f) < 1e-6f, "淡入结束后权重 1");
        }

        class FTestTarget
        {
            public double Value = 0.0;
        }
        class FTestAccessor : ISequencePropertyAccessor
        {
            public string PropertyId { get => "UTest.Value"; }
            public string DisplayName { get => "Value"; }
            public Rtti.TtTypeDesc ValueType { get => Rtti.TtTypeDesc.TypeOf(typeof(double)); }
            public object Read(object target)
            {
                var t = target as FTestTarget;
                return t != null ? (object)t.Value : null;
            }
            public void Write(object target, object value)
            {
                var t = target as FTestTarget;
                if (t == null || value is double == false)
                    return;
                t.Value = (double)value;
            }
        }
        /// <summary>
        /// 连续打点必须落在同一个 Section 里。这条守的是一个界面上完全看不出来的回归:
        /// 若每次在已有 Section 之外打点都新建一个 [0, tick], 它们会在时间轴同一行上重叠
        /// 成一条看着正常的条, 但每个只含一个关键帧 —— 求值退化成阶梯跳变, 插值永远不生效。
        /// </summary>
        void TestSectionKeyGrouping()
        {
            var track = new TtTransformTrack();
            var first = track.GetOrCreateSectionAt(0);
            TtUnitTestManager.TAssert(track.Sections.Count == 1, "空轨道打点新建一个 Section");

            // Section 范围之外打点: 必须复用已有的, 不能新建
            var outside = track.GetOrCreateSectionAt(24000);
            TtUnitTestManager.TAssert(ReferenceEquals(outside, first), "Section 之外打点复用已有 Section");
            TtUnitTestManager.TAssert(track.Sections.Count == 1, "不得新建重叠的 Section");

            // 模拟编辑器打点时的范围扩展, 之后该 tick 就能被 Contains 命中了
            first.EndTick = 24000;
            first.PositionX.AddKey(0, 0.0);
            first.PositionX.AddKey(24000, 10.0);
            TtUnitTestManager.TAssert(track.GetOrCreateSectionAt(12000) == first, "扩展后区间内打点命中同一个");

            // 两个关键帧落在同一通道里, 帧间才会真的插值 (单关键帧通道会恒返回那个值)
            var tickRes = TtFrameRate.DefaultTickResolution;
            var mid = first.PositionX.Evaluate(12000, tickRes);
            TtUnitTestManager.TAssert(mid > 0.0 && mid < 10.0, "帧间求值落在两个关键帧之间");
        }
        void TestEvalTableAndPreAnimated()
        {
            var registry = new TtSequencePropertyRegistry();
            var accessor = new FTestAccessor();
            TtUnitTestManager.TAssert(registry.Register(accessor), "首次注册成功");
            TtUnitTestManager.TAssert(registry.Register(new FTestAccessor()) == false, "撞键注册被拒绝且不抛异常");
            TtUnitTestManager.TAssert(ReferenceEquals(registry.Find("UTest.Value"), accessor), "撞键时保留先注册的");
            TtUnitTestManager.TAssert(registry.Find("NoSuchId") == null, "找不到返回 null");

            var target = new FTestTarget() { Value = 7.0 };
            var table = new TtSequenceEvalTable();
            var store = new TtPreAnimatedStore();

            table.Write(target, accessor, 1.0, 0);
            table.Write(target, accessor, 2.0, 0);
            TtUnitTestManager.TAssert(table.Count == 1, "同一属性只占一条");
            table.Flush(store);
            TtUnitTestManager.TAssert(Math.Abs(target.Value - 1.0) < 1e-9, "相同优先级保留先写入的");
            TtUnitTestManager.TAssert(store.Count == 1, "Flush 时记下原值");

            table.Clear();
            table.Write(target, accessor, 3.0, 5);
            table.Flush(store);
            TtUnitTestManager.TAssert(Math.Abs(target.Value - 3.0) < 1e-9, "高优先级覆盖");
            TtUnitTestManager.TAssert(store.Count == 1, "第二帧不重复记原值");

            store.RestoreAll();
            TtUnitTestManager.TAssert(Math.Abs(target.Value - 7.0) < 1e-9, "恢复到播放前的值而不是上一帧的值");
            TtUnitTestManager.TAssert(store.Count == 0, "RestoreAll 后清空");

            // 目标已销毁的情形: Forget 之后不该再往它身上写
            var target2 = new FTestTarget() { Value = 1.0 };
            table.Clear();
            table.Write(target2, accessor, 9.0, 0);
            table.Flush(store);
            store.Forget(target2);
            TtUnitTestManager.TAssert(store.Count == 0, "Forget 丢掉记录");
            store.RestoreAll();
            TtUnitTestManager.TAssert(Math.Abs(target2.Value - 9.0) < 1e-9, "Forget 后不恢复");
        }

        enum ETestMode
        {
            First = 0,
            Second = 1,
            Third = 2,
        }
        class FTestReflectTarget
        {
            [Rtti.Meta]
            public bool Visible { get; set; } = true;
            [Rtti.Meta]
            public double Weight { get; set; } = 0.0;
            [Rtti.Meta]
            public ETestMode Mode { get; set; } = ETestMode.First;
            [Rtti.Meta]
            public RName Asset { get; set; } = null;
            /// <summary>没有 [Rtti.Meta]: 不该出现在可动画属性里</summary>
            public double Hidden { get; set; } = 0.0;
            /// <summary>只有 get: 不该出现在可动画属性里</summary>
            [Rtti.Meta]
            public double Computed { get => Weight * 2.0; }
        }

        /// <summary>
        /// 适配器注册表是静态的, 单测可能跑在模块初始化之前也可能在之后。重复注册
        /// 只会报 Warning 并保留先注册的, 行为上没影响, 但先看一眼能省掉一串无意义的告警。
        /// </summary>
        static void EnsureAdapters()
        {
            if (TtSequenceValueAdapters.FindById(TtBoolValueAdapter.Id) == null)
                TtBuiltinValueAdapters.RegisterAll();
        }
        /// <summary>比 RName 不用 ReferenceEquals: 不依赖 RNameManager 是不是把同名字缓成同一实例</summary>
        static bool SameName(RName x, RName y)
        {
            return x != null && y != null && x.ToString() == y.ToString();
        }
        static FSectionEvalContext MakeEvalCtx(long time, in TtFrameRate tickRes)
        {
            return new FSectionEvalContext()
            {
                Time = time,
                Weight = 1.0f,
                TickResolution = tickRes,
            };
        }
        /// <summary>
        /// 资产引用通道: 不可插值的数据靠阶梯求值, 且第一个关键帧之前不往前铺。
        /// 往前铺会让"第 30 帧换贴图"在第 0 帧就已经换过了。
        /// </summary>
        void TestNameChannel()
        {
            var a = RName.GetRName("utest/seq_a.tex", RName.ERNameType.Engine);
            var b = RName.GetRName("utest/seq_b.tex", RName.ERNameType.Engine);
            var channel = new TtNameChannel();
            TtUnitTestManager.TAssert(channel.Evaluate(0) == null, "空通道返回 null 表示不覆盖");

            // null 进了 Values 会让整条序列存不下来, 宁可这个点打不上
            TtUnitTestManager.TAssert(channel.AddKey(1200, null) == -1, "null 不能进通道");
            TtUnitTestManager.TAssert(channel.KeyCount == 0, "拒绝 null 后通道还是空的");

            channel.AddKey(2400, b);
            channel.AddKey(1200, a);
            TtUnitTestManager.TAssert(channel.KeyCount == 2 && channel.GetKeyTime(0) == 1200, "乱序插入自动排序");
            TtUnitTestManager.TAssert(channel.Evaluate(800) == null, "第一个关键帧之前不往前铺");
            TtUnitTestManager.TAssert(SameName(channel.Evaluate(1200), a), "关键帧上取到自己");
            TtUnitTestManager.TAssert(SameName(channel.Evaluate(2000), a), "两帧之间保持左侧值");
            TtUnitTestManager.TAssert(SameName(channel.Evaluate(999999), b), "最后一帧之后保持");

            channel.RemoveKey(0);
            TtUnitTestManager.TAssert(channel.KeyCount == 1 && SameName(channel.Evaluate(999999), b), "删关键帧后 Times/Values 仍一一对应");
        }
        /// <summary>
        /// 值适配器: 重点是不可插值的类型必须用 Constant, 以及没打点的分量要保持目标原值。
        /// </summary>
        void TestValueAdapters()
        {
            EnsureAdapters();
            var tickRes = TtFrameRate.DefaultTickResolution;

            // 布尔: 存 0/1 走标量通道。插值必须是 Constant, 否则会在两帧中间就提前翻转
            var boolAdapter = TtSequenceValueAdapters.FindByValueType(typeof(bool));
            TtUnitTestManager.TAssert(boolAdapter != null, "bool 有适配器");
            TtUnitTestManager.TAssert(boolAdapter.DefaultInterpMode == EChannelInterpMode.Constant, "bool 用阶梯插值");
            var boolSection = new TtPropertySection() { AdapterId = boolAdapter.AdapterId };
            boolSection.EnsureChannels(boolAdapter);
            TtUnitTestManager.TAssert(boolSection.ScalarChannels.Count == 1, "bool 占一条标量通道");
            boolAdapter.AddKey(boolSection, 0, false, in tickRes);
            boolAdapter.AddKey(boolSection, 2400, true, in tickRes);
            TtUnitTestManager.TAssert(boolSection.ScalarChannels[0].GetKey(0).InterpMode == EChannelInterpMode.Constant, "打点时就把插值方式设对");
            var midCtx = MakeEvalCtx(1200, in tickRes);
            var midValue = boolAdapter.Evaluate(boolSection, true, in midCtx);
            TtUnitTestManager.TAssert(midValue is bool && (bool)midValue == false, "两帧之间保持左侧的 false");
            var endCtx = MakeEvalCtx(2400, in tickRes);
            TtUnitTestManager.TAssert((bool)boolAdapter.Evaluate(boolSection, false, in endCtx), "到点翻成 true");

            // 枚举: 类型是开放集合, 靠注册表的 IsEnum 分支兜底到同一个适配器
            var enumAdapter = TtSequenceValueAdapters.FindByValueType(typeof(ETestMode));
            TtUnitTestManager.TAssert(enumAdapter != null && enumAdapter.AdapterId == TtEnumValueAdapter.Id, "枚举兜底到 Enum 适配器");
            var enumSection = new TtPropertySection() { AdapterId = enumAdapter.AdapterId };
            enumSection.EnsureChannels(enumAdapter);
            enumAdapter.AddKey(enumSection, 0, ETestMode.First, in tickRes);
            enumAdapter.AddKey(enumSection, 2400, ETestMode.Third, in tickRes);
            var enumValue = enumAdapter.Evaluate(enumSection, ETestMode.Second, in midCtx);
            TtUnitTestManager.TAssert(enumValue is ETestMode && (ETestMode)enumValue == ETestMode.First, "枚举阶梯保持左值");
            // 拿不到具体枚举类型时不写, 好过写一个类型不对的值进去被 Write 静默丢弃
            TtUnitTestManager.TAssert(enumAdapter.Evaluate(enumSection, 1.0, in midCtx) == null, "当前值不是枚举就不写");

            // 三分量向量: 只 K 了 Z 的轨道不能把 X/Y 拽回 0
            var vecAdapter = TtSequenceValueAdapters.FindByValueType(typeof(DVector3));
            TtUnitTestManager.TAssert(vecAdapter != null && vecAdapter.ScalarChannelCount == 3, "DVector3 占三条通道");
            var vecSection = new TtPropertySection() { AdapterId = vecAdapter.AdapterId };
            vecSection.EnsureChannels(vecAdapter);
            vecSection.ScalarChannels[2].AddKey(0, 5.0);
            var vecValue = vecAdapter.Evaluate(vecSection, new DVector3(1.0, 2.0, 3.0), in midCtx);
            TtUnitTestManager.TAssert(vecValue is DVector3, "组装出 DVector3");
            var dv = (DVector3)vecValue;
            TtUnitTestManager.TAssert(Math.Abs(dv.X - 1.0) < 1e-9 && Math.Abs(dv.Y - 2.0) < 1e-9, "没打点的分量保持目标原值");
            TtUnitTestManager.TAssert(Math.Abs(dv.Z - 5.0) < 1e-9, "打了点的分量取通道值");
            // 一条通道都没打过时不写, 否则刚建好的空轨道会把目标锁死在当前值上
            var emptyVec = new TtPropertySection() { AdapterId = vecAdapter.AdapterId };
            emptyVec.EnsureChannels(vecAdapter);
            TtUnitTestManager.TAssert(vecAdapter.Evaluate(emptyVec, new DVector3(1.0, 2.0, 3.0), in midCtx) == null, "空轨道不写");

            // 四分量向量: 引擎里的 RGBA 颜色属性大多是 Vector4
            var vec4Adapter = TtSequenceValueAdapters.FindByValueType(typeof(Vector4));
            TtUnitTestManager.TAssert(vec4Adapter != null && vec4Adapter.ScalarChannelCount == 4, "Vector4 占四条通道");
            var vec4Section = new TtPropertySection() { AdapterId = vec4Adapter.AdapterId };
            vec4Section.EnsureChannels(vec4Adapter);
            vec4Section.ScalarChannels[3].AddKey(0, 0.5);
            var vec4Value = vec4Adapter.Evaluate(vec4Section, new Vector4(1, 2, 3, 4), in midCtx);
            TtUnitTestManager.TAssert(vec4Value is Vector4, "组装出 Vector4");
            var v4 = (Vector4)vec4Value;
            TtUnitTestManager.TAssert(Math.Abs(v4.W - 0.5) < 1e-6 && Math.Abs(v4.X - 1.0) < 1e-6, "只打 W 时 X 保持目标原值");

            var vec2Adapter = TtSequenceValueAdapters.FindByValueType(typeof(Vector2));
            TtUnitTestManager.TAssert(vec2Adapter != null && vec2Adapter.ScalarChannelCount == 2, "Vector2 占两条通道");
            var color3Adapter = TtSequenceValueAdapters.FindByValueType(typeof(Color3f));
            TtUnitTestManager.TAssert(color3Adapter != null && color3Adapter.ScalarChannelCount == 3, "Color3f 占三条通道");

            // Color4f 的通道映到 Red/Green/Blue/Alpha 而不是 X/Y/Z/W。写反了也能编译过,
            // 只会表现成“打红色出来是蓝色”, 所以首尾两条通道各钉一下
            var color4Adapter = TtSequenceValueAdapters.FindByValueType(typeof(Color4f));
            TtUnitTestManager.TAssert(color4Adapter != null && color4Adapter.ScalarChannelCount == 4, "Color4f 占四条通道");
            var color4Section = new TtPropertySection() { AdapterId = color4Adapter.AdapterId };
            color4Section.EnsureChannels(color4Adapter);
            color4Section.ScalarChannels[0].AddKey(0, 0.25);
            color4Section.ScalarChannels[3].AddKey(0, 0.5);
            var color4Value = color4Adapter.Evaluate(color4Section, new Color4f(1, 1, 1, 1), in midCtx);
            TtUnitTestManager.TAssert(color4Value is Color4f, "组装出 Color4f");
            var c4 = (Color4f)color4Value;
            TtUnitTestManager.TAssert(Math.Abs(c4.Red - 0.25) < 1e-6, "通道 0 落在 Red");
            TtUnitTestManager.TAssert(Math.Abs(c4.Alpha - 0.5) < 1e-6, "通道 3 落在 Alpha");
            TtUnitTestManager.TAssert(Math.Abs(c4.Green - 1.0) < 1e-6 && Math.Abs(c4.Blue - 1.0) < 1e-6, "没打点的 G/B 保持原值");

            // 资产引用: 没打点不能把目标的资产清空
            var nameAdapter = TtSequenceValueAdapters.FindByValueType(typeof(RName));
            TtUnitTestManager.TAssert(nameAdapter != null && nameAdapter.Kind == EChannelKind.Name, "RName 走资产引用通道");
            var nameSection = new TtPropertySection() { AdapterId = nameAdapter.AdapterId };
            TtUnitTestManager.TAssert(nameAdapter.Evaluate(nameSection, null, in midCtx) == null, "没打点的资产轨不覆盖");
        }
        /// <summary>
        /// 通用属性轨端到端: 反射访问器 + 适配器 + 中间表。
        ///
        /// 这条盖的是"轨道存的只是两个字符串 Id, 求值时才拼回访问器和适配器"这条链路 ——
        /// 链上任何一环拼错都只表现成"属性不动", 而不动是不报错的。
        /// </summary>
        void TestReflectedPropertyTrack()
        {
            EnsureAdapters();
            var tickRes = TtFrameRate.DefaultTickResolution;
            var registry = new TtSequencePropertyRegistry();

            // 可动画判据: [Rtti.Meta] + 公开 get/set + 值类型有适配器
            var props = new List<System.Reflection.PropertyInfo>();
            TtReflectedPropertyAccessor.GatherAnimatableProperties(typeof(FTestReflectTarget), props);
            TtUnitTestManager.TAssert(props.Count == 4, "Visible/Weight/Mode/Asset 四条可动画");
            for (int i = 0; i < props.Count; ++i)
            {
                TtUnitTestManager.TAssert(props[i].Name != "Hidden", "没有 [Rtti.Meta] 的属性不可动画");
                TtUnitTestManager.TAssert(props[i].Name != "Computed", "只读属性不可动画");
            }

            // 注册表对 "Reflect:" 前缀自动兜底, 且必须复用同一个实例 (它缓存了 PropertyInfo)
            var visibleId = TtReflectedPropertyAccessor.IdPrefix + "Visible";
            var accessor = registry.Find(visibleId);
            TtUnitTestManager.TAssert(accessor != null, "反射兜底建出访问器");
            TtUnitTestManager.TAssert(ReferenceEquals(registry.Find(visibleId), accessor), "兜底出来的访问器被缓存");
            TtUnitTestManager.TAssert(registry.Find("NoSuchPrefix.Visible") == null, "没有前缀的未知 Id 仍然返回 null");

            var target = new FTestReflectTarget() { Visible = true };
            TtUnitTestManager.TAssert(accessor.Read(target) is bool, "反射读到 bool");
            TtUnitTestManager.TAssert((bool)accessor.Read(target), "反射读值正确");
            accessor.Write(target, false);
            TtUnitTestManager.TAssert(target.Visible == false, "反射写");
            // 求值是每帧都跑的, 类型不对必须静默跳过而不能抛
            accessor.Write(target, "wrong type");
            TtUnitTestManager.TAssert(target.Visible == false, "类型不匹配静默跳过");
            TtUnitTestManager.TAssert(registry.Find(TtReflectedPropertyAccessor.IdPrefix + "Hidden").Read(target) == null,
                "没有 [Rtti.Meta] 的属性即使被手写进资产也读不到");

            // 完整一条 bool 轨道: 0 帧 false, 2400 帧 true
            var boolAdapter = TtSequenceValueAdapters.FindByValueType(typeof(bool));
            var track = new TtPropertyTrack()
            {
                PropertyId = visibleId,
                AdapterId = boolAdapter.AdapterId,
                DisplayName = "Visible",
            };
            var section = track.GetOrCreateSectionAt(2400);
            TtUnitTestManager.TAssert(section.PropertyId == visibleId, "新建 Section 继承轨道的属性 Id");
            TtUnitTestManager.TAssert(section.ScalarChannels.Count == 1, "新建 Section 按适配器分好通道");
            section.AddKeyFromValue(0, false, in tickRes);
            section.AddKeyFromValue(2400, true, in tickRes);
            TtUnitTestManager.TAssert(section.HasAnyKey, "打点后有关键帧");

            target.Visible = true;
            EvaluateTrack(track, target, 1200, in tickRes, registry);
            TtUnitTestManager.TAssert(target.Visible == false, "两帧之间保持左侧的 false");
            EvaluateTrack(track, target, 2400, in tickRes, registry);
            TtUnitTestManager.TAssert(target.Visible, "到点翻成 true");

            // 资产轨: 没打点时不许把目标的资产清空
            var nameAdapter = TtSequenceValueAdapters.FindByValueType(typeof(RName));
            var assetTrack = new TtPropertyTrack()
            {
                PropertyId = TtReflectedPropertyAccessor.IdPrefix + "Asset",
                AdapterId = nameAdapter.AdapterId,
            };
            var assetSection = assetTrack.GetOrCreateSectionAt(2400);
            var texA = RName.GetRName("utest/seq_a.tex", RName.ERNameType.Engine);
            var texB = RName.GetRName("utest/seq_b.tex", RName.ERNameType.Engine);
            target.Asset = texA;
            EvaluateTrack(assetTrack, target, 0, in tickRes, registry);
            TtUnitTestManager.TAssert(SameName(target.Asset, texA), "没打点的资产轨不覆盖目标当前值");

            assetSection.AddKeyFromValue(1200, texB, in tickRes);
            EvaluateTrack(assetTrack, target, 0, in tickRes, registry);
            TtUnitTestManager.TAssert(SameName(target.Asset, texA), "第一个关键帧之前仍然不覆盖");
            EvaluateTrack(assetTrack, target, 1200, in tickRes, registry);
            TtUnitTestManager.TAssert(SameName(target.Asset, texB), "到点换成关键帧的资产");

            // Muted 轨道不参与求值, 但数据保留
            target.Asset = texA;
            assetTrack.Muted = true;
            EvaluateTrack(assetTrack, target, 1200, in tickRes, registry);
            TtUnitTestManager.TAssert(SameName(target.Asset, texA), "Muted 轨道不写目标");
        }
        static void EvaluateTrack(TtPropertyTrack track, object target, long time, in TtFrameRate tickRes, TtSequencePropertyRegistry registry)
        {
            var table = new TtSequenceEvalTable();
            var ctx = new FSectionEvalContext()
            {
                Time = time,
                Weight = 1.0f,
                TickResolution = tickRes,
                Table = table,
                Registry = registry,
            };
            track.Evaluate(in ctx, target);
            table.Flush(null);
        }

        static TtSequence BuildTestSequence()
        {
            var seq = new TtSequence();
            seq.PlaybackStartTick = 0;
            seq.PlaybackEndTick = 48000;

            var binding = seq.GetOrCreateBinding(Guid.Empty, "Root/Cube01", "Cube01");
            var track = binding.GetOrCreateTransformTrack();
            var section = new TtTransformSection()
            {
                StartTick = 0,
                EndTick = 48000,
                EaseInDuration = 800,
                OverlapPriority = 3,
                CompletionMode = ESectionCompletionMode.RestoreState,
            };
            section.PositionZ.AddKey(0, new FScalarKey(0.0) { InterpMode = EChannelInterpMode.Linear });
            section.PositionZ.AddKey(24000, new FScalarKey(5.0) { InterpMode = EChannelInterpMode.Cubic, TangentMode = EChannelTangentMode.User, ArriveTangent = 1.5, LeaveTangent = 2.5 });
            section.PositionZ.AddKey(48000, new FScalarKey(10.0));
            section.ScaleX.AddKey(0, 1.0);
            section.Rotation.AddKey(0, Quaternion.Identity);
            section.Rotation.AddKey(48000, Quaternion.RotationAxis(Vector3.UnitZ, MathHelper.PI * 0.5f));
            track.AddSection(section);
            return seq;
        }
        void TestSerializeRoundTrip()
        {
            var rn = RName.GetRName("UTest/sequencer_t0.xnd");
            IO.TtFileManager.SureDirectory(RName.GetRName("UTest").Address);

            var src = BuildTestSequence();
            {
                var typeStr = Rtti.TtTypeDesc.TypeOf(src.GetType()).TypeString;
                var xnd = new IO.TtXndHolder(typeStr, 0, 0);
                using (var attr = xnd.NewAttribute("Sequence", 0, 0))
                {
                    using (var ar = attr.GetWriter(512))
                    {
                        ar.Write(src);
                    }
                    xnd.RootNode.AddAttribute(attr);
                }
                xnd.SaveXnd(rn.Address);
            }

            TtSequence loaded = null;
            using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
            {
                TtUnitTestManager.TAssert(xnd != null, "xnd 存盘后应能载入");
                if (xnd == null)
                    return;
                loaded = TtSequence.LoadXnd(new TtSequenceManager(), xnd);
            }
            TtUnitTestManager.TAssert(loaded != null, "序列读回不为 null");
            if (loaded == null)
                return;

            // 时基是 blit 的结构体属性
            TtUnitTestManager.TAssert(loaded.TickResolution.Numerator == src.TickResolution.Numerator, "TickResolution 往返");
            TtUnitTestManager.TAssert(loaded.DisplayRate.Numerator == src.DisplayRate.Numerator, "DisplayRate 往返");
            TtUnitTestManager.TAssert(loaded.PlaybackEndTick == src.PlaybackEndTick, "PlaybackEndTick 往返");

            TtUnitTestManager.TAssert(loaded.Bindings.Count == 1, "Binding 数量往返");
            var srcBinding = src.Bindings[0];
            var dstBinding = loaded.Bindings[0];
            TtUnitTestManager.TAssert(dstBinding.BindingId == srcBinding.BindingId, "Guid 往返");
            TtUnitTestManager.TAssert(dstBinding.RelativePath == srcBinding.RelativePath, "相对路径往返");
            TtUnitTestManager.TAssert(dstBinding.Tracks.Count == 1, "轨道数量往返");

            // 列表元素声明成抽象基类, 读回来必须还是原来的派生类
            var dstTrack = dstBinding.Tracks[0] as TtTransformTrack;
            TtUnitTestManager.TAssert(dstTrack != null, "轨道多态往返");
            if (dstTrack == null)
                return;
            TtUnitTestManager.TAssert(dstTrack.Sections.Count == 1, "片段数量往返");
            var dstSection = dstTrack.Sections[0] as TtTransformSection;
            TtUnitTestManager.TAssert(dstSection != null, "片段多态往返");
            if (dstSection == null)
                return;

            var srcSection = src.Bindings[0].Tracks[0].Sections[0] as TtTransformSection;
            TtUnitTestManager.TAssert(dstSection.StartTick == srcSection.StartTick, "StartTick 往返");
            TtUnitTestManager.TAssert(dstSection.EndTick == srcSection.EndTick, "EndTick 往返");
            TtUnitTestManager.TAssert(dstSection.EaseInDuration == srcSection.EaseInDuration, "EaseInDuration 往返");
            TtUnitTestManager.TAssert(dstSection.OverlapPriority == srcSection.OverlapPriority, "OverlapPriority 往返");
            TtUnitTestManager.TAssert(dstSection.CompletionMode == srcSection.CompletionMode, "枚举往返");

            // List<long> 与 List<可 blit 结构体>: 这是本测试最主要的目的
            var srcZ = srcSection.PositionZ;
            var dstZ = dstSection.PositionZ;
            TtUnitTestManager.TAssert(dstZ.KeyCount == srcZ.KeyCount, "List<long> 关键帧数量往返");
            for (int i = 0; i < srcZ.KeyCount; ++i)
            {
                TtUnitTestManager.TAssert(dstZ.GetKeyTime(i) == srcZ.GetKeyTime(i), "List<long> 元素往返");
                var sk = srcZ.GetKey(i);
                var dk = dstZ.GetKey(i);
                TtUnitTestManager.TAssert(Math.Abs(dk.Value - sk.Value) < 1e-9, "关键帧值往返");
                TtUnitTestManager.TAssert(Math.Abs(dk.ArriveTangent - sk.ArriveTangent) < 1e-9, "ArriveTangent 往返");
                TtUnitTestManager.TAssert(Math.Abs(dk.LeaveTangent - sk.LeaveTangent) < 1e-9, "LeaveTangent 往返");
                TtUnitTestManager.TAssert(dk.InterpMode == sk.InterpMode, "结构体里的枚举往返");
                TtUnitTestManager.TAssert(dk.TangentMode == sk.TangentMode, "TangentMode 往返");
            }
            // 空通道读回来也必须是可用的空通道, 不能是 null
            TtUnitTestManager.TAssert(dstSection.PositionX != null && dstSection.PositionX.KeyCount == 0, "空通道往返");
            TtUnitTestManager.TAssert(dstSection.ScaleX.KeyCount == 1, "ScaleX 往返");

            TtUnitTestManager.TAssert(dstSection.Rotation.KeyCount == 2, "四元数通道关键帧数量往返");
            for (int i = 0; i < srcSection.Rotation.KeyCount; ++i)
            {
                TtUnitTestManager.TAssert(dstSection.Rotation.Times[i] == srcSection.Rotation.Times[i], "四元数通道时刻往返");
                TtUnitTestManager.TAssert(QuatNear(dstSection.Rotation.Values[i], srcSection.Rotation.Values[i]), "四元数往返");
            }

            // 读回来的序列求值结果必须和原序列一致
            var tickRes = loaded.TickResolution;
            for (long t = 0; t <= 48000; t += 4800)
            {
                var a = srcZ.Evaluate(t, tickRes);
                var b = dstZ.Evaluate(t, tickRes);
                TtUnitTestManager.TAssert(Math.Abs(a - b) < 1e-9, "往返后求值结果一致");
            }
        }
    }
}
