using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EngineNS.EGui.UIProxy
{
    public class SingleInputDialog
    {
        public enum enResult
        {
            None,
            OK,
            Cancel,
        }

        public static void Open(string title)
        {
            ImGuiAPI.OpenPopup(title, ImGuiPopupFlags_.ImGuiPopupFlags_None | ImGuiPopupFlags_.ImGuiPopupFlags_NoOpenOverExistingPopup);
            mErrorString = null;
        }

        static string mErrorString = null;
        public static unsafe enResult Draw(string title, string inputInfo, ref string inputValue, Func<string, string> inputValueMatch)
        {
            enResult retValue = enResult.None;
            if(ImGuiAPI.BeginPopupModal(title, (bool*)0, ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize))
            {
                ImGuiAPI.Text(inputInfo);
                ImGuiAPI.SameLine(0, -1);
                if(string.IsNullOrEmpty(mErrorString))
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, 0xFF0000FF);
                ImGuiAPI.InputText("##in_inputValue", ref inputValue);
                if(string.IsNullOrEmpty(mErrorString))
                    ImGuiAPI.PopStyleColor(1);
                mErrorString = inputValueMatch(inputValue);
                if(!string.IsNullOrEmpty(mErrorString))
                {
                    var clr = new Vector4(1, 0, 0, 1);
                    ImGuiAPI.TextColored(in clr, mErrorString);
                }

                if(ImGuiAPI.Button("OK", in Vector2.Zero) && string.IsNullOrEmpty(mErrorString))
                {
                    retValue = enResult.OK;
                    ImGuiAPI.CloseCurrentPopup();
                }
                ImGuiAPI.SetItemDefaultFocus();
                ImGuiAPI.SameLine(0, -1);
                if(ImGuiAPI.Button("Cancel", in Vector2.Zero))
                {
                    retValue = enResult.Cancel;
                    ImGuiAPI.CloseCurrentPopup();
                }

                ImGuiAPI.EndPopup();
            }
            return retValue;
        }
    }
}
