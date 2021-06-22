using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Editor
{
    public struct ValueType
    {
        public bool BoolValue { get; set; }
    }
    public class PropertyGridTestClass
    {
        public PropertyGridTestClass NewTest { get; set; }
        public ValueType ValueType { get; set; }
        public bool BoolValue { get; set; }
        public SByte SByteValue { get; set; }
        public UInt16 UInt16Value { get; set; }
        public UInt32 UInt32Value { get; set; }
        public UInt64 UInt64Value { get; set; }
        public Byte ByteValue { get; set; }
        public Int16 Int16Value { get; set; }
        public Int32 Int32Value { get; set; }
        public Int64 Int64Value { get; set; }
        public float FloatValue { get; set; }
        public double DoubleValue { get; set; }
        public string StringValue { get; set; }
        public enum NormalEnum
        {
            EnumV0,
            EnumV1,
            EnumV2,
            EnumV3,
            EnumV4,
        }
        public NormalEnum NEnumValue { get; set; }
        [Flags]
        public enum FlagEnum
        {
            FEV0 = 1,
            FEV1 = 1<<1,
            FEV2 = 1<<2,
            FEV3 = 1<<3,
            FEV4 = 1<<4,
        }
        public FlagEnum FlagEnumValue { get; set; }
        [Category("Collections")]
        public int[] ArrayValue = new int[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        [Category("Collections")]
        public List<int> ListValue = new List<int>()
        {
            0,1,2,3,4,5
        };
        [Category("Collections")]
        public Dictionary<int, string> DicValue = new Dictionary<int, string>();
        public Vector2 Vector2Value { get; set; } = new Vector2();
        public Vector3 Vector3Value { get; set; } = new Vector3();
        public Vector4 Vector4Value { get; set; } = new Vector4();
        [EngineNS.EGui.Controls.PropertyGrid.Color4PickerEditorAttribute()]
        public Vector4 Color4Value { get; set; } = new Vector4(1, 1, 1, 1);
        [EngineNS.EGui.Controls.PropertyGrid.Color3PickerEditorAttribute()]
        public Vector3 Color3Value { get; set; } = new Vector3(1, 1, 1);
        [EGui.Controls.PropertyGrid.UByte4ToColor4PickerEditor]
        public UInt32 Color { get; set; } = 0xFFFFFFFF;

        public PropertyGridTestClass()
        {
            //for(int i=0; i<5; i++)
            //    DicValue[i] = "A" + i;
        }
    }

    public class UIWindowsTest
    {
        public unsafe void Initialized()
        {
            //var io = ImGuiAPI.GetIO();
            //var size_pixels = 18.0f;
            //ImFontConfig fontConfig = new ImFontConfig();
            ////fontConfig.MergeMode = true;
            ////var font = io.Fonts.AddFontDefault(&fontConfig);
            //mDefaultFont = io.Fonts.AddFontFromFileTTF(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Medium.ttf", size_pixels, &fontConfig, io.Fonts.GetGlyphRangesDefault());
            //////io.Fonts.AddFontFromFileTTF(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Bold.ttf", size_pixels, &fontConfig, io.Fonts.GetGlyphRangesDefault());
            //////io.Fonts.AddFontFromFileTTF(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Regular.ttf", size_pixels, &fontConfig, io.Fonts.GetGlyphRangesChineseSimplifiedCommon());
            //////io.Fonts.AddFontFromFileTTF(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Regular.ttf", size_pixels, &fontConfig, io.Fonts.GetGlyphRangesDefault());
            //io.Fonts.Build();

            var style = ImGuiAPI.GetStyle();
            style->Colors[(int)ImGuiCol_.ImGuiCol_Text] = new Vector4(0.75f, 0.75f, 0.75f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_WindowBg] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TitleBgActive] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TitleBg] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Header] = new Vector4(0.15f, 0.73f, 1.00f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_HeaderActive] = new Vector4(0.15f, 0.73f, 1.00f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_HeaderHovered] = new Vector4(0.15f, 0.73f, 1.00f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Tab] = new Vector4(0.08f, 0.08f, 0.08f, 0.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabHovered] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabActive] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabUnfocused] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabUnfocusedActive] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_PopupBg] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_MenuBarBg] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_DockingEmptyBg] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Separator] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_SeparatorHovered] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_SeparatorActive] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_FrameBg] = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_FrameBgHovered] = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_FrameBgActive] = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Header] = new Vector4(0.13f, 0.13f, 0.13f, 1.00f);
            //style->WindowPadding = new Vector2(1, 0);
            style->ChildBorderSize = 0.0f;
            style->ScrollbarSize = 14.0f;

            mMenuItems = new List<EGui.UIProxy.MenuItemProxy>()
            {
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = "File",
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.NamedMenuSeparator()
                        {
                            Name = "OPEN",
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "New Level...",
                            Shortcut = "Ctrl+N",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(140.0f/1024, 265.0f/1024),
                                UVMax = new Vector2(156.0f/1024, 281.0f/1024),
                            },
                            Action = ()=>
                            {
                                var files = IO.FileManager.GetFiles(@"I:\titan3d\icons", "*.*");
                                var tagDir = RName.GetRName("icons", RName.ERNameType.Engine);
                                foreach(var file in files)
                                {
                                    RHI.CShaderResourceView.ImportAttribute.ImportImage(file, tagDir);
                                }
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Open Level..",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(140.0f/1024, 281.0f/1024),
                                UVMax = new Vector2(156.0f/1024, 297.0f/1024),
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Open Assets...",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(156.0f/1024, 265.0f/1024),
                                UVMax = new Vector2(172.0f/1024, 281.0f/1024),
                            },
                        },
                        new EGui.UIProxy.NamedMenuSeparator()
                        {
                            Name = "SAVE",
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Save Current Level",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(156.0f/1024, 281.0f/1024),
                                UVMax = new Vector2(172.0f/1024, 297.0f/1024),
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Save Current Level As...",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(124.0f/1024, 305.0f/1024),
                                UVMax = new Vector2(140.0f/1024, 321.0f/1024),
                            },
                        },
                    },
                },
                new EGui.UIProxy.MenuItemProxy()
                {
                    MenuName = "Edit",
                    IsTopMenuItem = true,
                    SubMenus = new List<EGui.UIProxy.IUIProxyBase>()
                    {
                        new EGui.UIProxy.NamedMenuSeparator()
                        {
                            Name = "HISTORY",
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Undo",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(126.0f/1024, 385.0f/1024),
                                UVMax = new Vector2(142.0f/1024, 401.0f/1024),
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Redo",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(142.0f/1024, 353.0f/1024),
                                UVMax = new Vector2(158.0f/1024, 369.0f/1024),
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Undo History",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(158.0f/1024, 353.0f/1024),
                                UVMax = new Vector2(174.0f/1024, 369.0f/1024),
                            },
                        },
                        new EGui.UIProxy.NamedMenuSeparator()
                        {
                            Name = "EDIT",
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Cut",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(142.0f/1024, 369.0f/1024),
                                UVMax = new Vector2(158.0f/1024, 385.0f/1024),
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Copy",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(142.0f/1024, 385.0f/1024),
                                UVMax = new Vector2(158.0f/1024, 401.0f/1024),
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Paste",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(158.0f/1024, 369.0f/1024),
                                UVMax = new Vector2(174.0f/1024, 385.0f/1024),
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Duplicate",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(162.0f/1024, 428.0f/1024),
                                UVMax = new Vector2(178.0f/1024, 444.0f/1024),
                            },
                        },
                        new EGui.UIProxy.MenuItemProxy()
                        {
                            MenuName = "Delete",
                            Icon = new EGui.UIProxy.ImageProxy()
                            {
                                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                                ImageSize = new Vector2(16, 16),
                                UVMin = new Vector2(560.0f/1024, 21.0f/1024),
                                UVMax = new Vector2(576.0f/1024, 37.0f/1024),
                            },
                        },

                    },
                },
            };

            var meshEditor = new MeshEditor()
            {
                DockCond = ImGuiCond_.ImGuiCond_FirstUseEver,
            };
            meshEditor.Initialize();
            mForms = new List<IRootForm>()
            {
                meshEditor,
            };
        }

        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        List<IRootForm> mForms = new List<IRootForm>();
        //bool fileMenuOpen = false;
        //bool colorPushed = false;
        //bool hover = false;

        UInt32 mMainDockId;

        public unsafe void OnDraw()
        {
            //fixed(ImFont* font = &mDefaultFont)
            //    ImGuiAPI.PushFont(font);

            mMainDockId = ImGuiAPI.GetID("MainDocker");
            var mainPos = new Vector2(0);
            ImGuiAPI.SetNextWindowPos(ref mainPos, ImGuiCond_.ImGuiCond_FirstUseEver, ref mainPos);
            var wSize = new Vector2(1290, 800);
            ImGuiAPI.SetNextWindowSize(ref wSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            bool visible = true;
            if (ImGuiAPI.Begin("UI Test", ref visible, 
                ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
            {

                // Menu
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.TopMenuFramePadding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, ref EGui.UIProxy.StyleConfig.Instance.TopMenuItemSpacing);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_PopupBg, EGui.UIProxy.StyleConfig.Instance.MenuBG);
                if (ImGuiAPI.BeginMenuBar())
                {
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    for (int i=0; i<mMenuItems.Count; i++)
                    {
                        mMenuItems[i].OnDraw(ref drawList);
                    }
                    ImGuiAPI.EndMenuBar();
                }
                //var menubarMin = ImGuiAPI.GetItemRectMin();
                //var menubarMax = ImGuiAPI.GetItemRectMax();
                //var menubarSize = ImGuiAPI.GetItemRectSize();
                ImGuiAPI.PopStyleVar(2);
                ImGuiAPI.PopStyleColor(1);

                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, ref EGui.UIProxy.StyleConfig.Instance.WindowPadding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.MainTabFramePadding);
                //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, ref EGui.UIProxy.StyleConfig.Instance.TopMenuWindowPadding);

                var size = new Vector2(0, 0);
                ImGuiWindowClass dockClass = new ImGuiWindowClass()
                {
                    //TabItemFlagsOverrideSet = ImGuiTabItemFlags_.ImGuiTabItemFlags_Leading
                };
                ImGuiAPI.DockSpace(mMainDockId, ref size, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, ref dockClass);
                for (int i = 0; i < mForms.Count; ++i)
                {
                    if (mForms[i].DockId == uint.MaxValue)
                        mForms[i].DockId = mMainDockId;
                    ImGuiAPI.SetNextWindowDockID(mForms[i].DockId, mForms[i].DockCond);
                    mForms[i].OnDraw();
                }

                //bool open = true;
                //ImGuiAPI.SetNextWindowDockID(mMainDockId, ImGuiCond_.ImGuiCond_FirstUseEver);
                //if (ImGuiAPI.Begin("Material Editor", ref open, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                //{
                //}
                //ImGuiAPI.End();

                ImGuiAPI.PopStyleVar(2);

                //var drawList = ImGuiAPI.GetWindowDrawList();
                //drawList.AddRect(ref menubarMin, ref menubarMax, 0xFF0000FF, 0, ImDrawFlags_.ImDrawFlags_None, 2);
            }
            ImGuiAPI.End();

            //ImGuiAPI.PopFont();
        }
    }

    public class MeshEditor : IRootForm
    {
        bool mVisible = true;
        public bool Visible
        {
            get => mVisible;
            set => mVisible = value;
        }
        public uint DockId
        {
            get;
            set;
        } = uint.MaxValue;
        public ImGuiCond_ DockCond
        {
            get;
            set;
        } = ImGuiCond_.ImGuiCond_FirstUseEver;

        EGui.UIProxy.Toolbar mToolbar;

        public unsafe void Initialize()
        {
            mToolbar = new EGui.UIProxy.Toolbar();
            mToolbar.AddToolbarItems(
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "Save",
                    Icon = new EGui.UIProxy.ImageProxy()
                    {
                        ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                        ImageSize = new Vector2(20, 20),
                        UVMin = new Vector2(3.0f / 1024, 4.0f / 1024),
                        UVMax = new Vector2(23.0f / 1024, 24.0f / 1024),
                    },
                    Action = ()=>
                    {

                    },
                },
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "Browse",
                    Icon = new EGui.UIProxy.ImageProxy()
                    {
                        ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                        ImageSize = new Vector2(20, 20),
                        UVMin = new Vector2(414.0f / 1024, 3.0f / 1024),
                        UVMax = new Vector2(434.0f / 1024, 23.0f / 1024),
                    },
                    Action = ()=>
                    {

                    },
                },
                new EGui.UIProxy.ToolbarSeparator(),
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "Reimport Base Mesh",
                    Icon = new EGui.UIProxy.ImageProxy()
                    {
                        ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                        ImageSize = new Vector2(20, 20),
                        UVMin = new Vector2(437.0f / 1024, 3.0f / 1024),
                        UVMax = new Vector2(457.0f / 1024, 23.0f / 1024),
                    },
                    Action = ()=>
                    {

                    },
                },
                new EGui.UIProxy.ToolbarSeparator(),
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "Collision",
                    Icon = new EGui.UIProxy.ImageProxy()
                    {
                        ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                        ImageSize = new Vector2(20, 20),
                        UVMin = new Vector2(459.0f / 1024, 4.0f / 1024),
                        UVMax = new Vector2(479.0f / 1024, 24.0f / 1024),
                    },
                },
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "UV",
                    Icon = new EGui.UIProxy.ImageProxy()
                    {
                        ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                        ImageSize = new Vector2(20, 20),
                        UVMin = new Vector2(481.0f / 1024, 4.0f / 1024),
                        UVMax = new Vector2(501.0f / 1024, 24.0f / 1024),
                    },
                }
            );

            var viewPort = ImGuiAPI.GetMainViewport();
            var inspector = new EGui.Controls.PropertyGrid.PropertyGrid();
            inspector.Initialize();
            inspector.SearchInfo = "Search Details";
            //inspector.Target = *viewPort;
            //inspector.Target = EGui.UIProxy.StyleConfig.Instance;
            inspector.Target = new PropertyGridTestClass();//EGui.UIProxy.StyleConfig.Instance;
            mPanels.Add(inspector);
            var contentBrowser = new ContentBrowser();
            contentBrowser.Initialize();
            mPanels.Add(contentBrowser);
            mPanels.Add(new ViewportPanel());
        }

        UInt32 mPanelDockId;
        List<EGui.IPanel> mPanels = new List<EGui.IPanel>();

        public unsafe void OnDraw()
        {
            mPanelDockId = ImGuiAPI.GetID("PanelDocker");
            if (ImGuiAPI.Begin("Mesh Editor", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {

                var drawList = ImGuiAPI.GetWindowDrawList();
                mToolbar.OnDraw(ref drawList);

                var winPosMin = ImGuiAPI.GetWindowContentRegionMin() + new Vector2(0, mToolbar.ToolbarHeight);
                var winPosMax = ImGuiAPI.GetWindowContentRegionMax();
                var contentSize = winPosMax - winPosMin;
                var winPos = ImGuiAPI.GetWindowPos();
                winPos.Y += ImGuiAPI.GetWindowSize().Y - contentSize.Y;
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(ref winPos, ImGuiCond_.ImGuiCond_Always, ref pivot);
                if(ImGuiAPI.BeginChild("MeshEDDockChild", ref contentSize, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, ref EGui.UIProxy.StyleConfig.Instance.WindowPadding);
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.MainTabFramePadding);

                    var size = new Vector2(0, 0);
                    ImGuiWindowClass centerDockClass = new ImGuiWindowClass();
                    ImGuiAPI.DockSpace(mPanelDockId, ref size, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, ref centerDockClass);

                    for(int i=0; i<mPanels.Count; ++i)
                    {
                        if (mPanels[i].DockId == uint.MaxValue)
                            mPanels[i].DockId = mPanelDockId;
                        ImGuiAPI.SetNextWindowDockID(mPanels[i].DockId, mPanels[i].DockCond);
                        mPanels[i].OnDraw();
                    }

                    ImGuiAPI.PopStyleVar(2);
                }
                ImGuiAPI.EndChild();
            }
            ImGuiAPI.End();
        }
    }

    public class ViewportPanel : EGui.IPanel
    {
        bool mVisible = true;
        public bool Visible { get=> mVisible; set=> mVisible = value; }
        public uint DockId { get; set; } = uint.MaxValue;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        EGui.UIProxy.ImageProxy image = new EGui.UIProxy.ImageProxy()
        {
            ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
            ImageSize = new Vector2(320, 320),
            UVMin = new Vector2(303.0f / 1024, 270.0f / 1024),
            UVMax = new Vector2((303.0f + 32) / 1024, (270.0f + 32) / 1024),
        };

        Vector2 uvMin = new Vector2(303.0f / 1024, 270.0f / 1024);
        Vector2 uvMax = new Vector2((303.0f + 32) / 1024, (270.0f + 32) / 1024);
        public void OnDraw()
        {
            if(ImGuiAPI.Begin("Viewport", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                image.UVMin = uvMin;
                image.UVMax = uvMax;
                image.OnDraw(ref drawList);
            }
            ImGuiAPI.End();
        }
    }

    public class ContentBrowser : EGui.IPanel
    {
        bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; } = uint.MaxValue;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        EGui.UIProxy.Toolbar mToolbar;

        public void Initialize()
        {
            mToolbar = new EGui.UIProxy.Toolbar();
            mToolbar.AddToolbarItems(
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "ADD",
                    Icon = new EGui.UIProxy.ImageProxy()
                    {
                        ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                        ImageSize = new Vector2(20, 20),
                        UVMin = new Vector2(299.0f / 1024, 4.0f / 1024),
                        UVMax = new Vector2(315.0f / 1024, 20.0f / 1024),
                    },
                    Action = () =>
                    {
                
                    },
                },
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "Import",
                    Icon = new EGui.UIProxy.ImageProxy()
                    {
                        ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                        ImageSize = new Vector2(20, 20),
                        UVMin = new Vector2(299.0f / 1024, 22.0f / 1024),
                        UVMax = new Vector2(315.0f / 1024, 38.0f / 1024),
                    },
                    Action = () =>
                    {
                
                    },
                },
                new EGui.UIProxy.ToolbarIconButtonProxy()
                {
                    Name = "Save All",
                    Icon = new EGui.UIProxy.ImageProxy()
                    {
                        ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                        ImageSize = new Vector2(20, 20),
                        UVMin = new Vector2(317.0f / 1024, 4.0f / 1024),
                        UVMax = new Vector2(333.0f / 1024, 20.0f / 1024),
                    },
                    Action = () =>
                    {
                
                    },
                }
            );
        }

        public void OnDraw()
        {
            if (ImGuiAPI.Begin("Content Browser", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                mToolbar.OnDraw(ref drawList);

                var winPosMin = ImGuiAPI.GetWindowContentRegionMin() + new Vector2(0, mToolbar.ToolbarHeight);
                var winPosMax = ImGuiAPI.GetWindowContentRegionMax();
                var contentSize = winPosMax - winPosMin;
                contentSize.Y -= 1;
                var winPos = ImGuiAPI.GetWindowPos();
                winPos.Y += ImGuiAPI.GetWindowSize().Y - contentSize.Y;
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(ref winPos, ImGuiCond_.ImGuiCond_Always, ref pivot);
                if (ImGuiAPI.BeginChild("Content Browser Child", ref contentSize, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var childDrawList = ImGuiAPI.GetWindowDrawList();
                    var posMin = ImGuiAPI.GetWindowPos();
                    var posMax = posMin + ImGuiAPI.GetWindowSize();
                    childDrawList.AddRectFilled(ref posMin, ref posMax, EGui.UIProxy.StyleConfig.Instance.PanelBackground, 1, ImDrawFlags_.ImDrawFlags_None);
                }
                ImGuiAPI.EndChild();
            }
            ImGuiAPI.End();
        }
    }

}
