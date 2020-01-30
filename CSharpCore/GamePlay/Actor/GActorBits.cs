using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace EngineNS.GamePlay.Actor
{
    public class GActorBits
    {
        private Support.BitSet BitSet;
        public GActorBits()
        {
            BitSet = new Support.BitSet((UInt32)EBitDefine.Count);
            SetBit(EBitDefine.Visible, true);
            SetBit(EBitDefine.CastShadow, true);
            SetBit(EBitDefine.Navgation, false);
            SetBit(EBitDefine.NeedRefreshNavgation, true);
            SetBit(EBitDefine.NeedTick, true);
            SetBit(EBitDefine.Selected, false);
            SetBit(EBitDefine.InPVS, true);
            SetBit(EBitDefine.BoundVolume, true);
            SetBit(EBitDefine.HideInGame, false);
            SetBit(EBitDefine.OnlyForGame, false);
            SetBit(EBitDefine.AcceptLights, true);
            SetBit(EBitDefine.StaticObject, true);
        }
        public enum EBitDefine
        {
            Visible,
            CastShadow,
            Navgation,
            NeedRefreshNavgation,
            NeedTick,
            Selected,
            InPVS,
            BoundVolume,
            HideInGame,
            OnlyForGame,
            AcceptLights,
            StaticObject,

            Count,
        }
        public bool IsBit(EBitDefine bit)
        {
            return BitSet.IsBit((int)bit);
        }
        public void SetBit(EBitDefine bit, bool value)
        {
            BitSet.SetBit((int)bit, value);
        }
    }
}
