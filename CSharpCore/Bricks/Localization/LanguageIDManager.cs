using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Localization
{
    public class LanguageManager
    {
        public static string UTF8_GB2312(string text)
        {
            //声明字符集   
            System.Text.Encoding utf8, gb2312;
            //utf8   
            utf8 = System.Text.Encoding.GetEncoding("utf-8");
            //gb2312   
            gb2312 = System.Text.Encoding.GetEncoding("gb2312");
            byte[] utf;
            utf = utf8.GetBytes(text);
            utf = System.Text.Encoding.Convert(utf8, gb2312, utf);
            //返回转换后的字符   
            return gb2312.GetString(utf);
        }
        public static string GB2312_UTF8(string text)
        {
            //声明字符集   
            System.Text.Encoding utf8, gb2312;
            //gb2312   
            gb2312 = System.Text.Encoding.GetEncoding("gb2312");
            //utf8   
            utf8 = System.Text.Encoding.GetEncoding("utf-8");
            byte[] gb;
            gb = gb2312.GetBytes(text);
            gb = System.Text.Encoding.Convert(gb2312, utf8, gb);
            //返回转换后的字符   
            return utf8.GetString(gb);
        }
        public static string JP932_GB2312(string text)
        {
            var srcEncoding = "shift_jis";//"utf-8","utf-16","shift_jis","iso-2022-jp","csISO2022JP","euc-jp"
            //声明字符集   
            System.Text.Encoding src, gb2312;
            src = System.Text.Encoding.GetEncoding(srcEncoding);//"cp932","cp936"
            //gb2312   
            gb2312 = System.Text.Encoding.GetEncoding("gb2312");
            byte[] target;
            target = src.GetBytes(text);
            target = System.Text.Encoding.Convert(src, gb2312, target);
            //返回转换后的字符   
            return gb2312.GetString(target);
        }
        public string DefaultLanguage = "zh_cn";
        public string FileExt = ".lang";
        public string LanguagePath = "languages/";
        string mCurrentLanguage;
        public string CurrentLanguage
        {
            get { return mCurrentLanguage; }
            set
            {
                mCurrentLanguage = value;

                if (!Texts.ContainsKey(mCurrentLanguage) && !mLostLanguage.Contains(mCurrentLanguage))
                {
                    if (LoadMapper(mCurrentLanguage) == false)
                        mLostLanguage.Add(mCurrentLanguage);
                }
            }
        }

        static LanguageManager smInstance = new LanguageManager();
        public static LanguageManager Instance
        {
            get { return smInstance; }
        }

        List<string> mLostLanguage = new List<string>();

        Dictionary<string, Dictionary<UInt32, string>> mTexts = new Dictionary<string, Dictionary<UInt32, string>>();
        public Dictionary<string, Dictionary<UInt32, string>> Texts
        {
            get { return mTexts; }
            set { mTexts = value; }
        }

        private LanguageManager()
        {
            CurrentLanguage = DefaultLanguage;
            LoadMapper(DefaultLanguage);
        }
        ~LanguageManager()
        {
            //SaveMapper(DefaultLanguage);
        }

        public bool LoadMapper(string language)
        {
            var fileName = RName.GetRName(LanguagePath + language + FileExt);
            using (var xml = IO.XmlHolder.LoadXML(fileName.Address))
            {
                if (xml == null || xml.RootNode == null)
                    return false;

                var lanTexts = new Dictionary<UInt32, string>();
                mTexts[language] = lanTexts;
                var root = xml.RootNode;
                var texts = root.GetNodes();
                foreach (var i in texts)
                {
                    var name = i.FindAttrib("ID");

                    if (name == null || string.IsNullOrEmpty(name.Value))
                        continue;
                    var id = System.Convert.ToUInt32(name.Value);

                    string textValue = "";
                    var text = i.FindAttrib("Text");
                    if (text != null)
                    {
                        textValue = text.Value;
                    }

                    lanTexts[id] = textValue;
                }
            }
            return true;
        }
        public void SaveMapper()
        {
            foreach (var lang in mTexts)
            {
                var xml = IO.XmlHolder.NewXMLHolder("lang", "");
                var root = xml.RootNode;

                foreach (var texValue in lang.Value)
                {
                    var node = root.AddNode("Text", "", xml);
                    node.AddAttrib("ID", texValue.Key.ToString());
                    node.AddAttrib("Text", texValue.Value);
                }
                IO.XmlHolder.SaveXML(LanguagePath + lang.Key + FileExt, xml, false);
            }
        }

        private string GetErrorString(UInt64 id, string language)
        {
            return "(" + language + " id=" + id + ")";
        }

        public static string Translate(UInt32 id, string language = "")
        {
            return Instance._Translate(id, language);
        }
        private string _Translate(UInt32 id, string language = "")
        {
            if (language == "" || language == null)
            {
                language = CurrentLanguage;
            }
            //if (lang == "")
            //    return str;

            Dictionary<UInt32, string> text;
            if (mTexts.TryGetValue(language, out text))
            {
                string astr;
                if (text.TryGetValue(id, out astr))
                    return astr;
            }
            else
            {
                if (mLostLanguage.Contains(language))
                    return GetErrorString(id, language);
                if (!LoadMapper(language))
                {
                    mLostLanguage.Add(language);
                    return GetErrorString(id, language);
                }

                if (mTexts.TryGetValue(language, out text))
                {
                    string astr;
                    if (text.TryGetValue(id, out astr))
                        return astr;
                }
            }

            return GetErrorString(id, language);
        }

        // id对应格式化的字符串，如：玩家{0}击杀{1}个{2}... 大括号内的对象会被parameters按顺序填充
        public static string Translate(UInt32 id, string language, params string[] parameters)
        {
            return Instance._Translate(id, language, parameters);
        }
        public string _Translate(UInt32 id, string language, params string[] parameters)
        {
            if (language == "" || language == null)
                language = CurrentLanguage;

            if (!mTexts.ContainsKey(language))
            {
                if (mLostLanguage.Contains(language))
                    return GetErrorString(id, language);
                if (!LoadMapper(language))
                {
                    mLostLanguage.Add(language);
                    return GetErrorString(id, language);
                }
            }

            Dictionary<UInt32, string> text;
            if (mTexts.TryGetValue(language, out text))
            {
                string astr;
                if (text.TryGetValue(id, out astr))
                {
                    if (parameters == null)
                        return astr;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        astr = astr.Replace("{" + i + "}", parameters[i]);
                    }
                    return astr;
                }
            }

            return GetErrorString(id, language);
        }
    }
}
