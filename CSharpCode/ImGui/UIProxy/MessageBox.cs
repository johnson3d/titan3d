using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class MessageBox
    {
        public enum EButtonType
        {
            OKCancel,
            YesNo,
            YesNoCancel,
        }

        public static void Open(string title)
        {
            ImGuiAPI.OpenPopup(title, ImGuiPopupFlags_.ImGuiPopupFlags_None);
        }

        public static unsafe void Draw(string title, string message, EButtonType buttonType, params Action[] actions)
        {
            if(ImGuiAPI.BeginPopupModal(title, (bool*)0, ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize))
            {
                ImGuiAPI.Text(message);

                switch(buttonType)
                {
                    case EButtonType.OKCancel:
                        {
                            if (ImGuiAPI.Button("OK", in Vector2.Zero))
                            {
                                if (actions.Length > 0)
                                    actions[0]?.Invoke();
                                ImGuiAPI.CloseCurrentPopup();
                            }
                            ImGuiAPI.SetItemDefaultFocus();
                            ImGuiAPI.SameLine(0, -1);
                            if (ImGuiAPI.Button("Cancel", in Vector2.Zero))
                            {
                                if (actions.Length > 1)
                                    actions[1]?.Invoke();
                                ImGuiAPI.CloseCurrentPopup();
                            }
                        }
                        break;
                    case EButtonType.YesNo:
                        {
                            if(ImGuiAPI.Button("Yes", in Vector2.Zero))
                            {
                                if (actions.Length > 0)
                                    actions[0]?.Invoke();
                                ImGuiAPI.CloseCurrentPopup();
                            }
                            ImGuiAPI.SetItemDefaultFocus();
                            ImGuiAPI.SameLine(0, -1);
                            if(ImGuiAPI.Button("No", in Vector2.Zero))
                            {
                                if (actions.Length > 1)
                                    actions[1]?.Invoke();
                                ImGuiAPI.CloseCurrentPopup();
                            }
                        }
                        break;
                    case EButtonType.YesNoCancel:
                        {
                            if (ImGuiAPI.Button("Yes", in Vector2.Zero))
                            {
                                if (actions.Length > 0)
                                    actions[0]?.Invoke();
                                ImGuiAPI.CloseCurrentPopup();
                            }
                            ImGuiAPI.SetItemDefaultFocus();
                            ImGuiAPI.SameLine(0, -1);
                            if (ImGuiAPI.Button("No", in Vector2.Zero))
                            {
                                if (actions.Length > 1)
                                    actions[1]?.Invoke();
                                ImGuiAPI.CloseCurrentPopup();
                            }
                            ImGuiAPI.SetItemDefaultFocus();
                            ImGuiAPI.SameLine(0, -1);
                            if (ImGuiAPI.Button("Cancel", in Vector2.Zero))
                            {
                                if (actions.Length > 2)
                                    actions[2]?.Invoke();
                                ImGuiAPI.CloseCurrentPopup();
                            }
                        }
                        break;
                }

                ImGuiAPI.EndPopup();
            }
        }
    }
}
