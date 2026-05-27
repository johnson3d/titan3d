#ifndef	 _SYSTEM_ENUM_DEFINE_H_
#define _SYSTEM_ENUM_DEFINE_H_

// RenderFlags_10Bit layout (stored in RT1.b as 10-bit UNORM):
// bit0:   DisableEnvColor
// bit1:   AcceptShadow (merged from per-mesh ObjectFlags at encode time)
// bit2:   UnLight      (merged from per-mesh ObjectFlags at encode time)
// bit3~5: reserved
// bit6~9: ShadingMode (4 bits, supports 0~15)
#define ERenderFlags_DisableEnvColor    0x0001
#define ERenderFlags_AcceptShadow       0x0002
#define ERenderFlags_UnLight            0x0004

#define SHADINGMODE_BIT_OFFSET          6
#define SHADINGMODE_BIT_MASK            0x03C0  // (0xF << 6)

#endif 