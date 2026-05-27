using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EngineNS.EGui
{
    public enum ECodeEditorViewStyle
    {
        Dark,
        Light,
        Blue,
    }

    public class TtCodeEditor : AuxPtrType<FCodeEditor>
    {
        public TtCodeEditor()
        {
            mCoreObject = FCodeEditor.CreateInstance();
            SetViewStyle(ECodeEditorViewStyle.Dark);
        }

        public unsafe string Text
        {
            get
            {
                var blob = new Support.TtBlobObject();
                mCoreObject.GetText(blob.mCoreObject);
                var data = (IntPtr)blob.mCoreObject.GetData();
                var size = (int)blob.mCoreObject.GetSize();
                if (data == IntPtr.Zero || size == 0)
                    return string.Empty;
                return Marshal.PtrToStringUTF8(data, size);
            }
            set => SetText(value);
        }

        public void SetText(string text)
        {
            mCoreObject.SetText(text ?? "");
        }

        public void SetLanguage(string language, bool apply = true)
        {
            mCoreObject.SetLanguage(language ?? "C#");
            if (apply)
                mCoreObject.ApplyLangDefine();
        }

        public void PushPreprocIdentifier(string name, string value)
        {
            mCoreObject.PushPreprocIdentifier(name ?? "", value ?? "");
        }

        public void PushIdentifier(string name, string value)
        {
            mCoreObject.PushIdentifier(name ?? "", value ?? "");
        }

        public void ApplyLanguageDefinition()
        {
            mCoreObject.ApplyLangDefine();
        }

        public void SetErrorMarkers(IEnumerable<(int Line, string Message)> markers)
        {
            TryClearErrorMarkers();
            if (markers != null)
            {
                foreach (var marker in markers)
                {
                    if (marker.Line >= 0)
                        mCoreObject.PushErrorMarker(marker.Line, marker.Message ?? "");
                }
            }
            mCoreObject.ApplyErrorMarkers();
        }

        public void SetCursorPosition(int line, int column = 0)
        {
            TrySetCursorPosition(line, column);
        }

        public void Render(string title, in Vector2 size, bool border = false)
        {
            mCoreObject.Render(title ?? "CodeEditor", in size, border);
        }

        public void Undo()
        {
            mCoreObject.Undo();
        }

        public void Redo()
        {
            mCoreObject.Redo();
        }

        public void Copy()
        {
            mCoreObject.Copy();
        }

        public void Cut()
        {
            mCoreObject.Cut();
        }

        public void Paste()
        {
            mCoreObject.Paste();
        }

        public void Delete()
        {
            mCoreObject.Delete();
        }

        public void SelectAll()
        {
            mCoreObject.SelectAll();
        }

        public void SetReadOnly(bool value)
        {
            mCoreObject.SetReadOnly(value);
        }

        public bool IsReadOnly
        {
            get => mCoreObject.IsReadOnly();
        }

        public void SetViewStyle(ECodeEditorViewStyle style)
        {
            mCoreObject.SetViewStyle(style.ToString());
        }

        private static bool sNativeCodeEditorBridgeAvailable = true;

        private void TryClearErrorMarkers()
        {
            if (!sNativeCodeEditorBridgeAvailable || !mCoreObject.IsValidPointer)
                return;

            try
            {
                TitanCodeEditor_ClearErrorMarkers(mCoreObject.NativePointer);
            }
            catch (EntryPointNotFoundException)
            {
                sNativeCodeEditorBridgeAvailable = false;
            }
            catch (DllNotFoundException)
            {
                sNativeCodeEditorBridgeAvailable = false;
            }
        }

        private void TrySetCursorPosition(int line, int column)
        {
            if (!sNativeCodeEditorBridgeAvailable || !mCoreObject.IsValidPointer)
                return;

            try
            {
                TitanCodeEditor_SetCursorPosition(mCoreObject.NativePointer, line, column);
            }
            catch (EntryPointNotFoundException)
            {
                sNativeCodeEditorBridgeAvailable = false;
            }
            catch (DllNotFoundException)
            {
                sNativeCodeEditorBridgeAvailable = false;
            }
        }

        [DllImport(EngineNS.CoreSDK.CoreModule, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TitanCodeEditor_ClearErrorMarkers")]
        private static extern void TitanCodeEditor_ClearErrorMarkers(IntPtr editor);

        [DllImport(EngineNS.CoreSDK.CoreModule, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TitanCodeEditor_SetCursorPosition")]
        private static extern void TitanCodeEditor_SetCursorPosition(IntPtr editor, int line, int column);
    }
}
