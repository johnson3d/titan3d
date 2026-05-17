#ifndef	 _SYSTEM_ENUM_DEFINE_H_
#define _SYSTEM_ENUM_DEFINE_H_

// ObjectFlags_2Bit (2 bits, stored in rt1.w)
#define EObjectFlags_2Bit_AcceptShadow  1
#define EObjectFlags_2Bit_UnLight       2

// ERenderFlags (bit0~bit5 for flags, bit6~bit9 for ShadingMode)
#define ERenderFlags_DisableEnvColor    1

// ShadingMode occupies bit6~bit9 of RenderFlags_10Bit (4 bits, supports 0~15)
#define SHADINGMODE_BIT_OFFSET          6
#define SHADINGMODE_BIT_MASK            0x03C0  // (0xF << 6)

// EShadingMode enumeration
#define EShadingMode_PBR                0
#define EShadingMode_Subsurface         1

#endif 