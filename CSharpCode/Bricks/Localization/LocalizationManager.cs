using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EngineNS.Localization
{
    public partial class LocalizationManager : UModule<UEngine>
    {
        public static bool ICUMode()
        {
            var sortVersion = CultureInfo.InvariantCulture.CompareInfo.Version;
            byte[] bytes = sortVersion.SortId.ToByteArray();
            int version = bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0];
            return version != 0 && version == sortVersion.FullVersion;
        }

        public static void SplitTextWithLanguages(string text, ref List<string> splits)
        {
            // https://www.cnblogs.com/crsky/p/13785729.html
            // https://stackoverflow.com/questions/45619497/c-sharp-split-a-string-with-mixed-language-into-different-language-chunks
            // https://blog.csdn.net/weixin_33836874/article/details/91867661
            splits.Clear();
            TypeCode cc = text[0].GetTypeCode();
            CultureInfo.CurrentCulture.
            CharUnicodeInfo.GetUnicodeCategory
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