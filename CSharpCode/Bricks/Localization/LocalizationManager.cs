using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EngineNS.Localization
{
    public partial class LocalizationManager : UModule<UEngine>
    {
        public enum ECulture
        {
            Unknow,
            Separator,
            Symbol,
            Punctuation,
            Digit,
            English,
            Chinese,
        }

        public LocalizationManager()
        {

        }

        public bool ICUMode()
        {
            var sortVersion = CultureInfo.InvariantCulture.CompareInfo.Version;
            byte[] bytes = sortVersion.SortId.ToByteArray();
            int version = bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0];
            return version != 0 && version == sortVersion.FullVersion;
        }

        public void SplitTextWithLanguages(string text, ref List<string> splits)
        {
            // https://www.cnblogs.com/crsky/p/13785729.html
            // https://stackoverflow.com/questions/45619497/c-sharp-split-a-string-with-mixed-language-into-different-language-chunks
            // https://blog.csdn.net/weixin_33836874/article/details/91867661
            //splits.Clear();
            //TypeCode cc = text[0].GetTypeCode();
            //CultureInfo.CurrentCulture.
            //CharUnicodeInfo.GetUnicodeCategory
        }

        // 先简单实现中文和英文，后续引入ICU和harfbuzz来处理多国语言和排版
        public int GetLastWordBreaker(string text, int startIndex, int lastIndex, out ECulture breakerCulture)
        {
            breakerCulture = ECulture.Unknow;
            if (lastIndex < startIndex || lastIndex >= text.Length)
                return -1;
            var chr = text[lastIndex];
            var chrCul = GetCulture(chr);
            breakerCulture = chrCul;
            switch (chrCul)
            {
                case ECulture.Chinese:
                    return lastIndex;
                case ECulture.English:
                case ECulture.Digit:
                    {
                        for(int i=lastIndex - 1; i>=startIndex; i--)
                        {
                            var cul = GetCulture(text[i]);
                            breakerCulture = cul;
                            switch(cul)
                            {
                                case ECulture.Separator:
                                case ECulture.Punctuation:
                                    return i;
                                case ECulture.Symbol:
                                    return i;
                            }
                        }
                    }
                    break;
                default:
                    return lastIndex;
            }
            return startIndex;
        }

        public int GetNextWordBreaker(string text, int startIndex, int lastIndex, out ECulture breakerCulture)
        {
            breakerCulture = ECulture.Unknow;
            if (lastIndex < startIndex || lastIndex >= text.Length)
                return -1;
            var chr = text[startIndex];
            var chrCul = GetCulture(chr);
            breakerCulture = chrCul;
            switch (chrCul)
            {
                case ECulture.Chinese:
                    return startIndex + 1;
                case ECulture.English:
                case ECulture.Digit:
                    {
                        for(int i= startIndex; i< lastIndex; i++)
                        {
                            var cul = GetCulture(text[i]);
                            breakerCulture = cul;
                            switch(cul)
                            {
                                case ECulture.Separator:
                                case ECulture.Punctuation:
                                    return i;
                                case ECulture.Symbol:
                                    return i;
                            }
                        }
                    }
                    break;
                default:
                    return startIndex + 1;
            }
            return startIndex;
        }

        public ECulture GetCulture(char chr)
        {
            if (chr > 0x4e00 && chr < 0x9fbb)
                return ECulture.Chinese;
            else if (Char.IsLetter(chr))
                return ECulture.English;
            else if (char.IsDigit(chr))
                return ECulture.Digit;
            else if (char.IsSeparator(chr))
                return ECulture.Separator;
            else if (char.IsSymbol(chr))
                return ECulture.Symbol;
            else if (char.IsPunctuation(chr))
                return ECulture.Punctuation;
            return ECulture.Unknow;
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        Localization.LocalizationManager mLocalizationManager;
        internal Localization.LocalizationManager LocalizationManager
        {
            get
            {
                if (mLocalizationManager == null)
                    mLocalizationManager = new Localization.LocalizationManager();
                return mLocalizationManager;
            }
        }
    }
}