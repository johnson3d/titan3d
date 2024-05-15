using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor
{
    public struct ValueType
    {
        public bool BoolValue { get; set; }
    }
    public class PropertyGridTestClass2
    {
        public UInt32 UInt32Value { get; set; } = 2;
        public bool BoolValue { get; set; } = true;
        public ValueType ValueType { get; set; }
    }
    public class PropertyGridTestClass : EGui.Controls.PropertyGrid.IPropertyCustomization
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
            FEV1 = 1 << 1,
            FEV2 = 1 << 2,
            FEV3 = 1 << 3,
            FEV4 = 1 << 4,
        }
        public FlagEnum FlagEnumValue { get; set; }
        [Category("Collections")]
        public int[] ArrayValue { get; set; } = new int[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        [Category("Collections")]
        public List<int> ListValue { get; set; } = new List<int>()
        {
            0,1,2,3,4,5
        };
        [Category("Collections")]
        public Dictionary<int, string> DicValue { get; set; } = new Dictionary<int, string>();
        public Vector2 Vector2Value { get; set; } = new Vector2();
        public Vector3 Vector3Value { get; set; } = new Vector3();
        public Vector4 Vector4Value { get; set; } = new Vector4();
        [EngineNS.EGui.Controls.PropertyGrid.Color4PickerEditorAttribute()]
        public Vector4 Color4Value { get; set; } = new Vector4(1, 1, 1, 1);
        [EngineNS.EGui.Controls.PropertyGrid.Color3PickerEditorAttribute()]
        public Vector3 Color3Value { get; set; } = new Vector3(1, 1, 1);
        [EGui.Controls.PropertyGrid.UByte4ToColor4PickerEditor]
        public UInt32 Color { get; set; } = 0xFFFFFFFF;
        public RName RNameValue { get; set; }

        public PropertyGridTestClass()
        {
            //for(int i=0; i<5; i++)
            //    DicValue[i] = "A" + i;
            DicValue[0] = "A";
            DicValue[1] = "B";
            DicValue[2] = "C";
            DicValue[3] = "D";
            DicValue[4] = "E";
            DicValue[5] = "F";

            RNameValue = RName.GetRName("UTest/ground.uminst");
        }

        [Category("Ext")]
        public bool ShowExtPro { get; set; } = true;
        [Category("Ext")]
        public string ExtStringPro { get; set; } = "this is ext property";
        [Category("Ext")]
        public string ExtCategoryChangeTest { get; set; } = "this origin category is Ext";

        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;
        public void GetProperties(ref EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            var pros = TypeDescriptor.GetProperties(this);
            var thisType = Rtti.UTypeDesc.TypeOf(this.GetType());
            //collection.InitValue(this,  pros, parentIsValueType);
            foreach(PropertyDescriptor prop in pros)
            {
                if(!ShowExtPro)
                {
                    if (prop.Name == "ExtStringPro")
                        continue;
                    if (prop.Name == "ExtCategoryChangeTest")
                        continue;
                }
                var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDesc.InitValue(this, thisType, prop, parentIsValueType);
                if(prop.Name == "ExtCategoryChangeTest")
                {
                    proDesc.Category = "Ext2";
                }
                if(!proDesc.IsBrowsable)
                {
                    proDesc.ReleaseObject();
                    continue;
                }
                collection.Add(proDesc);
            }
        }

        public object GetPropertyValue(string propertyName)
        {
            var proInfo = this.GetType().GetProperty(propertyName);
            if(proInfo != null)
                return proInfo.GetValue(this);
            var fieldInfo = this.GetType().GetField(propertyName);
            if(fieldInfo != null)
                return fieldInfo.GetValue(this);
            return null;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            var proInfo = this.GetType().GetProperty(propertyName);
            if (proInfo != null)
                proInfo.SetValue(this, value);
            var fieldInfo = this.GetType().GetField(propertyName);
            if (fieldInfo != null)
                fieldInfo.SetValue(this, value);
        }
    }

    public class UIWindowsTest : EngineNS.IRootForm
    {
        public void Dispose()
        {
            for(int i=0; i<mForms.Count; i++)
            {
                mForms[i].Dispose();
            }
        }

        public async Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();

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
                            Action = (item, data)=>
                            {
                                var files = IO.TtFileManager.GetFiles(@"I:\titan3d\icons", "*.*");
                                var tagDir = RName.GetRName("icons", RName.ERNameType.Engine);
                                foreach(var file in files)
                                {
                                    NxRHI.USrView.ImportAttribute.ImportImage(file, tagDir, new NxRHI.USrView.UPicDesc());
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
            await meshEditor.Initialize();
            mForms = new List<IRootForm>()
            {
                meshEditor,
            };

            return true;
        }

        List<EGui.UIProxy.MenuItemProxy> mMenuItems = new List<EGui.UIProxy.MenuItemProxy>();
        List<IRootForm> mForms = new List<IRootForm>();
        //bool fileMenuOpen = false;
        //bool colorPushed = false;
        //bool hover = false;

        UInt32 mMainDockId;

        public bool Visible { get; set; }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; }

        public unsafe void OnDraw()
        {
            //fixed(ImFont* font = &mDefaultFont)
            //    ImGuiAPI.PushFont(font);

            mMainDockId = ImGuiAPI.GetID("MainDocker");
            var mainPos = new Vector2(0);
            ImGuiAPI.SetNextWindowPos(in mainPos, ImGuiCond_.ImGuiCond_FirstUseEver, in mainPos);
            var wSize = new Vector2(1290, 800);
            ImGuiAPI.SetNextWindowSize(in wSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm("UI Test", this,
                ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar);
            if (result)
            {

                // Menu
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in EGui.UIProxy.StyleConfig.Instance.TopMenuFramePadding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in EGui.UIProxy.StyleConfig.Instance.TopMenuItemSpacing);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_PopupBg, EGui.UIProxy.StyleConfig.Instance.MenuBG);
                if (ImGuiAPI.BeginMenuBar())
                {
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    for (int i=0; i<mMenuItems.Count; i++)
                    {
                        mMenuItems[i].OnDraw(in drawList, in Support.UAnyPointer.Default);
                    }
                    ImGuiAPI.EndMenuBar();
                }
                //var menubarMin = ImGuiAPI.GetItemRectMin();
                //var menubarMax = ImGuiAPI.GetItemRectMax();
                //var menubarSize = ImGuiAPI.GetItemRectSize();
                ImGuiAPI.PopStyleVar(2);
                ImGuiAPI.PopStyleColor(1);

                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, in EGui.UIProxy.StyleConfig.Instance.WindowPadding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in EGui.UIProxy.StyleConfig.Instance.MainTabFramePadding);
                //ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, ref EGui.UIProxy.StyleConfig.Instance.TopMenuWindowPadding);

                var size = new Vector2(0, 0);
                ImGuiWindowClass dockClass = new ImGuiWindowClass()
                {
                    //TabItemFlagsOverrideSet = ImGuiTabItemFlags_.ImGuiTabItemFlags_Leading
                };
                ImGuiAPI.DockSpace(mMainDockId, in size, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, in dockClass);
                for (int i = 0; i < mForms.Count; ++i)
                {
                    //if (mForms[i].DockId == uint.MaxValue)
                    //    mForms[i].DockId = mMainDockId;
                    //ImGuiAPI.SetNextWindowDockID(mForms[i].DockId, mForms[i].DockCond);
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
            EGui.UIProxy.DockProxy.EndMainForm(result);

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
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond
        {
            get;
            set;
        } = ImGuiCond_.ImGuiCond_FirstUseEver;

        EGui.UIProxy.Toolbar mToolbar;

        public void Dispose()
        {
            for(int i=0; i<mPanels.Count; i++)
            {
                mPanels[i].Dispose();
            }
        }

        public async Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();

            unsafe
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
                        Action = () =>
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
                        Action = () =>
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
                        Action = () =>
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
            }
                //var viewPort = ImGuiAPI.GetMainViewport();
                var inspector = new EGui.Controls.PropertyGrid.PropertyGrid();
                inspector.PGName = "MeshEditor_PG";
                await inspector.Initialize();
                inspector.SearchInfo = "Search Details";
                //inspector.Target = *viewPort;
                //inspector.Target = EGui.UIProxy.StyleConfig.Instance;
                inspector.Target = new PropertyGridTestClass();//EGui.UIProxy.StyleConfig.Instance;
                /*inspector.Target = new object[] { new PropertyGridTestClass(),
                    new PropertyGridTestClass()
                    {
                        BoolValue = true,
                        SByteValue = 1,
                        UInt16Value = 1,
                        UInt32Value = 1,
                        UInt64Value = 1,
                        ByteValue = 1,
                        Int16Value = 1,
                        Int32Value = 1,
                        Int64Value = 1,
                        FloatValue = 1,
                        DoubleValue = 1,
                        StringValue = "abc",
                        NEnumValue = PropertyGridTestClass.NormalEnum.EnumV3,
                        FlagEnumValue = PropertyGridTestClass.FlagEnum.FEV2,
                        Vector2Value = new Vector2(1, 0),
                        Vector3Value = new Vector3(1, 0, 1),
                        Vector4Value = new Vector4(1, 0, 1, 0),
                        Color4Value = new Vector4(1, 0, 1, 0),
                        Color3Value = new Vector3(1, 0, 1),
                        Color = 0xFF00FF00,
                        ValueType = new ValueType()
                        {
                            BoolValue = true,
                        },
                    }
                };//EGui.UIProxy.StyleConfig.Instance;*/
                mPanels.Add(inspector);
                var contentBrowser = new ContentBrowser();
                await contentBrowser.Initialize();
                mPanels.Add(contentBrowser);
            var viewPort = new ViewportPanel();
            await viewPort.Initialize();
            mPanels.Add(viewPort);

            return true;
        }

        UInt32 mPanelDockId;
        List<EGui.IPanel> mPanels = new List<EGui.IPanel>();

        public unsafe void OnDraw()
        {
            mPanelDockId = ImGuiAPI.GetID("PanelDocker");
            if (ImGuiAPI.Begin("Mesh Editor", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {

                var drawList = ImGuiAPI.GetWindowDrawList();
                mToolbar.OnDraw(in drawList, in Support.UAnyPointer.Default);

                var winPosMin = ImGuiAPI.GetWindowContentRegionMin() + new Vector2(0, mToolbar.ToolbarHeight);
                var winPosMax = ImGuiAPI.GetWindowContentRegionMax();
                var contentSize = winPosMax - winPosMin;
                var winPos = ImGuiAPI.GetWindowPos();
                winPos.Y += ImGuiAPI.GetWindowSize().Y - contentSize.Y;
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(in winPos, ImGuiCond_.ImGuiCond_Always, in pivot);
                if(ImGuiAPI.BeginChild("MeshEDDockChild", in contentSize, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, in EGui.UIProxy.StyleConfig.Instance.WindowPadding);
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in EGui.UIProxy.StyleConfig.Instance.MainTabFramePadding);

                    var size = new Vector2(0, 0);
                    ImGuiWindowClass centerDockClass = new ImGuiWindowClass();
                    ImGuiAPI.DockSpace(mPanelDockId, in size, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, in centerDockClass);

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

        //EGui.UIProxy.ImageProxy image = new EGui.UIProxy.ImageProxy()
        //{
        //    ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
        //    ImageSize = new Vector2(320, 320),
        //    UVMin = new Vector2(303.0f / 1024, 270.0f / 1024),
        //    UVMax = new Vector2((303.0f + 32) / 1024, (270.0f + 32) / 1024),
        //};

        public void Dispose() { }
        public async Task<bool> Initialize() 
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();

            mBezierControl.Initialize(10.0f, 10.0f, 30.0f, 20.0f);

            return true; 
        }

        Vector2 uvMin = new Vector2(303.0f / 1024, 270.0f / 1024);
        Vector2 uvMax = new Vector2((303.0f + 32) / 1024, (270.0f + 32) / 1024);

        EGui.Controls.BezierControl mBezierControl = new EGui.Controls.BezierControl();

        public void OnDraw()
        {
            if(ImGuiAPI.Begin("Viewport", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                mBezierControl.OnDraw();
            //    var drawList = ImGuiAPI.GetWindowDrawList();
            //    image.UVMin = uvMin;
            //    image.UVMax = uvMax;
            //    image.OnDraw(ref drawList);
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

        public void Dispose()
        {
            mToolbar.Cleanup();
        }

        public async Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();

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

            return true;
        }

        public void OnDraw()
        {
            if (ImGuiAPI.Begin("Content Browser", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                mToolbar.OnDraw(in drawList, in Support.UAnyPointer.Default);

                var winPosMin = ImGuiAPI.GetWindowContentRegionMin() + new Vector2(0, mToolbar.ToolbarHeight);
                var winPosMax = ImGuiAPI.GetWindowContentRegionMax();
                var contentSize = winPosMax - winPosMin;
                contentSize.Y -= 1;
                var winPos = ImGuiAPI.GetWindowPos();
                winPos.Y += ImGuiAPI.GetWindowSize().Y - contentSize.Y;
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(in winPos, ImGuiCond_.ImGuiCond_Always, in pivot);
                if (ImGuiAPI.BeginChild("Content Browser Child", in contentSize, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var childDrawList = ImGuiAPI.GetWindowDrawList();
                    var posMin = ImGuiAPI.GetWindowPos();
                    var posMax = posMin + ImGuiAPI.GetWindowSize();
                    childDrawList.AddRectFilled(in posMin, in posMax, EGui.UIProxy.StyleConfig.Instance.PanelBackground, 1, ImDrawFlags_.ImDrawFlags_None);
                }
                ImGuiAPI.EndChild();
            }
            ImGuiAPI.End();
        }
    }

}