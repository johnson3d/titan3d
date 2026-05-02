using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    [FSamplerDesc.TypeConverter]
    public unsafe partial struct FSamplerDesc
    {
        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                obj = FSamplerDesc.FromString(text);
                return true;
            }
        }

        public override string ToString()
        {
            return $"{(int)m_Filter},{(int)m_CmpMode},{(int)m_AddressU},{(int)m_AddressU},{(int)m_AddressV},{(int)m_AddressW},{m_MaxAnisotropy},{m_MipLODBias},{m_MinLOD},{m_MaxLOD}";
        }
        public static FSamplerDesc FromString(string text)
        {
            try
            {
                var result = new FSamplerDesc();
                ReadOnlySpan<char> chars = text.ToCharArray();
                int iStart = 0;
                int j = 0;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (chars[i] == ',')
                    {
                        switch (j)
                        {
                            case 0:
                                result.Filter = (ESamplerFilter)int.Parse(chars.Slice(iStart, i - iStart));
                                break;
                            case 1:
                                result.CmpMode = (EComparisionMode)int.Parse(chars.Slice(iStart, i - iStart));
                                break;
                            case 2:
                                result.AddressU = (EAddressMode)int.Parse(chars.Slice(iStart, i - iStart));
                                break;
                            case 3:
                                result.AddressV = (EAddressMode)int.Parse(chars.Slice(iStart, i - iStart));
                                break;
                            case 4:
                                result.AddressW = (EAddressMode)int.Parse(chars.Slice(iStart, i - iStart));
                                break;
                            case 5:
                                result.MaxAnisotropy = uint.Parse(chars.Slice(iStart, i - iStart));
                                break;
                            case 6:
                                result.MipLODBias = float.Parse(chars.Slice(iStart, i - iStart));
                                break;
                            case 7:
                                result.MinLOD = float.Parse(chars.Slice(iStart, i - iStart));
                                break;
                            case 8:
                                result.MaxLOD = float.Parse(chars.Slice(iStart, i - iStart));
                                break;
                        }

                        iStart = i + 1;
                        j++;
                    }
                }
                return result;
            }
            catch
            {
                return new FSamplerDesc();
            }
        }

    }
}
