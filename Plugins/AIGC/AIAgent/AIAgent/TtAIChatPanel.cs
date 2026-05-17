using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.EGui;

namespace EngineNS.Plugins.AIAgent
{
    /// <summary>
    /// ImGui chat panel for the AI Agent. Provides a conversational UI
    /// with message history, streaming output, tool call visualization,
    /// and skill switching.
    /// </summary>
    public class TtAIChatPanel : IRootForm
    {
        private TtAIAgent mAgent;
        private string mInputText = "";
        private readonly List<ChatBubble> mBubbles = new();
        private string mStreamingText = "";
        private bool mIsStreaming;
        private bool mScrollToBottom;
        private string mSelectedSkill = "Default";
        private string mStatusText = "";
        private bool mDockInitialized;
        private ImGuiWindowClass mDockKeyClass;
        private bool mShowChatPanel = true;
        private bool mShowInputPanel = true;

        public bool Visible { get; set; }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        private enum EBubbleType
        {
            User,
            Assistant,
            ToolCall,
            ToolResult,
            Error,
        }

        private struct ChatBubble
        {
            public EBubbleType Type;
            public string Content;
            public string ToolName;
        }

        public TtAIChatPanel()
        {
            TtEngine.RootFormManager.RegRootForm(this);
            Visible = false;
        }

        public bool Initialize(TtAIAgentPlugin plugin)
        {
            if (plugin == null)
                return false;

            mAgent = plugin.Agent;
            if (mAgent == null)
                return false;

            mAgent.OnStreamChunk += OnStreamChunk;
            mAgent.OnToolExecuting += OnToolExecuting;
            mAgent.OnToolExecuted += OnToolExecuted;
            mAgent.OnCompleted += OnCompleted;

            if (mAgent.Config?.Skills != null)
            {
                mSelectedSkill = mAgent.ActiveSkill?.Name ?? "Default";
            }

            return true;
        }

        public void Dispose()
        {
            if (mAgent != null)
            {
                mAgent.OnStreamChunk -= OnStreamChunk;
                mAgent.OnToolExecuting -= OnToolExecuting;
                mAgent.OnToolExecuted -= OnToolExecuted;
                mAgent.OnCompleted -= OnCompleted;
            }
            TtEngine.RootFormManager.UnregRootForm(this);
        }

        #region Agent Event Handlers

        private void OnStreamChunk(string chunk)
        {
            mStreamingText += chunk;
            mScrollToBottom = true;
        }

        private void OnToolExecuting(string toolName, string argsJson)
        {
            mBubbles.Add(new ChatBubble
            {
                Type = EBubbleType.ToolCall,
                Content = argsJson,
                ToolName = toolName
            });
            mStatusText = $"Calling {toolName}...";
            mScrollToBottom = true;
        }

        private void OnToolExecuted(string toolName, string argsJson, string result)
        {
            mBubbles.Add(new ChatBubble
            {
                Type = EBubbleType.ToolResult,
                Content = result,
                ToolName = toolName
            });
            mStatusText = "";
            mScrollToBottom = true;
        }

        private void OnCompleted(object sender, TtAgentEventArgs eventArgs)
        {
            if (mIsStreaming)
            {
                mBubbles.Add(new ChatBubble
                {
                    Type = EBubbleType.Assistant,
                    Content = mStreamingText
                });
                mStreamingText = "";
                mIsStreaming = false;
            }
            mStatusText = "";
            mScrollToBottom = true;
        }

        #endregion

        #region Drawing

        public unsafe void OnDraw()
        {
            if (!Visible)
                return;

            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var windowSize = new Vector2(420, 600);
            ImGuiAPI.SetNextWindowSize(in windowSize, ImGuiCond_.ImGuiCond_FirstUseEver);

            var result = EGui.UIProxy.DockProxy.BeginMainForm("AI Agent", this, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse);
            if (result)
            {
                DockId = ImGuiAPI.GetWindowDockID();
                DrawToolbar();
                ImGuiAPI.Separator();
                ResetDockspace();
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawChatPanel();
            DrawInputPanel();
        }

        private unsafe void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID("AIAgent_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);

            if (mDockInitialized && !force)
                return;

            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            uint topId = 0;
            uint bottomId = 0;
            ImGuiAPI.DockBuilderSplitNode(id, ImGuiDir.ImGuiDir_Down, 0.25f, ref bottomId, ref topId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Chat", mDockKeyClass), topId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Input", mDockKeyClass), bottomId);

            ImGuiAPI.DockBuilderFinish(id);
        }

        private unsafe void DrawToolbar()
        {
            ImGuiAPI.Text("Skill:");
            ImGuiAPI.SameLine(0, 5);

            if (mAgent?.Config?.Skills != null)
            {
                if (ImGuiAPI.BeginCombo("##skill", mSelectedSkill, ImGuiComboFlags_.ImGuiComboFlags_None))
                {
                    foreach (var skill in mAgent.Config.Skills)
                    {
                        bool isSelected = skill.Name == mSelectedSkill;
                        if (ImGuiAPI.Selectable(skill.Name, isSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            mSelectedSkill = skill.Name;
                            mAgent.SwitchSkill(skill.Name);
                            mBubbles.Clear();
                        }
                        if (isSelected)
                            ImGuiAPI.SetItemDefaultFocus();
                    }
                    ImGuiAPI.EndCombo();
                }
            }

            ImGuiAPI.SameLine(0, 10);
            if (ImGuiAPI.Button("Clear", in Vector2.Zero))
            {
                mBubbles.Clear();
                mAgent?.ClearHistory();
                mStreamingText = "";
                mIsStreaming = false;
            }

            if (mAgent != null && mAgent.IsBusy)
            {
                ImGuiAPI.SameLine(0, 10);
                if (ImGuiAPI.Button("Stop", in Vector2.Zero))
                {
                    mAgent.Cancel();
                }
            }
        }

        private unsafe void DrawChatPanel()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Chat", ref mShowChatPanel, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                foreach (var bubble in mBubbles)
                {
                    DrawBubble(in bubble);
                    ImGuiAPI.Spacing();
                }

                if (mIsStreaming && mStreamingText.Length > 0)
                {
                    var assistantColor = new Vector4(0.6f, 0.8f, 1.0f, 1.0f);
                    ImGuiAPI.TextColored(in assistantColor, "Assistant:");
                    ImGuiAPI.RenderMarkdown(mStreamingText);
                    ImGuiAPI.Spacing();
                }

                if (!string.IsNullOrEmpty(mStatusText))
                {
                    var statusColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
                    ImGuiAPI.TextColored(in statusColor, mStatusText);
                }

                if (mScrollToBottom)
                {
                    ImGuiAPI.SetScrollHereY(1.0f);
                    mScrollToBottom = false;
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        private static unsafe void DrawBubble(in ChatBubble bubble)
        {
            switch (bubble.Type)
            {
                case EBubbleType.User:
                {
                    var userColor = new Vector4(0.4f, 1.0f, 0.4f, 1.0f);
                    ImGuiAPI.TextColored(in userColor, "You:");
                    ImGuiAPI.TextWrapped(bubble.Content);
                    break;
                }
                case EBubbleType.Assistant:
                {
                    var assistantColor = new Vector4(0.6f, 0.8f, 1.0f, 1.0f);
                    ImGuiAPI.TextColored(in assistantColor, "Assistant:");
                    ImGuiAPI.RenderMarkdown(bubble.Content);
                    break;
                }
                case EBubbleType.ToolCall:
                {
                    var toolColor = new Vector4(1.0f, 0.8f, 0.3f, 1.0f);
                    ImGuiAPI.TextColored(in toolColor, $"🔧 {bubble.ToolName}");
                    var dimColor = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
                    ImGuiAPI.TextColored(in dimColor, TruncateText(bubble.Content, 200));
                    break;
                }
                case EBubbleType.ToolResult:
                {
                    var resultColor = new Vector4(0.5f, 0.9f, 0.5f, 1.0f);
                    ImGuiAPI.TextColored(in resultColor, $"✓ {bubble.ToolName} result:");
                    var dimColor = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
                    ImGuiAPI.TextColored(in dimColor, TruncateText(bubble.Content, 300));
                    break;
                }
                case EBubbleType.Error:
                {
                    var errorColor = new Vector4(1.0f, 0.3f, 0.3f, 1.0f);
                    ImGuiAPI.TextColored(in errorColor, bubble.Content);
                    break;
                }
            }
        }

        private unsafe void DrawInputPanel()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Input", ref mShowInputPanel, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar);
            if (show)
            {
                var avail = ImGuiAPI.GetContentRegionAvail();
                float sendButtonWidth = 60;
                float spacing = 8;

                // Left: multiline text input (fills all width except send button)
                float inputWidth = avail.X - sendButtonWidth - spacing;
                if (inputWidth < 50)
                    inputWidth = 50;
                var textSize = new Vector2(inputWidth, avail.Y);
                ImGuiAPI.InputTextMultiline("##chat_input", ref mInputText, textSize,
                    ImGuiInputTextFlags_.ImGuiInputTextFlags_None);

                ImGuiAPI.SameLine(0, spacing);

                // Right: send button (fixed width, full height)
                bool ctrlEnter = ImGuiAPI.IsKeyDown(ImGuiKey.ImGuiKey_LeftCtrl) && ImGuiAPI.IsKeyPressed(ImGuiKey.ImGuiKey_Enter, false);
                var sendSize = new Vector2(sendButtonWidth, avail.Y);
                bool sendClicked = ImGuiAPI.Button("Send", in sendSize);

                if ((ctrlEnter || sendClicked) && !string.IsNullOrWhiteSpace(mInputText) && mAgent != null && !mAgent.IsBusy)
                {
                    SendMessage(mInputText.Trim());
                    mInputText = "";
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        #endregion

        #region Messaging

        private async void SendMessage(string text)
        {
            mBubbles.Add(new ChatBubble
            {
                Type = EBubbleType.User,
                Content = text
            });
            mScrollToBottom = true;
            mIsStreaming = true;
            mStreamingText = "";

            try
            {
                await mAgent.ChatAsync(text);
            }
            catch (Exception ex)
            {
                mBubbles.Add(new ChatBubble
                {
                    Type = EBubbleType.Error,
                    Content = $"Error: {ex.Message}"
                });
                mIsStreaming = false;
            }
        }

        #endregion

        #region Helpers

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            if (text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength) + "...";
        }

        #endregion
    }
}
