// Engine!

// 名称:ui_main
namespace samplers.mergeinstance
{
    
    [EngineNS.Macross.MacrossTypeClassAttribute()]
    [EngineNS.Editor.Editor_MacrossClassAttribute(EngineNS.ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Createable |  EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter)]
    [EngineNS.UISystem.Editor_UIControlInitAttribute(typeof(ui_main_Initializer))]
    [EngineNS.UISystem.Editor_UIControlAttribute("自定义控件.samplers.mergeinstance.ui_main", "", "UserWidget.png")]
    public partial class ui_main : EngineNS.UISystem.Controls.UserControl, EngineNS.Macross.IMacrossType
    {
        public static EngineNS.Profiler.TimeScope _mScope = EngineNS.Profiler.TimeScopeManager.GetTimeScope("samplers.mergeinstance.ui_main");
        public ui_main()
        {
        }
        public ui_main(bool init)
        {
        }
        private static DebugContext_ui_main mDebuggerContext
        {
            get
            {
                if (((mDebugHolder == null) 
                            || (mDebugHolder.Context == null)))
                {
                    mDebugHolder = new EngineNS.Macross.MacrossDataManager.MacrossDebugContextHolder(new DebugContext_ui_main());
                }
                return ((mDebugHolder.Context) as DebugContext_ui_main);
            }
        }
        [System.ThreadStaticAttribute()]
        private static EngineNS.Macross.MacrossDataManager.MacrossDebugContextHolder mDebugHolder;
        public virtual int Version
        {
            get
            {
                return 34;
            }
        }
#pragma warning disable 1998
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_Guid("1a254c81-32b6-478f-a5e4-200d522856a3")]
        public void OnProcessValue___1a254c81_32b6_478f_a5e4_200d522856a3(float x, float y, float deltaX, float deltaY)
        {
            float arithmetic_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d_Value1 = new float();
            float arithmeticResult_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d = new float();
            System.Type param_7aa900d7_952d_488d_aace_f419b7cf8651_type = typeof(EngineNS.GamePlay.Controller.GPlayerController);
            EngineNS.GamePlay.Controller.GPlayerController methodReturnValue_7aa900d7_952d_488d_aace_f419b7cf8651 = null;
            System.Type param_bdce1cb1_5095_4a62_9974_668f5a5969b9_type = typeof(samplers.mergeinstance.mergeinstance);
            samplers.mergeinstance.mergeinstance methodReturnValue_bdce1cb1_5095_4a62_9974_668f5a5969b9 = null;
            try
            {
                _mScope.Begin();
#if MacrossDebug
                mDebuggerContext.ParamPin_f5ed0d01_e5a6_4674_8d05_789a971da7f2 = x;
                mDebuggerContext.ParamPin_c07407fe_b7cd_4e1f_9105_c8eb91121f9c = y;
                mDebuggerContext.ParamPin_2b115774_d6f9_4b4e_abf9_3ea0977cff2f = deltaX;
                mDebuggerContext.ParamPin_ed71c0e6_3c85_4f43_a89b_d42165339bdb = deltaY;
                if (BreakEnable_df801aa5_d866_41e4_aea8_d4d698959da1)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("1a254c81-32b6-478f-a5e4-200d522856a3");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("df801aa5-d866-41e4-aea8-d4d698959da1");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    x = ((System.Single)(mDebuggerContext.ParamPin_f5ed0d01_e5a6_4674_8d05_789a971da7f2));
                    y = ((System.Single)(mDebuggerContext.ParamPin_c07407fe_b7cd_4e1f_9105_c8eb91121f9c));
                    deltaX = ((System.Single)(mDebuggerContext.ParamPin_2b115774_d6f9_4b4e_abf9_3ea0977cff2f));
                    deltaY = ((System.Single)(mDebuggerContext.ParamPin_ed71c0e6_3c85_4f43_a89b_d42165339bdb));
                }
#endif
#if MacrossDebug
                mDebuggerContext.ParamPin_bd1bcade_7d5c_4e96_89ac_bd5f5bf685fa = param_bdce1cb1_5095_4a62_9974_668f5a5969b9_type;
                if (BreakEnable_bdce1cb1_5095_4a62_9974_668f5a5969b9)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("1a254c81-32b6-478f-a5e4-200d522856a3");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("bdce1cb1-5095-4a62-9974-668f5a5969b9");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_bdce1cb1_5095_4a62_9974_668f5a5969b9_type = ((mDebuggerContext.ParamPin_bd1bcade_7d5c_4e96_89ac_bd5f5bf685fa) as System.Type);
                }
#endif
                methodReturnValue_bdce1cb1_5095_4a62_9974_668f5a5969b9 = ((EngineNS.GamePlay.Actor.McActor.GetGameInstance(param_bdce1cb1_5095_4a62_9974_668f5a5969b9_type)) as samplers.mergeinstance.mergeinstance);
#if MacrossDebug
                mDebuggerContext.returnLink_bdce1cb1_5095_4a62_9974_668f5a5969b9 = methodReturnValue_bdce1cb1_5095_4a62_9974_668f5a5969b9;
#endif
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_808b0313_9d0b_4571_b361_ae52f33fef43 = methodReturnValue_bdce1cb1_5095_4a62_9974_668f5a5969b9.MainActor;
#endif
#if MacrossDebug
                mDebuggerContext.ParamPin_4e9d4834_f0b3_4f78_afcf_dcbc3b1c7a69 = param_7aa900d7_952d_488d_aace_f419b7cf8651_type;
                if (BreakEnable_7aa900d7_952d_488d_aace_f419b7cf8651)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("1a254c81-32b6-478f-a5e4-200d522856a3");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("7aa900d7-952d-488d-aace-f419b7cf8651");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_7aa900d7_952d_488d_aace_f419b7cf8651_type = ((mDebuggerContext.ParamPin_4e9d4834_f0b3_4f78_afcf_dcbc3b1c7a69) as System.Type);
                }
#endif
                methodReturnValue_7aa900d7_952d_488d_aace_f419b7cf8651 = ((methodReturnValue_bdce1cb1_5095_4a62_9974_668f5a5969b9.MainActor.GetComponent(param_7aa900d7_952d_488d_aace_f419b7cf8651_type)) as EngineNS.GamePlay.Controller.GPlayerController);
#if MacrossDebug
                mDebuggerContext.returnLink_7aa900d7_952d_488d_aace_f419b7cf8651 = methodReturnValue_7aa900d7_952d_488d_aace_f419b7cf8651;
#endif
#if MacrossDebug
                mDebuggerContext.ValueInHandle_d33ed683_5df4_4065_9b69_40da5776ee80 = methodReturnValue_7aa900d7_952d_488d_aace_f419b7cf8651.HorizontalInput;
                if (BreakEnable_d33ed683_5df4_4065_9b69_40da5776ee80)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("1a254c81-32b6-478f-a5e4-200d522856a3");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("d33ed683-5df4-4065-9b69-40da5776ee80");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                methodReturnValue_7aa900d7_952d_488d_aace_f419b7cf8651.HorizontalInput = ((System.Single)(x));
#if MacrossDebug
                mDebuggerContext.ValueInHandle_d33ed683_5df4_4065_9b69_40da5776ee80 = methodReturnValue_7aa900d7_952d_488d_aace_f419b7cf8651.HorizontalInput;
#endif
#if MacrossDebug
                mDebuggerContext.ValueInHandle_3fe9dae4_e58e_4318_b740_b868535ca268 = methodReturnValue_7aa900d7_952d_488d_aace_f419b7cf8651.VerticalInput;
                if (BreakEnable_3fe9dae4_e58e_4318_b740_b868535ca268)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("1a254c81-32b6-478f-a5e4-200d522856a3");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("3fe9dae4-e58e-4318-b740-b868535ca268");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                arithmetic_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d_Value1 = y;
#if MacrossDebug
                mDebuggerContext.Value1_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d = arithmetic_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d_Value1;
                mDebuggerContext.Value2_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d = -1;
                if (BreakEnable_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("1a254c81-32b6-478f-a5e4-200d522856a3");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("49eb4c6c-4781-47bb-ba0d-ee1d2ce0fe8d");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    arithmetic_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d_Value1 = ((System.Single)(mDebuggerContext.Value1_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d));
                }
#endif
                arithmeticResult_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d = (arithmetic_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d_Value1 * -1);
#if MacrossDebug
                mDebuggerContext.ResultLink_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d = arithmeticResult_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d;
#endif
                methodReturnValue_7aa900d7_952d_488d_aace_f419b7cf8651.VerticalInput = ((System.Single)(arithmeticResult_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d));
#if MacrossDebug
                mDebuggerContext.ValueInHandle_3fe9dae4_e58e_4318_b740_b868535ca268 = methodReturnValue_7aa900d7_952d_488d_aace_f419b7cf8651.VerticalInput;
#endif
            }
            catch (System.Exception ex_df801aa5_d866_41e4_aea8_d4d698959da1)
            {
                EngineNS.Profiler.Log.WriteException(ex_df801aa5_d866_41e4_aea8_d4d698959da1, "Macross异常");
            }
            finally
            {
                _mScope.End();
            }
        }
#pragma warning restore 1998
        public static bool BreakEnable_df801aa5_d866_41e4_aea8_d4d698959da1 = false;
        public static bool BreakEnable_bdce1cb1_5095_4a62_9974_668f5a5969b9 = false;
        public static bool BreakEnable_7aa900d7_952d_488d_aace_f419b7cf8651 = false;
        public static bool BreakEnable_d33ed683_5df4_4065_9b69_40da5776ee80 = false;
        public static bool BreakEnable_3fe9dae4_e58e_4318_b740_b868535ca268 = false;
        public static bool BreakEnable_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d = false;
#pragma warning disable 1998
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_Guid("d11e4a04-282f-430e-8f86-ed4f50449eb0")]
        public void OnClick___d11e4a04_282f_430e_8f86_ed4f50449eb0(EngineNS.UISystem.UIElement ui, EngineNS.UISystem.RoutedEventArgs args)
        {
            System.Type param_2631be98_0a9c_4428_be1c_c7f5bbd255d0_type = typeof(samplers.mergeinstance.mergeinstance);
            samplers.mergeinstance.mergeinstance methodReturnValue_2631be98_0a9c_4428_be1c_c7f5bbd255d0 = null;
            try
            {
                _mScope.Begin();
#if MacrossDebug
                mDebuggerContext.ParamPin_4bc4890c_767e_4bc7_8123_94907b92e5e7 = ui;
                mDebuggerContext.ParamPin_4b1916cc_102c_4a2a_ad6c_2c41317862f0 = args;
                if (BreakEnable_8a416047_e9f3_4697_9d42_9235b195fdbd)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("d11e4a04-282f-430e-8f86-ed4f50449eb0");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("8a416047-e9f3-4697-9d42-9235b195fdbd");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    ui = ((mDebuggerContext.ParamPin_4bc4890c_767e_4bc7_8123_94907b92e5e7) as EngineNS.UISystem.UIElement);
                    args = ((mDebuggerContext.ParamPin_4b1916cc_102c_4a2a_ad6c_2c41317862f0) as EngineNS.UISystem.RoutedEventArgs);
                }
#endif
#if MacrossDebug
                mDebuggerContext.ParamPin_aaf2f186_30b6_4a09_a6a1_1c0938d17227 = param_2631be98_0a9c_4428_be1c_c7f5bbd255d0_type;
                if (BreakEnable_2631be98_0a9c_4428_be1c_c7f5bbd255d0)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("d11e4a04-282f-430e-8f86-ed4f50449eb0");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("2631be98-0a9c-4428-be1c-c7f5bbd255d0");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_2631be98_0a9c_4428_be1c_c7f5bbd255d0_type = ((mDebuggerContext.ParamPin_aaf2f186_30b6_4a09_a6a1_1c0938d17227) as System.Type);
                }
#endif
                methodReturnValue_2631be98_0a9c_4428_be1c_c7f5bbd255d0 = ((EngineNS.GamePlay.Actor.McActor.GetGameInstance(param_2631be98_0a9c_4428_be1c_c7f5bbd255d0_type)) as samplers.mergeinstance.mergeinstance);
#if MacrossDebug
                mDebuggerContext.returnLink_2631be98_0a9c_4428_be1c_c7f5bbd255d0 = methodReturnValue_2631be98_0a9c_4428_be1c_c7f5bbd255d0;
#endif
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_69215858_2694_40eb_b0cb_c65b75881c16 = methodReturnValue_2631be98_0a9c_4428_be1c_c7f5bbd255d0.SceneData;
#endif
#if MacrossDebug
                mDebuggerContext.ValueInHandle_7e2f3ca2_bdfe_410c_a9ea_32a46c8b529e = methodReturnValue_2631be98_0a9c_4428_be1c_c7f5bbd255d0.SceneData.EnableDraw;
                if (BreakEnable_7e2f3ca2_bdfe_410c_a9ea_32a46c8b529e)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("d11e4a04-282f-430e-8f86-ed4f50449eb0");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("7e2f3ca2-bdfe-410c-a9ea-32a46c8b529e");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                methodReturnValue_2631be98_0a9c_4428_be1c_c7f5bbd255d0.SceneData.EnableDraw = true;
#if MacrossDebug
                mDebuggerContext.ValueInHandle_7e2f3ca2_bdfe_410c_a9ea_32a46c8b529e = methodReturnValue_2631be98_0a9c_4428_be1c_c7f5bbd255d0.SceneData.EnableDraw;
#endif
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_c171b86a_3d3f_41cf_9ff4_840ef8aa1c5b = methodReturnValue_2631be98_0a9c_4428_be1c_c7f5bbd255d0.ListActors;
#endif
                var param_5b73be6f_7660_4b81_b6d7_bf9786658222 = methodReturnValue_2631be98_0a9c_4428_be1c_c7f5bbd255d0.ListActors;
#if MacrossDebug
                if (BreakEnable_5b73be6f_7660_4b81_b6d7_bf9786658222)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("d11e4a04-282f-430e-8f86-ed4f50449eb0");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("5b73be6f-7660-4b81-b6d7-bf9786658222");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
        for (int index_5b73be6f_7660_4b81_b6d7_bf9786658222 = 0; index_5b73be6f_7660_4b81_b6d7_bf9786658222 < param_5b73be6f_7660_4b81_b6d7_bf9786658222.Count; index_5b73be6f_7660_4b81_b6d7_bf9786658222++)        {
                EngineNS.GamePlay.Actor.GActor current_5b73be6f_7660_4b81_b6d7_bf9786658222 = param_5b73be6f_7660_4b81_b6d7_bf9786658222[index_5b73be6f_7660_4b81_b6d7_bf9786658222];
#if MacrossDebug
                mDebuggerContext.ValueInHandle_030fd285_4456_4fb7_818f_87a4ccdf5712 = current_5b73be6f_7660_4b81_b6d7_bf9786658222.Visible;
                if (BreakEnable_030fd285_4456_4fb7_818f_87a4ccdf5712)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("d11e4a04-282f-430e-8f86-ed4f50449eb0");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("030fd285-4456-4fb7-818f-87a4ccdf5712");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                current_5b73be6f_7660_4b81_b6d7_bf9786658222.Visible = false;
#if MacrossDebug
                mDebuggerContext.ValueInHandle_030fd285_4456_4fb7_818f_87a4ccdf5712 = current_5b73be6f_7660_4b81_b6d7_bf9786658222.Visible;
#endif
        }
            }
            catch (System.Exception ex_8a416047_e9f3_4697_9d42_9235b195fdbd)
            {
                EngineNS.Profiler.Log.WriteException(ex_8a416047_e9f3_4697_9d42_9235b195fdbd, "Macross异常");
            }
            finally
            {
                _mScope.End();
            }
        }
#pragma warning restore 1998
        public static bool BreakEnable_8a416047_e9f3_4697_9d42_9235b195fdbd = false;
        public static bool BreakEnable_2631be98_0a9c_4428_be1c_c7f5bbd255d0 = false;
        public static bool BreakEnable_7e2f3ca2_bdfe_410c_a9ea_32a46c8b529e = false;
        public static bool BreakEnable_5b73be6f_7660_4b81_b6d7_bf9786658222 = false;
        public static bool BreakEnable_030fd285_4456_4fb7_818f_87a4ccdf5712 = false;
#pragma warning disable 1998
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_Guid("7c4e1452-e8d7-4330-94bd-9400d2d50ae8")]
        public void OnClick___7c4e1452_e8d7_4330_94bd_9400d2d50ae8(EngineNS.UISystem.UIElement ui, EngineNS.UISystem.RoutedEventArgs args)
        {
            System.Type param_b8640db0_ff64_423d_a6db_2c44bbfa2f20_type = typeof(samplers.mergeinstance.mergeinstance);
            samplers.mergeinstance.mergeinstance methodReturnValue_b8640db0_ff64_423d_a6db_2c44bbfa2f20 = null;
            try
            {
                _mScope.Begin();
#if MacrossDebug
                mDebuggerContext.ParamPin_dc3bbdae_3117_4751_a6c4_792ad0e28965 = ui;
                mDebuggerContext.ParamPin_30bd72eb_7768_47d9_9690_cc15debf4db9 = args;
                if (BreakEnable_d14aa238_c194_4409_abdf_aa09e54dd807)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("7c4e1452-e8d7-4330-94bd-9400d2d50ae8");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("d14aa238-c194-4409-abdf-aa09e54dd807");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    ui = ((mDebuggerContext.ParamPin_dc3bbdae_3117_4751_a6c4_792ad0e28965) as EngineNS.UISystem.UIElement);
                    args = ((mDebuggerContext.ParamPin_30bd72eb_7768_47d9_9690_cc15debf4db9) as EngineNS.UISystem.RoutedEventArgs);
                }
#endif
#if MacrossDebug
                mDebuggerContext.ParamPin_0b835b57_ef1c_4285_bdc3_670bf938a596 = param_b8640db0_ff64_423d_a6db_2c44bbfa2f20_type;
                if (BreakEnable_b8640db0_ff64_423d_a6db_2c44bbfa2f20)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("7c4e1452-e8d7-4330-94bd-9400d2d50ae8");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("b8640db0-ff64-423d-a6db-2c44bbfa2f20");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                    param_b8640db0_ff64_423d_a6db_2c44bbfa2f20_type = ((mDebuggerContext.ParamPin_0b835b57_ef1c_4285_bdc3_670bf938a596) as System.Type);
                }
#endif
                methodReturnValue_b8640db0_ff64_423d_a6db_2c44bbfa2f20 = ((EngineNS.GamePlay.Actor.McActor.GetGameInstance(param_b8640db0_ff64_423d_a6db_2c44bbfa2f20_type)) as samplers.mergeinstance.mergeinstance);
#if MacrossDebug
                mDebuggerContext.returnLink_b8640db0_ff64_423d_a6db_2c44bbfa2f20 = methodReturnValue_b8640db0_ff64_423d_a6db_2c44bbfa2f20;
#endif
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_9d276343_d9be_41e5_94cc_f536518353bf = methodReturnValue_b8640db0_ff64_423d_a6db_2c44bbfa2f20.SceneData;
#endif
#if MacrossDebug
                mDebuggerContext.ValueInHandle_40ef42f2_069b_4d8c_9596_09bbcc2859d1 = methodReturnValue_b8640db0_ff64_423d_a6db_2c44bbfa2f20.SceneData.EnableDraw;
                if (BreakEnable_40ef42f2_069b_4d8c_9596_09bbcc2859d1)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("7c4e1452-e8d7-4330-94bd-9400d2d50ae8");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("40ef42f2-069b-4d8c-9596-09bbcc2859d1");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                methodReturnValue_b8640db0_ff64_423d_a6db_2c44bbfa2f20.SceneData.EnableDraw = false;
#if MacrossDebug
                mDebuggerContext.ValueInHandle_40ef42f2_069b_4d8c_9596_09bbcc2859d1 = methodReturnValue_b8640db0_ff64_423d_a6db_2c44bbfa2f20.SceneData.EnableDraw;
#endif
#if MacrossDebug
                mDebuggerContext.ValueOutHandle_033153b7_77d3_4c3a_b647_e923a78a97b8 = methodReturnValue_b8640db0_ff64_423d_a6db_2c44bbfa2f20.ListActors;
#endif
                var param_cebdaae1_9854_4be6_b225_962a7f445511 = methodReturnValue_b8640db0_ff64_423d_a6db_2c44bbfa2f20.ListActors;
#if MacrossDebug
                if (BreakEnable_cebdaae1_9854_4be6_b225_962a7f445511)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("7c4e1452-e8d7-4330-94bd-9400d2d50ae8");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("cebdaae1-9854-4be6-b225-962a7f445511");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
        for (int index_cebdaae1_9854_4be6_b225_962a7f445511 = 0; index_cebdaae1_9854_4be6_b225_962a7f445511 < param_cebdaae1_9854_4be6_b225_962a7f445511.Count; index_cebdaae1_9854_4be6_b225_962a7f445511++)        {
                EngineNS.GamePlay.Actor.GActor current_cebdaae1_9854_4be6_b225_962a7f445511 = param_cebdaae1_9854_4be6_b225_962a7f445511[index_cebdaae1_9854_4be6_b225_962a7f445511];
#if MacrossDebug
                mDebuggerContext.ValueInHandle_9c2a64d1_6bc0_45b2_b8ae_3c048f83a5a3 = current_cebdaae1_9854_4be6_b225_962a7f445511.Visible;
                if (BreakEnable_9c2a64d1_6bc0_45b2_b8ae_3c048f83a5a3)
                {
                    EngineNS.Editor.Runner.RunnerManager.BreakContext breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
                    breakContext.ThisObject = this;
                    breakContext.DebuggerId = EngineNS.Rtti.RttiHelper.GuidTryParse("7c4e1452-e8d7-4330-94bd-9400d2d50ae8");
                    breakContext.BreakId = EngineNS.Rtti.RttiHelper.GuidTryParse("9c2a64d1-6bc0-45b2-b8ae-3c048f83a5a3");
                    breakContext.ClassName = "ui_main";
                    breakContext.ValueContext = mDebuggerContext;
                    EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
                }
#endif
                current_cebdaae1_9854_4be6_b225_962a7f445511.Visible = true;
#if MacrossDebug
                mDebuggerContext.ValueInHandle_9c2a64d1_6bc0_45b2_b8ae_3c048f83a5a3 = current_cebdaae1_9854_4be6_b225_962a7f445511.Visible;
#endif
        }
            }
            catch (System.Exception ex_d14aa238_c194_4409_abdf_aa09e54dd807)
            {
                EngineNS.Profiler.Log.WriteException(ex_d14aa238_c194_4409_abdf_aa09e54dd807, "Macross异常");
            }
            finally
            {
                _mScope.End();
            }
        }
#pragma warning restore 1998
        public static bool BreakEnable_d14aa238_c194_4409_abdf_aa09e54dd807 = false;
        public static bool BreakEnable_b8640db0_ff64_423d_a6db_2c44bbfa2f20 = false;
        public static bool BreakEnable_40ef42f2_069b_4d8c_9596_09bbcc2859d1 = false;
        public static bool BreakEnable_cebdaae1_9854_4be6_b225_962a7f445511 = false;
        public static bool BreakEnable_9c2a64d1_6bc0_45b2_b8ae_3c048f83a5a3 = false;
        private EngineNS.UISystem.Controls.Containers.Border mUI_AtlasShow;
        [System.ComponentModel.BrowsableAttribute(false)]
        [EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.PropReadOnly)]
        public virtual EngineNS.UISystem.Controls.Containers.Border AtlasShow
        {
            get
            {
                return this.mUI_AtlasShow;
            }
        }
        protected virtual async System.Threading.Tasks.Task InitControls(EngineNS.CRenderContext rc)
        {
            this.PropertyBindFunctions.Clear();
            this.VariableBindInfosDic.Clear();
            mChildrenUIElements.Clear();
            EngineNS.UISystem.Controls.Containers.CanvasPanelInitializer uiInit_1075375955 = new EngineNS.UISystem.Controls.Containers.CanvasPanelInitializer();
            uiInit_1075375955.Id = 1075375955;
            EngineNS.RectangleF DesignRect_1075375955;
            DesignRect_1075375955 = new EngineNS.RectangleF(0F, 0F, 1920F, 1080F);
            uiInit_1075375955.DesignRect = DesignRect_1075375955;
            EngineNS.RectangleF DesignClipRect_1075375955;
            DesignClipRect_1075375955 = new EngineNS.RectangleF(0F, 0F, 1920F, 1080F);
            uiInit_1075375955.DesignClipRect = DesignClipRect_1075375955;
            EngineNS.UISystem.Controls.Containers.BorderSlot Slot_1075375955;
            EngineNS.UISystem.Controls.Containers.BorderSlot temp_d1bcb3d7_4d88_484e_803b_c68a5db96575 = new EngineNS.UISystem.Controls.Containers.BorderSlot();
            Slot_1075375955 = temp_d1bcb3d7_4d88_484e_803b_c68a5db96575;
            uiInit_1075375955.Slot = Slot_1075375955;
            EngineNS.UISystem.Controls.Containers.CanvasPanel ui_1075375955 = new EngineNS.UISystem.Controls.Containers.CanvasPanel();
            await ui_1075375955.Initialize(rc, uiInit_1075375955);
            ui_1075375955.PropertyBindFunctions.Clear();
            ui_1075375955.VariableBindInfosDic.Clear();
            this.AddChild(ui_1075375955, false);
            EngineNS.UISystem.Controls.JoysticksInitializer uiInit_945687006 = new EngineNS.UISystem.Controls.JoysticksInitializer();
            EngineNS.UISystem.Brush BackgroundBrush_945687006;
            BackgroundBrush_945687006 = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_ef721eff_e474_4316_a045_7a97e8ab73ec;
            ImageName_ef721eff_e474_4316_a045_7a97e8ab73ec = EngineNS.RName.GetRName("ui/uv_ui_joysticks_background.uvanim", EngineNS.RName.enRNameType.Engine);
            BackgroundBrush_945687006.ImageName = ImageName_ef721eff_e474_4316_a045_7a97e8ab73ec;
            EngineNS.Vector2 ImageSize_5d1f8af2_5143_4bee_a373_a76c1e862803;
            ImageSize_5d1f8af2_5143_4bee_a373_a76c1e862803 = new EngineNS.Vector2(256F, 256F);
            BackgroundBrush_945687006.ImageSize = ImageSize_5d1f8af2_5143_4bee_a373_a76c1e862803;
            uiInit_945687006.BackgroundBrush = BackgroundBrush_945687006;
            uiInit_945687006.BackgroundHorizontalAlignment = EngineNS.UISystem.HorizontalAlignment.Left;
            uiInit_945687006.BackgroundVerticalAlignment = EngineNS.UISystem.VerticalAlignment.Bottom;
            EngineNS.Thickness BackgroundMargin_945687006;
            BackgroundMargin_945687006 = new EngineNS.Thickness(100F, 50F, 0F, 0F);
            uiInit_945687006.BackgroundMargin = BackgroundMargin_945687006;
            EngineNS.UISystem.Style.ButtonStyle ThumbStyle_945687006;
            ThumbStyle_945687006 = new EngineNS.UISystem.Style.ButtonStyle();
            EngineNS.UISystem.Brush NormalBrush_5f4f3b5c_16bf_43c8_b3c3_cca00f2a4e20;
            NormalBrush_5f4f3b5c_16bf_43c8_b3c3_cca00f2a4e20 = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_ffcb8947_127f_4106_9ba2_b8bbb3d0cc30;
            ImageName_ffcb8947_127f_4106_9ba2_b8bbb3d0cc30 = EngineNS.RName.GetRName("ui/uv_ui_joysticks_thumb.uvanim", EngineNS.RName.enRNameType.Engine);
            NormalBrush_5f4f3b5c_16bf_43c8_b3c3_cca00f2a4e20.ImageName = ImageName_ffcb8947_127f_4106_9ba2_b8bbb3d0cc30;
            EngineNS.Vector2 ImageSize_41880152_b52a_4689_9f28_34bbed7b79c4;
            ImageSize_41880152_b52a_4689_9f28_34bbed7b79c4 = new EngineNS.Vector2(64F, 64F);
            NormalBrush_5f4f3b5c_16bf_43c8_b3c3_cca00f2a4e20.ImageSize = ImageSize_41880152_b52a_4689_9f28_34bbed7b79c4;
            ThumbStyle_945687006.NormalBrush = NormalBrush_5f4f3b5c_16bf_43c8_b3c3_cca00f2a4e20;
            EngineNS.UISystem.Brush HoveredBrush_533f3985_9ffd_4287_a115_7feefe0e2d9c;
            HoveredBrush_533f3985_9ffd_4287_a115_7feefe0e2d9c = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_8d04fda1_49d6_42b3_8225_07d3c2ba0993;
            ImageName_8d04fda1_49d6_42b3_8225_07d3c2ba0993 = EngineNS.RName.GetRName("ui/uv_ui_joysticks_thumb.uvanim", EngineNS.RName.enRNameType.Engine);
            HoveredBrush_533f3985_9ffd_4287_a115_7feefe0e2d9c.ImageName = ImageName_8d04fda1_49d6_42b3_8225_07d3c2ba0993;
            EngineNS.Vector2 ImageSize_aa871b55_b292_4767_8108_3f508e79ebba;
            ImageSize_aa871b55_b292_4767_8108_3f508e79ebba = new EngineNS.Vector2(64F, 64F);
            HoveredBrush_533f3985_9ffd_4287_a115_7feefe0e2d9c.ImageSize = ImageSize_aa871b55_b292_4767_8108_3f508e79ebba;
            ThumbStyle_945687006.HoveredBrush = HoveredBrush_533f3985_9ffd_4287_a115_7feefe0e2d9c;
            EngineNS.UISystem.Brush PressedBrush_0f46b439_b405_469c_90f9_785f800e51b2;
            PressedBrush_0f46b439_b405_469c_90f9_785f800e51b2 = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_1dbc6699_6f5b_417e_9656_a20a6a3e4774;
            ImageName_1dbc6699_6f5b_417e_9656_a20a6a3e4774 = EngineNS.RName.GetRName("ui/uv_ui_joysticks_thumb.uvanim", EngineNS.RName.enRNameType.Engine);
            PressedBrush_0f46b439_b405_469c_90f9_785f800e51b2.ImageName = ImageName_1dbc6699_6f5b_417e_9656_a20a6a3e4774;
            EngineNS.Vector2 ImageSize_cbe8ca0e_3d71_47cb_a5b1_3a22195b4978;
            ImageSize_cbe8ca0e_3d71_47cb_a5b1_3a22195b4978 = new EngineNS.Vector2(64F, 64F);
            PressedBrush_0f46b439_b405_469c_90f9_785f800e51b2.ImageSize = ImageSize_cbe8ca0e_3d71_47cb_a5b1_3a22195b4978;
            ThumbStyle_945687006.PressedBrush = PressedBrush_0f46b439_b405_469c_90f9_785f800e51b2;
            EngineNS.UISystem.Brush DisabledBrush_02fdbbdc_1208_4616_a0d3_65f762c9effe;
            DisabledBrush_02fdbbdc_1208_4616_a0d3_65f762c9effe = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_dd3ccc50_5918_4d82_a7c7_250a7bf84987;
            ImageName_dd3ccc50_5918_4d82_a7c7_250a7bf84987 = EngineNS.RName.GetRName("ui/uv_ui_joysticks_thumb.uvanim", EngineNS.RName.enRNameType.Engine);
            DisabledBrush_02fdbbdc_1208_4616_a0d3_65f762c9effe.ImageName = ImageName_dd3ccc50_5918_4d82_a7c7_250a7bf84987;
            EngineNS.Vector2 ImageSize_f841d03d_43a4_4628_8d8f_2eb422334dd1;
            ImageSize_f841d03d_43a4_4628_8d8f_2eb422334dd1 = new EngineNS.Vector2(64F, 64F);
            DisabledBrush_02fdbbdc_1208_4616_a0d3_65f762c9effe.ImageSize = ImageSize_f841d03d_43a4_4628_8d8f_2eb422334dd1;
            ThumbStyle_945687006.DisabledBrush = DisabledBrush_02fdbbdc_1208_4616_a0d3_65f762c9effe;
            uiInit_945687006.ThumbStyle = ThumbStyle_945687006;
            uiInit_945687006.Id = 945687006;
            uiInit_945687006.Name = "Joysticks_0";
            EngineNS.RectangleF DesignRect_945687006;
            DesignRect_945687006 = new EngineNS.RectangleF(133.0361F, 705.1003F, 285.8268F, 299.475F);
            uiInit_945687006.DesignRect = DesignRect_945687006;
            EngineNS.RectangleF DesignClipRect_945687006;
            DesignClipRect_945687006 = new EngineNS.RectangleF(133.0361F, 705.1003F, 285.8268F, 299.475F);
            uiInit_945687006.DesignClipRect = DesignClipRect_945687006;
            EngineNS.UISystem.Controls.Containers.CanvasSlot Slot_945687006;
            EngineNS.UISystem.Controls.Containers.CanvasSlot temp_61b5179c_e46d_46ef_a284_10b93d980553 = new EngineNS.UISystem.Controls.Containers.CanvasSlot();
            temp_61b5179c_e46d_46ef_a284_10b93d980553.X1 = 133.0361F;
            temp_61b5179c_e46d_46ef_a284_10b93d980553.Y1 = -374.8997F;
            temp_61b5179c_e46d_46ef_a284_10b93d980553.X2 = 285.8268F;
            temp_61b5179c_e46d_46ef_a284_10b93d980553.Y2 = 299.475F;
            EngineNS.Vector2 mMinimum_dd4a6323_629d_415c_b166_2f33cf2b2974;
            mMinimum_dd4a6323_629d_415c_b166_2f33cf2b2974 = new EngineNS.Vector2(0F, 1F);
            temp_61b5179c_e46d_46ef_a284_10b93d980553.mMinimum = mMinimum_dd4a6323_629d_415c_b166_2f33cf2b2974;
            EngineNS.Vector2 mMaximum_2267d359_7369_4193_8745_b5af33f5f46f;
            mMaximum_2267d359_7369_4193_8745_b5af33f5f46f = new EngineNS.Vector2(0F, 1F);
            temp_61b5179c_e46d_46ef_a284_10b93d980553.mMaximum = mMaximum_2267d359_7369_4193_8745_b5af33f5f46f;
            Slot_945687006 = temp_61b5179c_e46d_46ef_a284_10b93d980553;
            uiInit_945687006.Slot = Slot_945687006;
            EngineNS.UISystem.Controls.Joysticks ui_945687006 = new EngineNS.UISystem.Controls.Joysticks();
            await ui_945687006.Initialize(rc, uiInit_945687006);
            ui_945687006.OnProcessValue = OnProcessValue___1a254c81_32b6_478f_a5e4_200d522856a3;
            ui_945687006.PropertyBindFunctions.Clear();
            ui_945687006.VariableBindInfosDic.Clear();
            ui_1075375955.AddChild(ui_945687006, false);
            EngineNS.UISystem.Controls.ButtonInitializer uiInit_1405440519 = new EngineNS.UISystem.Controls.ButtonInitializer();
            EngineNS.UISystem.Style.ButtonStyle ButtonStyle_1405440519;
            ButtonStyle_1405440519 = new EngineNS.UISystem.Style.ButtonStyle();
            EngineNS.UISystem.Brush NormalBrush_352f2d7b_8065_42f3_947b_1f9011ab1c32;
            NormalBrush_352f2d7b_8065_42f3_947b_1f9011ab1c32 = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_b1721854_439d_47ae_a151_94cf3c3a190d;
            ImageName_b1721854_439d_47ae_a151_94cf3c3a190d = EngineNS.RName.GetRName("ui/uv_ui_button_normal.uvanim", EngineNS.RName.enRNameType.Engine);
            NormalBrush_352f2d7b_8065_42f3_947b_1f9011ab1c32.ImageName = ImageName_b1721854_439d_47ae_a151_94cf3c3a190d;
            EngineNS.Vector2 ImageSize_75fc09cc_aeea_4b54_9ffc_72697a273556;
            ImageSize_75fc09cc_aeea_4b54_9ffc_72697a273556 = new EngineNS.Vector2(250F, 58F);
            NormalBrush_352f2d7b_8065_42f3_947b_1f9011ab1c32.ImageSize = ImageSize_75fc09cc_aeea_4b54_9ffc_72697a273556;
            ButtonStyle_1405440519.NormalBrush = NormalBrush_352f2d7b_8065_42f3_947b_1f9011ab1c32;
            EngineNS.UISystem.Brush HoveredBrush_cf155968_5cf3_4f53_a8a0_f7382dd1470e;
            HoveredBrush_cf155968_5cf3_4f53_a8a0_f7382dd1470e = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_5f67bb6c_d325_4e92_8f2b_92e4cc37347a;
            ImageName_5f67bb6c_d325_4e92_8f2b_92e4cc37347a = EngineNS.RName.GetRName("ui/uv_ui_button_hovered.uvanim", EngineNS.RName.enRNameType.Engine);
            HoveredBrush_cf155968_5cf3_4f53_a8a0_f7382dd1470e.ImageName = ImageName_5f67bb6c_d325_4e92_8f2b_92e4cc37347a;
            EngineNS.Vector2 ImageSize_0f5f19dd_6e5d_4e45_9f4b_76fb88e3a8b7;
            ImageSize_0f5f19dd_6e5d_4e45_9f4b_76fb88e3a8b7 = new EngineNS.Vector2(250F, 58F);
            HoveredBrush_cf155968_5cf3_4f53_a8a0_f7382dd1470e.ImageSize = ImageSize_0f5f19dd_6e5d_4e45_9f4b_76fb88e3a8b7;
            ButtonStyle_1405440519.HoveredBrush = HoveredBrush_cf155968_5cf3_4f53_a8a0_f7382dd1470e;
            EngineNS.UISystem.Brush PressedBrush_a2739d2e_2e75_43ca_9869_89db4ee3203e;
            PressedBrush_a2739d2e_2e75_43ca_9869_89db4ee3203e = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_fb96fc65_afc2_4a24_b3ca_a32d39017cd3;
            ImageName_fb96fc65_afc2_4a24_b3ca_a32d39017cd3 = EngineNS.RName.GetRName("ui/uv_ui_button_pressed.uvanim", EngineNS.RName.enRNameType.Engine);
            PressedBrush_a2739d2e_2e75_43ca_9869_89db4ee3203e.ImageName = ImageName_fb96fc65_afc2_4a24_b3ca_a32d39017cd3;
            EngineNS.Vector2 ImageSize_1c309802_b17d_4009_8635_3cad8124ac36;
            ImageSize_1c309802_b17d_4009_8635_3cad8124ac36 = new EngineNS.Vector2(250F, 58F);
            PressedBrush_a2739d2e_2e75_43ca_9869_89db4ee3203e.ImageSize = ImageSize_1c309802_b17d_4009_8635_3cad8124ac36;
            ButtonStyle_1405440519.PressedBrush = PressedBrush_a2739d2e_2e75_43ca_9869_89db4ee3203e;
            EngineNS.UISystem.Brush DisabledBrush_0c9f72c4_ffd1_4c96_af3f_7ddacd141503;
            DisabledBrush_0c9f72c4_ffd1_4c96_af3f_7ddacd141503 = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_f0252ded_19c9_4690_8917_ef197ca83ba6;
            ImageName_f0252ded_19c9_4690_8917_ef197ca83ba6 = EngineNS.RName.GetRName("ui/uv_ui_button_disable.uvanim", EngineNS.RName.enRNameType.Engine);
            DisabledBrush_0c9f72c4_ffd1_4c96_af3f_7ddacd141503.ImageName = ImageName_f0252ded_19c9_4690_8917_ef197ca83ba6;
            EngineNS.Vector2 ImageSize_cbda6dc8_f304_448e_ace8_938f4e95ae5e;
            ImageSize_cbda6dc8_f304_448e_ace8_938f4e95ae5e = new EngineNS.Vector2(250F, 58F);
            DisabledBrush_0c9f72c4_ffd1_4c96_af3f_7ddacd141503.ImageSize = ImageSize_cbda6dc8_f304_448e_ace8_938f4e95ae5e;
            ButtonStyle_1405440519.DisabledBrush = DisabledBrush_0c9f72c4_ffd1_4c96_af3f_7ddacd141503;
            uiInit_1405440519.ButtonStyle = ButtonStyle_1405440519;
            uiInit_1405440519.Id = 1405440519;
            uiInit_1405440519.Name = "Button_1";
            EngineNS.RectangleF DesignRect_1405440519;
            DesignRect_1405440519 = new EngineNS.RectangleF(87.76904F, 39.94405F, 100F, 100F);
            uiInit_1405440519.DesignRect = DesignRect_1405440519;
            EngineNS.RectangleF DesignClipRect_1405440519;
            DesignClipRect_1405440519 = new EngineNS.RectangleF(87.76904F, 39.94405F, 100F, 100F);
            uiInit_1405440519.DesignClipRect = DesignClipRect_1405440519;
            EngineNS.UISystem.Controls.Containers.CanvasSlot Slot_1405440519;
            EngineNS.UISystem.Controls.Containers.CanvasSlot temp_a703f4e9_5038_4246_9eea_6b594ad58114 = new EngineNS.UISystem.Controls.Containers.CanvasSlot();
            temp_a703f4e9_5038_4246_9eea_6b594ad58114.X1 = 87.76904F;
            temp_a703f4e9_5038_4246_9eea_6b594ad58114.Y1 = 39.94405F;
            Slot_1405440519 = temp_a703f4e9_5038_4246_9eea_6b594ad58114;
            uiInit_1405440519.Slot = Slot_1405440519;
            EngineNS.UISystem.Controls.Button ui_1405440519 = new EngineNS.UISystem.Controls.Button();
            await ui_1405440519.Initialize(rc, uiInit_1405440519);
            ui_1405440519.OnClick = OnClick___d11e4a04_282f_430e_8f86_ed4f50449eb0;
            ui_1405440519.PropertyBindFunctions.Clear();
            ui_1405440519.VariableBindInfosDic.Clear();
            ui_1075375955.AddChild(ui_1405440519, false);
            EngineNS.UISystem.Controls.TextBlockInitializer uiInit_77626876 = new EngineNS.UISystem.Controls.TextBlockInitializer();
            uiInit_77626876.Text = "合并渲染";
            uiInit_77626876.Id = 77626876;
            uiInit_77626876.Name = "TextBlock_2";
            EngineNS.RectangleF DesignRect_77626876;
            DesignRect_77626876 = new EngineNS.RectangleF(87.76904F, 39.94405F, 100F, 100F);
            uiInit_77626876.DesignRect = DesignRect_77626876;
            EngineNS.RectangleF DesignClipRect_77626876;
            DesignClipRect_77626876 = new EngineNS.RectangleF(87.76904F, 39.94405F, 100F, 100F);
            uiInit_77626876.DesignClipRect = DesignClipRect_77626876;
            EngineNS.UISystem.Controls.Containers.BorderSlot Slot_77626876;
            EngineNS.UISystem.Controls.Containers.BorderSlot temp_75fb9d97_1ea8_458b_a06e_dfa1daa71b5e = new EngineNS.UISystem.Controls.Containers.BorderSlot();
            Slot_77626876 = temp_75fb9d97_1ea8_458b_a06e_dfa1daa71b5e;
            uiInit_77626876.Slot = Slot_77626876;
            EngineNS.UISystem.Controls.TextBlock ui_77626876 = new EngineNS.UISystem.Controls.TextBlock();
            await ui_77626876.Initialize(rc, uiInit_77626876);
            ui_77626876.PropertyBindFunctions.Clear();
            ui_77626876.VariableBindInfosDic.Clear();
            ui_1405440519.AddChild(ui_77626876, false);
            EngineNS.UISystem.Controls.ButtonInitializer uiInit_1740500430 = new EngineNS.UISystem.Controls.ButtonInitializer();
            EngineNS.UISystem.Style.ButtonStyle ButtonStyle_1740500430;
            ButtonStyle_1740500430 = new EngineNS.UISystem.Style.ButtonStyle();
            EngineNS.UISystem.Brush NormalBrush_8252aff7_3966_4de2_8985_bb6bcfe1cf15;
            NormalBrush_8252aff7_3966_4de2_8985_bb6bcfe1cf15 = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_72aefa9a_c2d8_41f4_b092_1895087af9fd;
            ImageName_72aefa9a_c2d8_41f4_b092_1895087af9fd = EngineNS.RName.GetRName("ui/uv_ui_button_normal.uvanim", EngineNS.RName.enRNameType.Engine);
            NormalBrush_8252aff7_3966_4de2_8985_bb6bcfe1cf15.ImageName = ImageName_72aefa9a_c2d8_41f4_b092_1895087af9fd;
            EngineNS.Vector2 ImageSize_261a14b3_4dc7_4ed4_9995_3fe86d1834c8;
            ImageSize_261a14b3_4dc7_4ed4_9995_3fe86d1834c8 = new EngineNS.Vector2(250F, 58F);
            NormalBrush_8252aff7_3966_4de2_8985_bb6bcfe1cf15.ImageSize = ImageSize_261a14b3_4dc7_4ed4_9995_3fe86d1834c8;
            ButtonStyle_1740500430.NormalBrush = NormalBrush_8252aff7_3966_4de2_8985_bb6bcfe1cf15;
            EngineNS.UISystem.Brush HoveredBrush_88593b83_c046_4ad3_b1d5_9bfdf2bec1f1;
            HoveredBrush_88593b83_c046_4ad3_b1d5_9bfdf2bec1f1 = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_15fc927a_7b99_430e_a523_7e41a5118492;
            ImageName_15fc927a_7b99_430e_a523_7e41a5118492 = EngineNS.RName.GetRName("ui/uv_ui_button_hovered.uvanim", EngineNS.RName.enRNameType.Engine);
            HoveredBrush_88593b83_c046_4ad3_b1d5_9bfdf2bec1f1.ImageName = ImageName_15fc927a_7b99_430e_a523_7e41a5118492;
            EngineNS.Vector2 ImageSize_e74c69ca_0fd2_43f4_bbdf_1b1324f1eaf5;
            ImageSize_e74c69ca_0fd2_43f4_bbdf_1b1324f1eaf5 = new EngineNS.Vector2(250F, 58F);
            HoveredBrush_88593b83_c046_4ad3_b1d5_9bfdf2bec1f1.ImageSize = ImageSize_e74c69ca_0fd2_43f4_bbdf_1b1324f1eaf5;
            ButtonStyle_1740500430.HoveredBrush = HoveredBrush_88593b83_c046_4ad3_b1d5_9bfdf2bec1f1;
            EngineNS.UISystem.Brush PressedBrush_821b8a74_09ca_4d56_ba2c_d1a32c9a53c0;
            PressedBrush_821b8a74_09ca_4d56_ba2c_d1a32c9a53c0 = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_4b34b166_806e_460d_913c_4b0085b9bfad;
            ImageName_4b34b166_806e_460d_913c_4b0085b9bfad = EngineNS.RName.GetRName("ui/uv_ui_button_pressed.uvanim", EngineNS.RName.enRNameType.Engine);
            PressedBrush_821b8a74_09ca_4d56_ba2c_d1a32c9a53c0.ImageName = ImageName_4b34b166_806e_460d_913c_4b0085b9bfad;
            EngineNS.Vector2 ImageSize_54b01853_78d7_4b4f_b6fa_ebd917c1a5ae;
            ImageSize_54b01853_78d7_4b4f_b6fa_ebd917c1a5ae = new EngineNS.Vector2(250F, 58F);
            PressedBrush_821b8a74_09ca_4d56_ba2c_d1a32c9a53c0.ImageSize = ImageSize_54b01853_78d7_4b4f_b6fa_ebd917c1a5ae;
            ButtonStyle_1740500430.PressedBrush = PressedBrush_821b8a74_09ca_4d56_ba2c_d1a32c9a53c0;
            EngineNS.UISystem.Brush DisabledBrush_f5e8bcb7_3a71_4257_b675_5fd604247c0a;
            DisabledBrush_f5e8bcb7_3a71_4257_b675_5fd604247c0a = new EngineNS.UISystem.Brush();
            EngineNS.RName ImageName_5f125c05_a2eb_4f0c_8470_0ac0e67107c4;
            ImageName_5f125c05_a2eb_4f0c_8470_0ac0e67107c4 = EngineNS.RName.GetRName("ui/uv_ui_button_disable.uvanim", EngineNS.RName.enRNameType.Engine);
            DisabledBrush_f5e8bcb7_3a71_4257_b675_5fd604247c0a.ImageName = ImageName_5f125c05_a2eb_4f0c_8470_0ac0e67107c4;
            EngineNS.Vector2 ImageSize_188844f6_5895_408f_95b9_169f277db653;
            ImageSize_188844f6_5895_408f_95b9_169f277db653 = new EngineNS.Vector2(250F, 58F);
            DisabledBrush_f5e8bcb7_3a71_4257_b675_5fd604247c0a.ImageSize = ImageSize_188844f6_5895_408f_95b9_169f277db653;
            ButtonStyle_1740500430.DisabledBrush = DisabledBrush_f5e8bcb7_3a71_4257_b675_5fd604247c0a;
            uiInit_1740500430.ButtonStyle = ButtonStyle_1740500430;
            uiInit_1740500430.Id = 1740500430;
            uiInit_1740500430.Name = "Button_3";
            EngineNS.RectangleF DesignRect_1740500430;
            DesignRect_1740500430 = new EngineNS.RectangleF(246.9991F, 36.79443F, 100F, 100F);
            uiInit_1740500430.DesignRect = DesignRect_1740500430;
            EngineNS.RectangleF DesignClipRect_1740500430;
            DesignClipRect_1740500430 = new EngineNS.RectangleF(246.9991F, 36.79443F, 100F, 100F);
            uiInit_1740500430.DesignClipRect = DesignClipRect_1740500430;
            EngineNS.UISystem.Controls.Containers.CanvasSlot Slot_1740500430;
            EngineNS.UISystem.Controls.Containers.CanvasSlot temp_3f845fe4_437a_4dd4_aaef_df855513c839 = new EngineNS.UISystem.Controls.Containers.CanvasSlot();
            temp_3f845fe4_437a_4dd4_aaef_df855513c839.X1 = 246.9991F;
            temp_3f845fe4_437a_4dd4_aaef_df855513c839.Y1 = 36.79443F;
            Slot_1740500430 = temp_3f845fe4_437a_4dd4_aaef_df855513c839;
            uiInit_1740500430.Slot = Slot_1740500430;
            EngineNS.UISystem.Controls.Button ui_1740500430 = new EngineNS.UISystem.Controls.Button();
            await ui_1740500430.Initialize(rc, uiInit_1740500430);
            ui_1740500430.OnClick = OnClick___7c4e1452_e8d7_4330_94bd_9400d2d50ae8;
            ui_1740500430.PropertyBindFunctions.Clear();
            ui_1740500430.VariableBindInfosDic.Clear();
            ui_1075375955.AddChild(ui_1740500430, false);
            EngineNS.UISystem.Controls.TextBlockInitializer uiInit_1222961994 = new EngineNS.UISystem.Controls.TextBlockInitializer();
            uiInit_1222961994.Text = "分开渲染";
            uiInit_1222961994.Id = 1222961994;
            uiInit_1222961994.Name = "TextBlock_4";
            EngineNS.RectangleF DesignRect_1222961994;
            DesignRect_1222961994 = new EngineNS.RectangleF(246.9991F, 36.79443F, 100F, 100F);
            uiInit_1222961994.DesignRect = DesignRect_1222961994;
            EngineNS.RectangleF DesignClipRect_1222961994;
            DesignClipRect_1222961994 = new EngineNS.RectangleF(246.9991F, 36.79443F, 100F, 100F);
            uiInit_1222961994.DesignClipRect = DesignClipRect_1222961994;
            EngineNS.UISystem.Controls.Containers.BorderSlot Slot_1222961994;
            EngineNS.UISystem.Controls.Containers.BorderSlot temp_b943727d_5216_4c1c_bca0_bae96860bc0d = new EngineNS.UISystem.Controls.Containers.BorderSlot();
            Slot_1222961994 = temp_b943727d_5216_4c1c_bca0_bae96860bc0d;
            uiInit_1222961994.Slot = Slot_1222961994;
            EngineNS.UISystem.Controls.TextBlock ui_1222961994 = new EngineNS.UISystem.Controls.TextBlock();
            await ui_1222961994.Initialize(rc, uiInit_1222961994);
            ui_1222961994.PropertyBindFunctions.Clear();
            ui_1222961994.VariableBindInfosDic.Clear();
            ui_1740500430.AddChild(ui_1222961994, false);
            EngineNS.UISystem.Controls.Containers.BorderInitializer uiInit__561430175 = new EngineNS.UISystem.Controls.Containers.BorderInitializer();
            uiInit__561430175.Id = -561430175;
            uiInit__561430175.Name = "AtlasShow";
            uiInit__561430175.IsVariable = true;
            EngineNS.RectangleF DesignRect__561430175;
            DesignRect__561430175 = new EngineNS.RectangleF(896F, 0F, 1024F, 1024F);
            uiInit__561430175.DesignRect = DesignRect__561430175;
            EngineNS.RectangleF DesignClipRect__561430175;
            DesignClipRect__561430175 = new EngineNS.RectangleF(896F, 0F, 1024F, 1024F);
            uiInit__561430175.DesignClipRect = DesignClipRect__561430175;
            EngineNS.UISystem.Controls.Containers.CanvasSlot Slot__561430175;
            EngineNS.UISystem.Controls.Containers.CanvasSlot temp_a5210994_ce19_41a3_aee7_8ae2bfd50685 = new EngineNS.UISystem.Controls.Containers.CanvasSlot();
            temp_a5210994_ce19_41a3_aee7_8ae2bfd50685.X1 = -1024F;
            temp_a5210994_ce19_41a3_aee7_8ae2bfd50685.X2 = 1024F;
            temp_a5210994_ce19_41a3_aee7_8ae2bfd50685.Y2 = 1024F;
            EngineNS.Vector2 mMinimum_aab4ad6d_475d_4a7a_a6e6_787fd8e94a9c;
            mMinimum_aab4ad6d_475d_4a7a_a6e6_787fd8e94a9c = new EngineNS.Vector2(1F, 0F);
            temp_a5210994_ce19_41a3_aee7_8ae2bfd50685.mMinimum = mMinimum_aab4ad6d_475d_4a7a_a6e6_787fd8e94a9c;
            EngineNS.Vector2 mMaximum_f8877589_70e0_48c7_b067_7c3145dd5d7e;
            mMaximum_f8877589_70e0_48c7_b067_7c3145dd5d7e = new EngineNS.Vector2(1F, 0F);
            temp_a5210994_ce19_41a3_aee7_8ae2bfd50685.mMaximum = mMaximum_f8877589_70e0_48c7_b067_7c3145dd5d7e;
            Slot__561430175 = temp_a5210994_ce19_41a3_aee7_8ae2bfd50685;
            uiInit__561430175.Slot = Slot__561430175;
            EngineNS.UISystem.Controls.Containers.Border ui__561430175 = new EngineNS.UISystem.Controls.Containers.Border();
            await ui__561430175.Initialize(rc, uiInit__561430175);
            this.mUI_AtlasShow = ui__561430175;
            ui__561430175.PropertyBindFunctions.Clear();
            ui__561430175.VariableBindInfosDic.Clear();
            ui_1075375955.AddChild(ui__561430175, false);
            EngineNS.UISystem.Controls.ImageInitializer uiInit_2021670274 = new EngineNS.UISystem.Controls.ImageInitializer();
            EngineNS.UISystem.Brush ImageBrush_2021670274;
            ImageBrush_2021670274 = new EngineNS.UISystem.Brush();
            EngineNS.Vector2 ImageSize_36335d69_8cd7_4b11_8862_cc1c5615d850;
            ImageSize_36335d69_8cd7_4b11_8862_cc1c5615d850 = new EngineNS.Vector2(128F, 128F);
            ImageBrush_2021670274.ImageSize = ImageSize_36335d69_8cd7_4b11_8862_cc1c5615d850;
            uiInit_2021670274.ImageBrush = ImageBrush_2021670274;
            uiInit_2021670274.Id = 2021670274;
            uiInit_2021670274.Name = "Image_5";
            EngineNS.RectangleF DesignRect_2021670274;
            DesignRect_2021670274 = new EngineNS.RectangleF(157.7603F, 207.923F, 128F, 128F);
            uiInit_2021670274.DesignRect = DesignRect_2021670274;
            EngineNS.RectangleF DesignClipRect_2021670274;
            DesignClipRect_2021670274 = new EngineNS.RectangleF(157.7603F, 207.923F, 128F, 128F);
            uiInit_2021670274.DesignClipRect = DesignClipRect_2021670274;
            EngineNS.UISystem.Controls.Containers.CanvasSlot Slot_2021670274;
            EngineNS.UISystem.Controls.Containers.CanvasSlot temp_f9773560_49dd_4b92_b4b1_ce24344703b6 = new EngineNS.UISystem.Controls.Containers.CanvasSlot();
            temp_f9773560_49dd_4b92_b4b1_ce24344703b6.X1 = 157.7603F;
            temp_f9773560_49dd_4b92_b4b1_ce24344703b6.Y1 = 207.923F;
            temp_f9773560_49dd_4b92_b4b1_ce24344703b6.X2 = 128F;
            temp_f9773560_49dd_4b92_b4b1_ce24344703b6.Y2 = 128F;
            Slot_2021670274 = temp_f9773560_49dd_4b92_b4b1_ce24344703b6;
            uiInit_2021670274.Slot = Slot_2021670274;
            EngineNS.UISystem.Controls.Image ui_2021670274 = new EngineNS.UISystem.Controls.Image();
            await ui_2021670274.Initialize(rc, uiInit_2021670274);
            ui_2021670274.PropertyBindFunctions.Clear();
            ui_2021670274.VariableBindInfosDic.Clear();
            ui_1075375955.AddChild(ui_2021670274, false);
            EngineNS.UISystem.Controls.TextBlockInitializer uiInit__1467831012 = new EngineNS.UISystem.Controls.TextBlockInitializer();
            uiInit__1467831012.Id = -1467831012;
            uiInit__1467831012.Name = "TextBlock_6";
            EngineNS.RectangleF DesignRect__1467831012;
            DesignRect__1467831012 = new EngineNS.RectangleF(167.909F, 221.2214F, 100F, 100F);
            uiInit__1467831012.DesignRect = DesignRect__1467831012;
            EngineNS.RectangleF DesignClipRect__1467831012;
            DesignClipRect__1467831012 = new EngineNS.RectangleF(167.909F, 221.2214F, 100F, 100F);
            uiInit__1467831012.DesignClipRect = DesignClipRect__1467831012;
            EngineNS.UISystem.Controls.Containers.CanvasSlot Slot__1467831012;
            EngineNS.UISystem.Controls.Containers.CanvasSlot temp_7f627a2e_e464_480b_bec6_d1b002f49fef = new EngineNS.UISystem.Controls.Containers.CanvasSlot();
            temp_7f627a2e_e464_480b_bec6_d1b002f49fef.X1 = 167.909F;
            temp_7f627a2e_e464_480b_bec6_d1b002f49fef.Y1 = 221.2214F;
            Slot__1467831012 = temp_7f627a2e_e464_480b_bec6_d1b002f49fef;
            uiInit__1467831012.Slot = Slot__1467831012;
            EngineNS.UISystem.Controls.TextBlock ui__1467831012 = new EngineNS.UISystem.Controls.TextBlock();
            await ui__1467831012.Initialize(rc, uiInit__1467831012);
            ui__1467831012.PropertyBindFunctions.Clear();
            ui__1467831012.VariableBindInfosDic.Clear();
            ui_1075375955.AddChild(ui__1467831012, false);
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(EngineNS.CRenderContext rc, EngineNS.UISystem.UIElementInitializer init)
        {
            await this.InitControls(rc);
            return await base.Initialize(rc, init);
        }
    }
    public class DebugContext_ui_main
    {
        public float ParamPin_f5ed0d01_e5a6_4674_8d05_789a971da7f2;
        public float ParamPin_c07407fe_b7cd_4e1f_9105_c8eb91121f9c;
        public float ParamPin_2b115774_d6f9_4b4e_abf9_3ea0977cff2f;
        public float ParamPin_ed71c0e6_3c85_4f43_a89b_d42165339bdb;
        public System.Type ParamPin_bd1bcade_7d5c_4e96_89ac_bd5f5bf685fa;
        public samplers.mergeinstance.mergeinstance returnLink_bdce1cb1_5095_4a62_9974_668f5a5969b9;
        public EngineNS.GamePlay.Actor.GActor ValueOutHandle_808b0313_9d0b_4571_b361_ae52f33fef43;
        public System.Type ParamPin_4e9d4834_f0b3_4f78_afcf_dcbc3b1c7a69;
        public EngineNS.GamePlay.Controller.GPlayerController returnLink_7aa900d7_952d_488d_aace_f419b7cf8651;
        public float ValueInHandle_d33ed683_5df4_4065_9b69_40da5776ee80;
        public float ValueInHandle_3fe9dae4_e58e_4318_b740_b868535ca268;
        public float Value1_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d;
        public int Value2_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d;
        public float ResultLink_49eb4c6c_4781_47bb_ba0d_ee1d2ce0fe8d;
        public EngineNS.UISystem.UIElement ParamPin_4bc4890c_767e_4bc7_8123_94907b92e5e7;
        public EngineNS.UISystem.RoutedEventArgs ParamPin_4b1916cc_102c_4a2a_ad6c_2c41317862f0;
        public System.Type ParamPin_aaf2f186_30b6_4a09_a6a1_1c0938d17227;
        public samplers.mergeinstance.mergeinstance returnLink_2631be98_0a9c_4428_be1c_c7f5bbd255d0;
        public EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager ValueOutHandle_69215858_2694_40eb_b0cb_c65b75881c16;
        public bool ValueInHandle_7e2f3ca2_bdfe_410c_a9ea_32a46c8b529e;
        public System.Collections.Generic.List<EngineNS.GamePlay.Actor.GActor> ValueOutHandle_c171b86a_3d3f_41cf_9ff4_840ef8aa1c5b;
        public bool ValueInHandle_030fd285_4456_4fb7_818f_87a4ccdf5712;
        public EngineNS.UISystem.UIElement ParamPin_dc3bbdae_3117_4751_a6c4_792ad0e28965;
        public EngineNS.UISystem.RoutedEventArgs ParamPin_30bd72eb_7768_47d9_9690_cc15debf4db9;
        public System.Type ParamPin_0b835b57_ef1c_4285_bdc3_670bf938a596;
        public samplers.mergeinstance.mergeinstance returnLink_b8640db0_ff64_423d_a6db_2c44bbfa2f20;
        public EngineNS.Bricks.GpuDriven.GpuScene.SceneDataManager ValueOutHandle_9d276343_d9be_41e5_94cc_f536518353bf;
        public bool ValueInHandle_40ef42f2_069b_4d8c_9596_09bbcc2859d1;
        public System.Collections.Generic.List<EngineNS.GamePlay.Actor.GActor> ValueOutHandle_033153b7_77d3_4c3a_b647_e923a78a97b8;
        public bool ValueInHandle_9c2a64d1_6bc0_45b2_b8ae_3c048f83a5a3;
    }
    [EngineNS.Rtti.MetaClassAttribute()]
    public partial class ui_main_Initializer : EngineNS.UISystem.Controls.UserControlInitializer
    {
        public ui_main_Initializer()
        {
            this.WidthAuto = true;
            this.HeightAuto = true;
            this.Id = 1069865170;
            this.DesignRect = new EngineNS.RectangleF(0F, 0F, 1920F, 1080F);
            this.DesignClipRect = new EngineNS.RectangleF(0F, 0F, 1920F, 1080F);
        }
    }
}
