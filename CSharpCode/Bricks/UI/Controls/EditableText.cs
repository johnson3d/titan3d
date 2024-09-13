using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using EngineNS.UI.Bind;
using EngineNS.Bricks.Input;
using EngineNS.UI.Event;
using SixLabors.ImageSharp.Advanced;
using EngineNS.UI.Canvas;

namespace EngineNS.UI.Controls
{
    [Editor_UIControl("Controls.EditableText", "Editable text control, for input string", "")]
    public partial class TtEditableText : TtText
    {
        TtBrush mCursorBrush;
        [BindProperty, Rtti.Meta]
        public TtBrush CursorBrush
        {
            get => mCursorBrush;
            set
            {
                OnValueChange(value, mCursorBrush);
                mCursorBrush = value;
                if(mCursorBrush != null)
                {
                    mCursorBrush.HostElement = this;
                }
            }
        }
        float mCursorWidth = 3.0f;
        [BindProperty, Rtti.Meta]
        public float CursorWidth
        {
            get => mCursorWidth;
            set
            {
                OnValueChange(value, mCursorWidth);
                mCursorWidth = value;
            }
        }

        TtBrush mSelectionBrush;
        [BindProperty, Rtti.Meta]
        public TtBrush SelectionBrush
        {
            get => mSelectionBrush;
            set
            {
                OnValueChange(value, mSelectionBrush);
                mSelectionBrush.HostElement = this;
                mSelectionBrush = value;
            }
        }

        protected int mSelectionStart = 0;
        [BindProperty(DefaultMode = EBindingMode.OneWayToSource)]
        public int SelectionStart
        {
            get => mSelectionStart;
            set
            {
                if (mSelectionStart == value)
                    return;
                OnValueChange(value, mSelectionStart);
                mSelectionStart = value;
                mSelectionDrawDirty = true;
                MeshDirty = true;
            }
        }
        protected int mSelectionLength = 0;
        [BindProperty(DefaultMode = EBindingMode.OneWayToSource)]
        public int SelectionLength
        {
            get => mSelectionLength;
            set
            {
                if (mSelectionLength == value)
                    return;
                OnValueChange(value, mSelectionLength);
                mSelectionLength = value;
                mSelectionDrawDirty = true;
                MeshDirty = true;
            }
        }

        protected bool mSelectAllWhenFocused = false;
        [Rtti.Meta, BindProperty]
        public bool SelectAllWhenFocused
        {
            get => mSelectAllWhenFocused;
            set
            {
                OnValueChange(value, mSelectAllWhenFocused);
                mSelectAllWhenFocused = value;
            }
        }
        
        int mCursorIndex = -1;
        public int CursorIndex
        {
            get => mCursorIndex;
            set
            {
                var temp = value;
                if (temp > Text.Length)
                    temp = Text.Length;
                if (mCursorIndex == temp)
                    return;
                mCursorIndex = temp;
                MeshDirty = true;

                //if(mCursorIndex >= 0)
                //    UInputSystem.IMEHandler.SetIMEStatus(true, new Vector2(0, 0));
            }
        }
        bool mSelectionDrawDirty = false;

        public static readonly TtRoutedEvent TextInputEvent = TtEventManager.RegisterRoutedEvent("TextInput", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtEditableText));
        public event TtRoutedEventHandler TextInput
        {
            add { AddHandler(TextInputEvent, value); }
            remove { RemoveHandler(TextInputEvent, value); }
        }
        public static readonly TtRoutedEvent TextEditingEvent = TtEventManager.RegisterRoutedEvent("TextEditing", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtEditableText));
        public event TtRoutedEventHandler TextEditing
        {
            add { AddHandler(TextEditingEvent, value); }
            remove { RemoveHandler(TextEditingEvent, value); }
        }

        public TtEditableText()
        {
            IsFocusable = true;

            mCursorBrush = new TtBrush();
            mCursorBrush.HostElement = this;
            mCursorBrush.BrushType = TtBrush.EBrushType.Rectangle;
            mCursorBrush.Material = RName.GetRName("ui/uicursor_mat_inst.uminst", RName.ERNameType.Engine);
            mSelectionBrush = new TtBrush();
            mSelectionBrush.HostElement = this;
            mSelectionBrush.BrushType = TtBrush.EBrushType.Rectangle;
            mSelectionBrush.Color = EngineNS.Color4b.Blue;

            MouseLeftButtonDown += TtEditableText_MouseLeftButtonDown;
            TouchDown += TtEditableText_MouseLeftButtonDown;
            MouseLeftButtonUp += TtEditableText_MouseLeftButtonUp;
            TouchUp += TtEditableText_MouseLeftButtonUp;
            MouseMove += TtEditableText_MouseMove;
            TouchMove += TtEditableText_MouseMove;
            KeyDown += TtEditableText_KeyDown;
            KeyUp += TtEditableText_KeyUp;
            OnFocus += TtEditableText_OnFocus;
            OnLostFocus += TtEditableText_OnLostFocus;
            TextInput += TtEditableText_TextInput;
            TextEditing += TtEditableText_TextEditing;
        }

        int mMouseDownCursor = 0;
        private unsafe void TtEditableText_MouseLeftButtonDown(object sender, TtRoutedEventArgs args)
        {
            args.Handled = true;

            if(!IsKeyboardFocused && SelectAllWhenFocused)
            {
                SelectionStart = 0;
                SelectionLength = Text.Length;
            }
            else
            {
                var mousePt = new Vector2(args.InputEventPtr->MouseButton.X, args.InputEventPtr->MouseButton.Y);
                mMouseDownCursor = CalculateCursorPos(in mousePt);
                SelectionStart = mMouseDownCursor;
                SelectionLength = 0;
            }

            TtEngine.Instance.UIManager.KeyboardFocus(args, this);
            TtEngine.Instance.UIManager.CaptureMouse(args, this);

        }
        int CalculateCursorPos(in Vector2 mousePt)
        {
            Vector2 offset;
            int cursorIndex = -1;
            if (GetMousePointOffset(in mousePt, out offset))
            {
                bool bFind = false;
                for (int i = 0; i < mTextInLines.Count; i++)
                {
                    var line = mTextInLines[i];
                    if (line.Rect.Contains(in offset))
                    {
                        float wordSizeX = 0;
                        var offsetInLineX = offset.X - line.Rect.X;
                        for (int wordIdxInLine = 0; wordIdxInLine < line.Count; wordIdxInLine++)
                        {
                            var halfAdvance = line.WordAdvances[wordIdxInLine] * 0.5f;
                            var temp = wordSizeX + halfAdvance;
                            if (wordSizeX <= offsetInLineX && temp > offsetInLineX)
                            {
                                cursorIndex = line.StartIndex + wordIdxInLine;
                                bFind = true;
                                break;
                            }
                            wordSizeX = temp;
                            temp = wordSizeX + halfAdvance;
                            if (wordSizeX <= offsetInLineX && temp > offsetInLineX)
                            {
                                cursorIndex = line.StartIndex + wordIdxInLine + 1;
                                bFind = true;
                                break;
                            }
                            wordSizeX = temp;
                        }
                        break;
                    }
                }
                if (!bFind)
                    cursorIndex = mText.Length;
            }
            return cursorIndex;
        }
        private unsafe void TtEditableText_MouseLeftButtonUp(object sender, TtRoutedEventArgs args)
        {
            args.Handled = true;
            if (IsMouseCaptured)
                TtEngine.Instance.UIManager.CaptureMouse(args, null);

            // 计算光标位置
            var mousePt = new Vector2(args.InputEventPtr->MouseButton.X, args.InputEventPtr->MouseButton.Y);
            var cursorIdx = CalculateCursorPos(in mousePt);

            if(SelectionLength <= 0)
            {
                CursorIndex = cursorIdx;
            }
            else
            {
                var start = MathHelper.Min(cursorIdx, mMouseDownCursor);
                var end = MathHelper.Max(cursorIdx, mMouseDownCursor);
                SelectionStart = start;
                SelectionLength = end - start;
                if (SelectionLength <= 0)
                    CursorIndex = cursorIdx;
            }
        }
        private unsafe void TtEditableText_MouseMove(object sender, TtRoutedEventArgs args)
        {
            if(IsMouseCaptured && TtEngine.Instance.InputSystem.Mouse.IsMouseButtonDown(Bricks.Input.EMouseButton.BUTTON_LEFT))
            {
                args.Handled = true;

                // selection text
                var mousePt = new Vector2(args.InputEventPtr->MouseButton.X, args.InputEventPtr->MouseButton.Y);
                var curIdx = CalculateCursorPos(in mousePt);
                var start = MathHelper.Min(mMouseDownCursor, curIdx);
                var end = MathHelper.Max(mMouseDownCursor, curIdx);
                SelectionStart = start;
                SelectionLength = end - start;
            }
        }
        private unsafe void TtEditableText_KeyDown(object sender, TtRoutedEventArgs args)
        {
            if (args.InputEventPtr == null)
                return;
            switch(args.InputEventPtr->Keyboard.Keysym.Sym)
            {
                case Keycode.KEY_LEFT:
                    {
                        if(SelectionLength > 0)
                        {
                            SelectionLength = 0;
                            CursorIndex = SelectionStart;
                        }
                        if(CursorIndex > 0)
                            CursorIndex--;
                    }
                    break;
                case Keycode.KEY_RIGHT:
                    if(SelectionLength > 0)
                    {
                        SelectionLength = 0;
                        CursorIndex = SelectionStart;
                    }
                    CursorIndex++;
                    break;
                case Keycode.KEY_UP:
                    {
                        if(SelectionLength > 0)
                        {
                            SelectionLength = 0;
                            CursorIndex = SelectionStart;
                        }
                        float offset;
                        var idx = GetCursorLineIndex(out offset);
                        if(idx > 0)
                        {
                            var absX = mTextInLines[idx].Rect.X + offset;
                            var targetLine = mTextInLines[idx - 1];
                            var offsetInTargetLine = absX - targetLine.Rect.X;
                            float tempAdvances = 0;
                            for(int i=0; i < targetLine.Count; i++)
                            {
                                var halfAdvance = targetLine.WordAdvances[i] * 0.5f;
                                var temp = tempAdvances + halfAdvance;
                                if(tempAdvances <= offsetInTargetLine && temp > offsetInTargetLine)
                                {
                                    CursorIndex = targetLine.StartIndex + i;
                                    break;
                                }
                                tempAdvances = temp;
                                temp = tempAdvances + halfAdvance;
                                if(tempAdvances <= offsetInTargetLine && temp > offsetInTargetLine)
                                {
                                    CursorIndex = targetLine.StartIndex + i + 1;
                                    break;
                                }
                                tempAdvances = temp;
                                if (i == targetLine.Count - 1)
                                {
                                    CursorIndex = targetLine.StartIndex + targetLine.Count;
                                }
                            }
                        }
                    }
                    break;
                case Keycode.KEY_DOWN:
                    {
                        if (SelectionLength > 0)
                        {
                            SelectionLength = 0;
                            CursorIndex = SelectionStart;
                        }
                        float offset;
                        var idx = GetCursorLineIndex(out offset);
                        if(idx >= 0 && idx < (mTextInLines.Count - 1))
                        {
                            var absX = mTextInLines[idx].Rect.X + offset;
                            var targetLine = mTextInLines[idx + 1];
                            var offsetInTargetLine = absX - targetLine.Rect.X;
                            float tempAdvances = 0;
                            for (int i = 0; i < targetLine.Count; i++)
                            {
                                var halfAdvance = targetLine.WordAdvances[i] * 0.5f;
                                var temp = tempAdvances + halfAdvance;
                                if (tempAdvances <= offsetInTargetLine && temp > offsetInTargetLine)
                                {
                                    CursorIndex = targetLine.StartIndex + i;
                                    break;
                                }
                                tempAdvances = temp;
                                temp = tempAdvances + halfAdvance;
                                if (tempAdvances <= offsetInTargetLine && temp > offsetInTargetLine)
                                {
                                    CursorIndex = targetLine.StartIndex + i + 1;
                                    break;
                                }
                                tempAdvances = temp;
                                if(i == targetLine.Count - 1)
                                {
                                    CursorIndex = targetLine.StartIndex + targetLine.Count;
                                }
                            }
                        }
                    }
                    break;
                case Keycode.KEY_DELETE:
                    if(SelectionLength > 0)
                    {
                        var start = MathHelper.Min(Text.Length - 1, MathHelper.Max(0, SelectionStart));
                        var length = MathHelper.Min(SelectionLength, Text.Length - start);
                        Text = Text.Remove(start, length);
                        SelectionLength = 0;
                        CursorIndex = SelectionStart;
                    }
                    else if(mCursorIndex < Text.Length)
                    {
                        if(Text.Length > 0)
                            Text = Text.Remove(mCursorIndex, 1);
                    }
                    break;
                case Keycode.KEY_BACKSPACE:
                    if(SelectionLength > 0)
                    {
                        var start = MathHelper.Min(Text.Length - 1, MathHelper.Max(0, SelectionStart));
                        var length = MathHelper.Min(SelectionLength, Text.Length - start);
                        Text = Text.Remove(start, length);
                        SelectionLength = 0;
                        CursorIndex = SelectionStart;
                    }
                    else if(mCursorIndex > 0)
                    {
                        Text = Text.Remove(CursorIndex - 1, 1);
                        CursorIndex--;
                    }
                    break;
            }
        }
        private void TtEditableText_KeyUp(object sender, TtRoutedEventArgs args)
        {

        }

        private unsafe void TtEditableText_OnFocus(object sender, TtRoutedEventArgs args)
        {
            args.Handled = true;
            
            TtInputSystem.StartTextInput();
            TtInputSystem.SetTextInputRect(in mDesignRect);

            if(SelectAllWhenFocused)
            {
                SelectionStart = 0;
                SelectionLength = Text.Length;
            }
        }
        private void TtEditableText_OnLostFocus(object sender, TtRoutedEventArgs args)
        {
            args.Handled = true;
            CursorIndex = -1;
            TtInputSystem.StopTextInput();
        }
        private unsafe void TtEditableText_TextInput(object sender, TtRoutedEventArgs args)
        {
            if (args.InputEventPtr == null)
                return;

            if(SelectionLength > 0)
            {
                Text = Text.Remove(SelectionStart, SelectionLength);
                SelectionLength = 0;
                CursorIndex = SelectionStart;
            }

            if (!string.IsNullOrEmpty(mEditingText))
            {
                Text = Text.Remove(CursorIndex, mEditingText.Length);
                mEditingText = "";
            }

            var text = args.InputEventPtr->TextInput.Text;
            if (mCursorIndex < 0 || mCursorIndex >= mText.Length)
            {
                Text += text;
                CursorIndex = Text.Length;
            }
            else
            {
                Text = Text.Insert(mCursorIndex, text);
                CursorIndex += text.Length;
            }
        }
        string mEditingText;
        private unsafe void TtEditableText_TextEditing(object sender, TtRoutedEventArgs args)
        {
            if (args.InputEventPtr == null)
                return;
            if (CursorIndex < 0)
                return;
            var editingText = args.InputEventPtr->TextEdit.Text;
            if(!string.IsNullOrEmpty(editingText))
            {
                if(!string.IsNullOrEmpty(mEditingText))
                    Text = Text.Remove(CursorIndex, mEditingText.Length);
                mEditingText = editingText;
                Text = Text.Insert(CursorIndex, mEditingText);
            }
            else
            {
                if(!string.IsNullOrEmpty(mEditingText))
                    Text = Text.Remove(CursorIndex, mEditingText.Length);
                mEditingText = editingText;
            }
        }

        private void UpdateSelectionDraw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            if (mSelectionDrawDirty == false)
                return;
            if (mSelectionLength <= 0)
                return;
            if (SelectionStart < 0)
                mSelectionStart = 0;
            if (mSelectionLength > Text.Length)
                mSelectionLength = Text.Length;

            var scaleDelta = mFontSize * 1.0f / FontAsset.FontSize;
            var lineHeight = mFontSize * mLineSpacingScale;

            for (int i = 0; i < mTextInLines.Count; i++)
            {
                var line = mTextInLines[i];
                if ((line.StartIndex >= (SelectionStart + SelectionLength)) ||
                   ((line.StartIndex + line.Count) <= SelectionStart))
                    continue;

                var selectStartInLine = MathHelper.Max(SelectionStart, line.StartIndex);
                var selectCountInLine = MathHelper.Min((SelectionStart + SelectionLength), (line.StartIndex + line.Count)) - selectStartInLine;
                float offset = 0;
                for(int sIdx = line.StartIndex; sIdx < selectStartInLine; sIdx++)
                {
                    offset += line.WordAdvances[sIdx - line.StartIndex];
                }
                RectangleF selectionRect = new RectangleF();
                selectionRect.X = line.Rect.X + offset + DesignRect.X;
                selectionRect.Y = line.Rect.Y + DesignRect.Y;
                selectionRect.Height = line.Rect.Height;
                for(int sIdx = selectStartInLine; sIdx < (selectStartInLine + selectCountInLine); sIdx++)
                {
                    selectionRect.Width += line.WordAdvances[sIdx - line.StartIndex];
                }
                SelectionBrush.Draw(this, in mDesignClipRect, in selectionRect, batch);
            }
        }
        public override bool IsReadyToDraw()
        {
            if (!CursorBrush.IsReadyToDraw())
                return false;
            if (!SelectionBrush.IsReadyToDraw())
                return false;
            return base.IsReadyToDraw();
        }
        int GetCursorLineIndex(out float widthOffsetInLine)
        {
            widthOffsetInLine = 0;
            for (int i = 0; i < mTextInLines.Count; i++)
            {
                var line = mTextInLines[i];
                if (mCursorIndex < line.StartIndex)
                    continue;
                if (i < mTextInLines.Count - 1)
                {
                    if (mCursorIndex >= (line.StartIndex + line.Count))
                        continue;
                }

                var maxIndex = MathHelper.Min(mCursorIndex, mText.Length);
                for (int idx = line.StartIndex; idx < maxIndex; idx++)
                {
                    widthOffsetInLine += line.WordAdvances[idx - line.StartIndex];
                }
                return i;
            }
            return -1;
        }
        public override void Draw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            // draw selection background
            UpdateSelectionDraw(canvas, batch);
            base.Draw(canvas, batch);
            // draw cursor
            if(mCursorIndex >= 0 && SelectionLength <= 0)
            {
                float offset = 0.0f;
                var halfCursorWidth = CursorWidth * 0.5f;
                int idx = GetCursorLineIndex(out offset);
                if (idx < 0)
                {
                    RectangleF cursorRect = new RectangleF(offset - halfCursorWidth + DesignRect.X, DesignRect.Y, CursorWidth, mFontSize * mLineSpacingScale);
                    RectangleF cursorClip = new RectangleF(mDesignClipRect.X - halfCursorWidth, mDesignClipRect.Y, mDesignClipRect.Width + halfCursorWidth, mDesignClipRect.Height);
                    CursorBrush.Draw(this, in cursorClip, in cursorRect, batch);
                }
                else
                {
                    var line = mTextInLines[idx];
                    RectangleF cursorRect = new RectangleF(offset - halfCursorWidth + DesignRect.X, line.Rect.Y + DesignRect.Y, CursorWidth, line.Rect.Height);
                    RectangleF cursorClip = new RectangleF(mDesignClipRect.X - halfCursorWidth, mDesignClipRect.Y, mDesignClipRect.Width + halfCursorWidth, mDesignClipRect.Height);
                    CursorBrush.Draw(this, in cursorClip, in cursorRect, batch);
                }
            }
        }
    }
}
