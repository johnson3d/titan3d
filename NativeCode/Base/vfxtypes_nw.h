// vfxtypes_32.h
// 
// VictoryCore Code
// Data type define for win32
//
// Author : johnson3d
// More author :
// Create time : 2003-1-20   11:12
// Modify time :
//-----------------------------------------------------------------------------
#ifndef __vfxtypes_32_h__2003_11_12
#define __vfxtypes_32_h__2003_11_12

#pragma once

#include <atomic>

#ifdef IOS
#include <unistd.h>
#endif

#ifndef PLATFORM_WIN
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wc++11-extensions"
#pragma clang diagnostic ignored "-Wc++11-long-long"
#endif


typedef wchar_t					WCHAR;
typedef char					CHAR;
#if defined(VFX_UNICODE)
typedef CHAR					TCHAR;
//typedef WCHAR					TCHAR;
#else // defined(UNICODE) || defined(_UNICODE)
typedef CHAR					TCHAR;
#endif // end UNICODE

typedef CHAR *					LPSTR;
typedef wchar_t *				LPWSTR;
typedef TCHAR *					LPTSTR;
typedef const CHAR *			LPCSTR;
typedef const wchar_t *			LPCWSTR;
typedef const TCHAR *			LPCTSTR;

typedef short					SHORT;
typedef int						INT;
typedef int						LONG;
typedef char					INT8;
typedef short					INT16;
typedef int						INT32;

typedef int64_t 				INT64;

typedef unsigned short			USHORT;
typedef unsigned int			UINT;
typedef unsigned long			ULONG;
typedef unsigned char			UINT8;
typedef unsigned short			UINT16;
typedef unsigned int			UINT;
typedef uint64_t 				UINT64;

typedef UINT8					BYTE;
typedef UINT16					WORD;
typedef UINT					DWORD;
typedef UINT64					QWORD;
	
typedef unsigned char			uchar;
typedef unsigned short			ushort;

typedef struct {
	INT64 a;
	UINT64 b;
	}							vfx_int128;
typedef struct {
	UINT64 a;
	UINT64 b;
}								vfx_uint128;

typedef float					FLOAT;
typedef double					DOUBLE;
typedef long double				LDOUBLE;
typedef float					FLOAT32;
typedef double					FLOAT64;

typedef SHORT *				LPSHORT;
typedef INT * 					LPINT;
typedef LONG * 				LPLONG;
typedef INT8 *					LPINT8;
typedef INT16 *				LPINT16;
typedef INT32 *				LPINT32;
typedef INT64 * 				LPINT64;

typedef const SHORT *			LPCSHORT;
typedef const INT * 			LPCINT;
typedef const INT * 			LPCINT;
typedef const LONG * 			LPCLONG;
typedef const INT8 *			LPCINT8;
typedef const INT16 *			LPCINT16;
typedef const INT32 *			LPCINT32;
typedef const INT64 * 			LPCINT64;

typedef BYTE *					LPBYTE;
typedef USHORT *				LPUSHORT;
typedef	UINT *					LPUINT;
typedef ULONG *				LPULONG;
typedef INT8 *					LPINT8;
typedef INT16 *				LPINT16;
typedef INT32 *				LPINT32;
typedef UINT64 *				LPUINT64;

typedef WORD *					LPWORD;
typedef DWORD *				LPDWORD;
typedef QWORD *				LPQWORD;

typedef const BYTE *			LPCBYTE;
typedef const USHORT *			LPCUSHORT;
typedef	const UINT *			LPCUINT;
typedef const ULONG *			LPCULONG;
typedef const INT8 *			LPCINT8;
typedef const INT16 *			LPCINT16;
typedef const INT32 *			LPCINT32;
typedef const UINT64 *			LPCUINT64;

typedef const WORD *			LPCWORD;
typedef const DWORD *			LPCDWORD;
typedef const QWORD *			LPCQWORD;

typedef FLOAT *				LPFLOAT;
typedef DOUBLE *				LPDOUBLE;
typedef LDOUBLE *				LPLDOUBLE;
typedef FLOAT32 *				LPFLOAT32;
typedef FLOAT64 *				LPFLOAT64;

typedef const FLOAT *			LPCFLOAT;
typedef const DOUBLE *			LPCDOUBLE;
typedef const LDOUBLE *		LPCLDOUBLE;
typedef const FLOAT32 *		LPCFLOAT32;
typedef const FLOAT64 *		LPCFLOAT64;

typedef void					VOID;
typedef void *					LPVOID;
typedef const void *			LPCVOID;

typedef LONG					HRESULT;
typedef LPVOID					HANDLE;

// boolean
typedef INT					vBOOL;
const INT vFALSE				= 0;
const INT vTRUE				= 1;

typedef signed char			int8;
typedef signed short		int16;
typedef signed int			int32;
typedef unsigned char		uint8;
typedef unsigned short		uint16;
typedef unsigned int		uint32;
typedef unsigned long long	uint64;

typedef ULONG	ulong;
typedef USHORT		ushort;
typedef BYTE		uchar;

typedef void*		HWND;

#ifdef PTR_64
#	define INT_PTR			long long
#	define UINT_PTR			unsigned long long
//typedef UINT		uint;
//typedef UINT_PTR			size_t;
#else
#	define INT_PTR			int
#	define UINT_PTR			unsigned int
//typedef UINT				size_t;
#endif

#ifdef VFX_UNICODE
#	define _stscanf		swscanf
#	define _tccmp		strcmp
#	define _tcslen      wcslen
#	define _tcstoul		wcstoul
#	define _tcstoui64	_wcstoui64
#	define _tcscpy		strcpy
#	define _tcsncpy		wcsncpy
#else
#	define _stscanf		sscanf
#	define _tccmp		strcmp
#	define _tcslen      strlen
#	define _tcstoul		strtoul
#	define _tcstoui64	_strtoui64
#	define _tcscpy		strcpy
#	define _tcsncpy		strncpy
#endif

#define S_OK			0
#define E_FAIL			0x80000008

#define MB_OK                       0x00000000L
#define MB_ICONHAND                 0x00000010L
#define MB_ICONERROR                MB_ICONHAND
#define MB_DEFBUTTON1               0x00000000L
#define MB_SYSTEMMODAL              0x00001000L
#define MB_TOPMOST                  0x00040000L
#define MB_YESNO                    0x00000004L
#define IDYES						6

#ifndef PASCAL
#define PASCAL  //__stdcall
#endif

#define TRUE				1
#define FALSE				0

// null
#ifndef NULL
#define NULL					0
#endif

#define CONST					const

#define IN
#define OUT

#define MIN_INT8				-128
#define MAX_INT8				127
#define MIN_INT16				-32768
#define MAX_INT16				32767
#define MIN_INT32				-2147483648
#define MAX_INT32				2147483647
#define MIN_INT64				-9223372036854775808
#define MAX_INT64				9223372036854775807


#if !defined(INT64_MIN)
#define INT64_MIN				MIN_INT64
#endif
#if !defined(INT64_MAX)
#define INT64_MAX				MAX_INT64
#endif

#define MAX_UINT8				255
#ifndef UINT8_MAX
	#define UINT8_MAX				0xff
#endif
#define MAX_UINT16				65,535
#define MAX_UINT32				4,294,967,295
#define MAX_UINT64				18446744073709551615

#define MIN_CHAR				MIN_INT8
#define MAX_CHAR				MAX_INT8
#define MIN_SHORT				MIN_INT16
#define MAX_SHORT				MAX_INT16
#define MIN_INT				MIN_INT32
#define MAX_INT				MAX_INT32
#define MIN_LONG				MIN_INT32
#define MAX_LONG				MAX_INT32

#define MAX_BYTE				MAX_UINT8
#define MAX_WORD				MAX_UINT16
#define MAX_DWORD				MAX_UINT32
#define MAX_QWORD				MAX_UINT64

#define MAX_WCHAR				MAX_UINT16

#define MIN_FLOAT				3.4E-38
#define MAX_FLOAT				3.4E+38
#define MIN_DOUBLE				1.7E-308
#define MAX_DOUBLE				1.7E+308

#ifndef FLT_MAX
#define FLT_MAX	3.40282347E+38F
#endif

#ifndef FLT_MIN
#define FLT_MIN	1.17549435E-38F
#endif

#define INFINITE            0xFFFFFFFF  // Infinite timeout

#define MAKEFOURCC(ch0, ch1, ch2, ch3)                              \
                ((DWORD)(BYTE)(ch0) | ((DWORD)(BYTE)(ch1) << 8) |   \
                ((DWORD)(BYTE)(ch2) << 16) | ((DWORD)(BYTE)(ch3) << 24 ))

//
// Macros that are no longer used in this header but which clients may
// depend on being defined here.
//
#define LOWORD(_dw)     ((WORD)(((UINT_PTR)(_dw)) & 0xffff))
#define HIWORD(_dw)     ((WORD)((((UINT_PTR)(_dw)) >> 16) & 0xffff))
#define LODWORD(_qw)    ((DWORD)(_qw))
#define HIDWORD(_qw)    ((DWORD)(((_qw) >> 32) & 0xffffffff))

inline vBOOL _BitScanReverse(DWORD* Index, DWORD Mask)
{
	return vFALSE;
}

inline vBOOL _BitScanReverse64(DWORD* Index, UINT64 Mask)
{
	return vFALSE;
}

#ifndef _SYSTEMTIME_
#define _SYSTEMTIME_
typedef struct _SYSTEMTIME
{
	WORD wYear;
	WORD wMonth;
	WORD wDayOfWeek;
	WORD wDay;
	WORD wHour;
	WORD wMinute;
	WORD wSecond;
	WORD wMilliseconds;
} 	SYSTEMTIME;
#endif

struct RECT
{
	LONG    left;
	LONG    top;
	LONG    right;
	LONG    bottom;
};

#ifndef Sleep
#include <unistd.h>
#	define Sleep(millisecond) usleep(millisecond * 1000)
#endif

#ifndef PLATFORM_WIN
#pragma clang diagnostic pop
#endif

#endif //__vfxtypes_32_h__2003_11_12
