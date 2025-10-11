using EngineNS.DistanceField;
using EngineNS.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Font
{
    [Rtti.Meta("")]
    public class TtFontSDFAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtFontSDF.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "FONTSDF";
        }
        public override async Thread.Async.TtTask<IO.IAsset> LoadAsset()
        {
            //return await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(GetAssetName());
            return null;
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            TtEngine.Instance.EditorInstance.FontIcon?.OnDraw(cmdlist, in start, in end, 0);
            //cmdlist.AddText(in start, 0xFFFFFFFF, "PhyMtl", null);
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.FontSDFBoderColor;
        }
    }
    [Rtti.Meta("")]
    [TtFontSDF.Import]
    [IO.AssetCreateMenu(MenuName = "UI/FontSDF")]
    public class TtFontSDF : AuxPtrType<Canvas.FTFont>, IO.IAsset
    {
        public const string AssetExt = ".fontsdf";
        public string TypeExt { get => AssetExt; }
        public class TtFontDesc : IO.BaseSerializer
        {
            public TtFontDesc()
            {
                
            }
            [Rtti.Meta("")]
            public int OriginPixelSize { get; set; } = 2048;
            [Rtti.Meta("")]
            public int FontSize { get; set; } = 64;
            [Rtti.Meta("")]
            public byte Spread { get; set; } = 5;
            [Rtti.Meta("")]
            public byte PixelColored { get; set; } = 127;
            public TtFontCharFilter CharFilters { get; set; } = new TtFontCharFilter();
        }
        public class ImportAttribute : IO.IAssetCreateAttribute
        {
            bool bPopOpen = false;
            bool bFileExisting = false;
            RName mDir;
            string mName;
            string mSourceFile;
            public TtFontDesc mDesc = new TtFontDesc();
            ImGui.ImGuiFileDialog mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                mDir = dir;
                var noused = PGAsset.Initialize();
                PGAsset.Target = mDesc;
            }
            public override unsafe bool OnDraw(EGui.Controls.TtContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import font", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                bool retValue = false;
                var visible = true;
                ImGuiAPI.SetNextWindowSize(new Vector2(200, 500), ImGuiCond_.ImGuiCond_FirstUseEver);
                if (ImGuiAPI.BeginPopupModal($"Import font", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if (string.IsNullOrEmpty(ContentBrowser.CurrentImporterFile))
                    {
                        var sz = new Vector2(-1, 0);
                        if (ImGuiAPI.Button("Select Font", in sz))
                        {
                            mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".ttf", ".");
                        }
                        // display
                        if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                        {
                            // action if OK
                            if (mFileDialog.IsOk() == true)
                            {
                                mSourceFile = mFileDialog.GetFilePathName();
                                mName = IO.TtFileManager.GetPureName(mSourceFile);
                            }
                            // close
                            mFileDialog.CloseDialog();
                        }
                    }
                    else if (string.IsNullOrEmpty(mSourceFile))
                    {
                        mSourceFile = ContentBrowser.CurrentImporterFile;
                        mName = IO.TtFileManager.GetPureName(mSourceFile);
                    }
                    if (bFileExisting)
                    {
                        var clr = new Vector4(1, 0, 0, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    else
                    {
                        var clr = new Vector4(1, 1, 1, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    ImGuiAPI.Separator();

                    using (var buffer = BigStackBuffer.CreateInstance(64))
                    {
                        buffer.SetTextUtf8(mName);
                        ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        var name = buffer.AsTextUtf8();
                        if (mName != name)
                        {
                            mName = name;
                            bFileExisting = IO.TtFileManager.FileExists(mDir.Address + mName + NxRHI.TtSrView.AssetExt);
                        }
                    }

                    var btSz = Vector2.Zero;
                    if (bFileExisting == false)
                    {
                        if (ImGuiAPI.Button("Create Asset", in btSz))
                        {
                            if (ImportFont())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                retValue = true;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in btSz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }

                    ImGuiAPI.Separator();

                    PGAsset.OnDraw(false, false, false);

                    ImGuiAPI.EndPopup();
                }
                if (!visible)
                    retValue = true;
                return retValue;
            }
            private bool ImportFont()
            {
                var rn = RName.GetRName(mDir.Name + mName + TtFontSDF.AssetExt, mDir.RNameType);

                mDesc.CharFilters.Excludes.Add(new Vector2ui(0, 65535));
                TtFontSDF.SaveFont(rn, TtEngine.Instance.FontModule.FontManager, mSourceFile, mDesc);

                var ameta = new TtFontSDFAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtFontSDFAMeta));
                ameta.Description = $"This is a {typeof(TtFontSDFAMeta).FullName}\n";
                ameta.SaveAMeta((IO.IAsset)null);

                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                return true;
            }
        }

        #region IAsset
        public override void Dispose()
        {
            base.Dispose();
        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtFontSDFAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);
        }
        [Rtti.Meta("")]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        TtFontManager mFontManager;
        public TtFontSDF()
        {
            mCoreObject = Canvas.FTFont.CreateInstance();
        }
        public TtFontSDF(TtFontManager mgr, Canvas.FTFont self)
        {
            mFontManager = mgr;
            mCoreObject = self;
        }
        public string Name
        {
            get { return mCoreObject.GetName(); }
        }
        public int FontSize
        {
            get { return mCoreObject.GetFontSize(); }
        }
        public bool IsNeedSave
        {
            get
            {
                return mCoreObject.IsNeedSave();
            }
        }
        public class TtFontCharFilter : IO.BaseSerializer
        {
            public TtFontCharFilter()
            {
                IncludeChars = "abc中国1A，!,";
            }
            [Rtti.Meta("")]
            public uint CharBegin { get; set; } = 0;
            [Rtti.Meta("")]
            public uint CharEnd { get; set; } = ushort.MaxValue;
            string mIncludeChars;
            [Rtti.Meta("")]
            public string IncludeChars 
            { 
                get
                {
                    return mIncludeChars;
                }
                set
                {
                    mIncludeChars = value;

                    unsafe
                    {
                        IncludeUnicodes.Clear();
                        var buffer = Encoding.UTF32.GetBytes(value);
                        if (buffer.Length == 0)
                            return;
                        int num = buffer.Length / 4;
                        fixed (byte* p = &buffer[0])
                        {
                            var pUnicodes = (uint*)p;
                            for (int i = 0; i < num; i++)
                            {
                                IncludeUnicodes.Add(pUnicodes[i]);
                            }
                        }
                    }
                }
            }
            public List<uint> IncludeUnicodes = new List<uint>();
            [Rtti.Meta("")]
            public List<Vector2ui> Includes { get; set; } = new List<Vector2ui>();
            [Rtti.Meta("")]
            public List<Vector2ui> Excludes { get; set; } = new List<Vector2ui>();
            public bool IsInclude(uint c)
            {
                if (IncludeUnicodes.Contains(c))
                    return true;
                foreach (var i in Includes)
                {
                    if (c >= i.X && c <= i.Y)
                        return true;
                }
                return false;
            }
            public bool IsExclude(uint c)
            {
                foreach (var i in Excludes)
                {
                    if (c >= i.X && c <= i.Y)
                        return true;
                }
                return false;
            }
        }
        public static unsafe void SaveFont(RName name, TtFontManager manager, string fontName, TtFontDesc desc)
        {
            fontName = IO.TtFileManager.GetRegularPath(fontName).ToLower();
            TtFontCharFilter filter = desc.CharFilters;
            var font = new TtFontSDF();
            font.mCoreObject.InitForBuildFont(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject,
                manager.mCoreObject, fontName, desc.FontSize,
                desc.OriginPixelSize, desc.Spread, desc.PixelColored);

            //内存扛不住。。。
            //var num = filter.CharEnd - filter.CharBegin;
            //var smp = TtEngine.Instance.EventPoster.ParrallelFor((int)num, (index) =>
            //{
            //    uint unicode = (uint)index + filter.CharBegin;

            //    if (font.mCoreObject.GetCharIndex(unicode) == 0)
            //        return;
            //    if (filter.IsInclude(unicode) == false && filter.IsExclude(unicode))
            //        return;
            //    font.mCoreObject.AddWordForBuild(unicode);
            //});
            //smp?.Wait(int.MaxValue);

            Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, $"Begin SFFont = {filter.CharBegin} : {filter.CharEnd}");
            for (uint unicode = filter.CharBegin; unicode < filter.CharEnd; unicode++)
            {
                if (font.mCoreObject.GetCharIndex(unicode) == 0)
                    continue;
                if (filter.IsInclude(unicode) == false && filter.IsExclude(unicode))
                    continue;
                font.mCoreObject.AddWordForBuild(unicode);
                System.Diagnostics.Debug.WriteLine($"AddWord {unicode}");
            }
            Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, "End SFFont");
            using (var xnd = new IO.TtXndHolder("FontSDF", 0, 0))
            {
                font.mCoreObject.SaveFontSDF(xnd.RootNode.mCoreObject);

                xnd.SaveXnd(name.Address);
            }
        }

        public unsafe uint[] GetTotalWords()
        {
            var num = mCoreObject.GetWordNum();
            uint[] words = new uint[num];
            fixed (uint* p = &words[0])
            {
                mCoreObject.GetTotalWords(p, num);
            }
            return words;
        }
        public Canvas.FTWord GetWord(uint uniCode)
        {
            return mCoreObject.GetWord(uniCode);
        }
        public void SaveFontAsset()
        {
            using (var xnd = new IO.TtXndHolder("FontSDF", 0, 0))
            {
                mCoreObject.SaveFontSDF(xnd.RootNode.mCoreObject);

                xnd.SaveXnd(AssetName.Address);
            }
        }
        public unsafe uint GetWords(EngineNS.Canvas.FTWord** pWords, uint count, wchar_t* text, uint numOfChar)
        {
            return mCoreObject.GetWords(pWords, count, text, numOfChar);
        }
        public unsafe uint GetWords(in TtNativeArray<EngineNS.Canvas.FTWord> words, string text)
        {
            var count = text.Length;
            words.SetSize(count);
            return GetWords((EngineNS.Canvas.FTWord**)words.UnsafeGetElementAddress(0), (uint)count, text);
        }
        public unsafe uint GetWords(EngineNS.Canvas.FTWord** pWords, uint count, string text)
        {
#if PWindow
            fixed(char* p = text)
            {
                return mCoreObject.GetWords(pWords, count, (wchar_t*)p, (uint)text.Length);
            }
#else
            fixed (char* p = text)
            {
                using (var buffer = BigStackBuffer.CreateInstance(text.Length * sizeof(wchar_t)))
                {
                    var numOfUtf32 = System.Text.Encoding.UTF32.GetBytes(p, text.Length, (byte*)buffer.GetBuffer(), text.Length * sizeof(wchar_t));
                    return mCoreObject.GetWords(pWords, count, (wchar_t*)buffer.GetBuffer(), (uint)numOfUtf32);
                }
            }
#endif
        }
        public unsafe Vector2 GetTextSize(string text)
        {
#if PWindow
            fixed(char* p = text)
            {
                return mCoreObject.GetTextSize((wchar_t*)p);
            }
#else
            fixed(char* p = text)
            {
                using (var buffer = BigStackBuffer.CreateInstance(text.Length * sizeof(wchar_t)))
                {
                    var numOfUtf32 = System.Text.Encoding.UTF32.GetBytes(p, text.Length, (byte*)buffer.GetBuffer(), text.Length * sizeof(wchar_t));
                    return mCoreObject.GetTextSize((wchar_t*)buffer.GetBuffer());
                }
            }
#endif
        }

        public bool LoadFtFaceFromFile(string font)
        {
            return mCoreObject.LoadFtFaceFromFile(mFontManager.mCoreObject, font);
        }
        public bool LoadFtFaceFromBlob(Support.TtBlobObject blob)
        {
            return mCoreObject.LoadFtFaceFromBlob(mFontManager.mCoreObject, blob.mCoreObject);
        }
        
    }

    public class TtFontManager : AuxPtrType<Canvas.FTFontManager>
    {
        public const string FontSDFAssetExt = ".fontsdf";
        public const string FontAssetExt = ".font";

        public struct FFontKey : IEquatable<FFontKey>
        {
            public RName Name;
            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
            public bool Equals(FFontKey other)
            {
                return (Name == other.Name);
            }
        }
        public Dictionary<FFontKey, TtFontSDF> CachedFonts = new Dictionary<FFontKey, TtFontSDF>();
        public TtFontManager()
        {
            mCoreObject = Canvas.FTFontManager.CreateInstance();
            mCoreObject.Init();
        }
        public override void Dispose()
        {
            foreach (var i in CachedFonts)
            {
                i.Value.Dispose();
            }
            CachedFonts.Clear();
            base.Dispose();
        }
        public TtFontSDF GetFontSDF(RName font, int texSizeX = -1, int texSizeY = -1)
        {
            FFontKey key;
            key.Name = font;
            TtFontSDF result;
            if (CachedFonts.TryGetValue(key, out result))
            {
                return result;
            }
            if (texSizeX < 0)
                texSizeX = 512;
            if (texSizeY < 0)
                texSizeY = 512;
            var xnd = IO.TtXndHolder.LoadXnd(font.Address);
            result = new TtFontSDF(this, mCoreObject.CreateFontSDF(font.ToString(),TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, xnd.mCoreObject, texSizeX, texSizeY));
            result.AssetName = font;
            result.LoadFtFaceFromFile(IO.TtFileManager.GetBaseDirectory(font.Address) + result.mCoreObject.GetSourceFont());
            CachedFonts.Add(key, result);
            return result;
        }
        public void Tick(TtEngine host)
        {
            foreach(var i in CachedFonts)
            {
                if (TtEngine.Instance.PlayMode != EPlayMode.Game)
                {
                    if (i.Value.IsNeedSave)
                    {
                        i.Value.SaveFontAsset();
                    }
                }
                i.Value.mCoreObject.Update(host.GfxDevice.RenderContext.mCoreObject, false);
            }
        }
    }

    [Rtti.Meta("")]
    [Bricks.CodeBuilder.ShaderNode.Control.TtMaterialShader]
    public partial class TtFontHLSLMethod
    {
        [Rtti.Meta("")]
        [Bricks.CodeBuilder.ShaderNode.Control.TtMaterialShader(Name = "GetFontSDF", Include = "@Engine/Shaders/Bricks/TextFont/FontSDF.cginc")]
        [Bricks.CodeBuilder.ContextMenu("font", "Bricks\\Font\\GetFontSDF", Bricks.CodeBuilder.ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 GetFontSDF(int effect, Vector3 baseColor, Vector3 borderColor, float alpha, float lowThreshold = 0, float highThreshold = 0.8f, float smoothValue = 0.5f)
        {
            return Vector4.Zero;
        }
    }

    public class TtFontModule : TtModule<TtEngine>
    {
        TtFontManager mFontManager = new TtFontManager();
        public TtFontManager FontManager { get => mFontManager; }
        public override Task<bool> PostInitialize(TtEngine host)
        {
            //var font = FontManager.GetFontSDF(RName.GetRName("fonts/roboto-regular.fontsdf", RName.ERNameType.Engine), 0, 1024, 512);
            //using (var words = UNativeArray<EngineNS.FTWord>.CreateInstance())
            //{
            //    font.GetWords(words, "abc中国1A，!,");
            //}
            return base.PostInitialize(host);
        }
        public override void TickModule(TtEngine host)
        {
            FontManager.Tick(host);
            base.TickModule(host);
        }
        public override void Cleanup(TtEngine host)
        {
            CoreSDK.DisposeObject(ref mFontManager);
            base.Cleanup(host);
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public Bricks.Font.TtFontModule FontModule { get; } = new Bricks.Font.TtFontModule();
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.Font
{
	partial class TtFontHLSLMethod
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetFontSDF_1544034440 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Font.TtFontHLSLMethod->static Vector4 GetFontSDF(int effect, Vector3 baseColor, Vector3 borderColor, float alpha, float lowThreshold, float highThreshold, float smoothValue)");
		public static unsafe Vector4 macross_GetFontSDF (string nodeName, int effect, Vector3 baseColor, Vector3 borderColor, float alpha, float lowThreshold, float highThreshold, float smoothValue) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":effect", effect);
					stackframe.SetWatchVariable(nodeName + ":baseColor", baseColor);
					stackframe.SetWatchVariable(nodeName + ":borderColor", borderColor);
					stackframe.SetWatchVariable(nodeName + ":alpha", alpha);
					stackframe.SetWatchVariable(nodeName + ":lowThreshold", lowThreshold);
					stackframe.SetWatchVariable(nodeName + ":highThreshold", highThreshold);
					stackframe.SetWatchVariable(nodeName + ":smoothValue", smoothValue);
				}
			}
			var _return_value = GetFontSDF(effect, baseColor, borderColor, alpha, lowThreshold, highThreshold, smoothValue);
			macross_break_GetFontSDF_1544034440.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross