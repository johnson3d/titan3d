///////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2002, Industrial Light & Magic, a division of Lucas
// Digital Ltd. LLC
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// *       Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
// *       Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
// *       Neither the name of Industrial Light & Magic nor the names of
// its contributors may be used to endorse or promote products derived
// from this software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
///////////////////////////////////////////////////////////////////////////

// Primary authors:
//     Florian Kainz <kainz@ilm.com>
//     Rod Bogart <rgb@ilm.com>

//---------------------------------------------------------------------------
//
//	float16 -- a 16-bit floating point number class:
//
//	Type float16 can represent positive and negative numbers, whose
//	magnitude is between roughly 6.1e-5 and 6.5e+4, with a relative
//	error of 9.8e-4; numbers smaller than 6.1e-5 can be represented
//	with an absolute error of 6.0e-8.  All integers from -2048 to
//	+2048 can be represented exactly.
//
//	Type float16 behaves (almost) like the built-in C++ floating point
//	types.  In arithmetic expressions, float16, float and double can be
//	mixed freely.  Here are a few examples:
//
//	    float16 a (3.5);
//	    float b (a + sqrt (a));
//	    a += b;
//	    b += a;
//	    b = a + 7;
//
//	Conversions from float16 to float are lossless; all float16 numbers
//	are exactly representable as floats.
//
//	Conversions from float to float16 may not preserve the float's
//	value exactly.  If a float is not representable as a float16, the
//	float value is rounded to the nearest representable float16.  If
//	a float value is exactly in the middle between the two closest
//	representable float16 values, then the float value is rounded to
//	the float16 with the greater magnitude.
//
//	Overflows during float-to-float16 conversions cause arithmetic
//	exceptions.  An overflow occurs when the float value to be
//	converted is too large to be represented as a float16, or if the
//	float value is an infinity or a NAN.
//
//	The implementation of type float16 makes the following assumptions
//	about the implementation of the built-in C++ types:
//
//	    float is an IEEE 754 single-precision number
//	    sizeof (float) == 4
//	    sizeof (unsigned int) == sizeof (float)
//	    alignof (unsigned int) == alignof (float)
//	    sizeof (unsigned short) == 2
//
//---------------------------------------------------------------------------

#pragma once

#include "../BaseHead.h"

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-register"
#endif

class float16
{
public:

	//-------------
	// Constructors
	//-------------

	float16 ();			// no initialization
	float16 (float f);


	//--------------------
	// Conversion to float
	//--------------------

	operator		float () const;


	//------------
	// Unary minus
	//------------

	float16		operator - () const;


	//-----------
	// Assignment
	//-----------

	float16 &		operator = (float16  h);
	float16 &		operator = (float f);

	float16 &		operator += (float16  h);
	float16 &		operator += (float f);

	float16 &		operator -= (float16  h);
	float16 &		operator -= (float f);

	float16 &		operator *= (float16  h);
	float16 &		operator *= (float f);

	float16 &		operator /= (float16  h);
	float16 &		operator /= (float f);


	//---------------------------------------------------------
	// Round to n-bit precision (n should be between 0 and 10).
	// After rounding, the significand's 10-n least significant
	// bits will be zero.
	//---------------------------------------------------------

	float16		round (unsigned int n) const;


	//--------------------------------------------------------------------
	// Classification:
	//
	//	h.isFinite()		returns true if h is a normalized number,
	//				a denormalized number or zero
	//
	//	h.isNormalized()	returns true if h is a normalized number
	//
	//	h.isDenormalized()	returns true if h is a denormalized number
	//
	//	h.isZero()		returns true if h is zero
	//
	//	h.isNan()		returns true if h is a NAN
	//
	//	h.isInfinity()		returns true if h is a positive
	//				or a negative infinity
	//
	//	h.isNegative()		returns true if the sign bit of h
	//				is set (negative)
	//--------------------------------------------------------------------

	bool		isFinite () const;
	bool		isNormalized () const;
	bool		isDenormalized () const;
	bool		isZero () const;
	bool		isNan () const;
	bool		isInfinity () const;
	bool		isNegative () const;


	//--------------------------------------------
	// Special values
	//
	//	posInf()	returns +infinity
	//
	//	negInf()	returns +infinity
	//
	//	qNan()		returns a NAN with the bit
	//			pattern 0111111111111111
	//
	//	sNan()		returns a NAN with the bit
	//			pattern 0111110111111111
	//--------------------------------------------

	static float16		posInf ();
	static float16		negInf ();
	static float16		qNan ();
	static float16		sNan ();


	//--------------------------------------
	// Access to the internal representation
	//--------------------------------------

	unsigned short	bits () const;
	void		setBits (unsigned short bits);


public:

	union uif
	{
		unsigned int	i;
		float		f;
	};

private:

	static short	convert (int i);
	static float	overflow ();

	unsigned short	_h;

	//---------------------------------------------------
	// Windows dynamic libraries don't like static
	// member variables.
	//---------------------------------------------------
	 static const uif	        _toFloat[1 << 16];
	 static const unsigned short _eLut[1 << 9];

};

//-----------
// Stream I/O
//-----------

//std::ostream &		operator << (std::ostream &os, float16  h);
//std::istream &		operator >> (std::istream &is, float16 &h);
//----------
// Debugging
//----------

//void			printBits   (std::ostream &os, float16  h);
//void			printBits   (std::ostream &os, float f);
//void			printBits   (char  c[19], float16  h);
//void			printBits   (char  c[35], float f);


//-------------------------------------------------------------------------
// Limits
//
// Visual C++ will complain if FLOAT16_MIN, FLOAT16_NRM_MIN etc. are not float
// constants, but at least one other compiler (gcc 2.96) produces incorrect
// results if they are.
//-------------------------------------------------------------------------

#define PLATFORM_WINDOWS

#if defined(PLATFORM_WINDOWS)

#define FLOAT16_MIN	5.96046448e-08f	// Smallest positive float16

#define FLOAT16_NRM_MIN	6.10351562e-05f	// Smallest positive normalized float16

#define FLOAT16_MAX	65504.0f	// Largest positive float16

#define HALF_EPSILON	0.00097656f	// Smallest positive e for which
// float16 (1.0 + e) != float16 (1.0)
#else

#define FLOAT16_MIN	5.96046448e-08f	// Smallest positive float16

#define FLOAT16_NRM_MIN	6.10351562e-05f	// Smallest positive normalized float16

#define FLOAT16_MAX	65504.0f		// Largest positive float16

#define HALF_EPSILON	0.00097656f	// Smallest positive e for which
// float16 (1.0 + e) != float16 (1.0)
#endif


#define HALF_MANT_DIG	11		// Number of digits in mantissa
// (significand + hidden leading 1)

#define FLOAT16_DIG	2		// Number of base 10 digits that
// can be represented without change

#define FLOAT16_RADIX	2		// Base of the exponent

#define FLOAT16_MIN_EXP	-13		// Minimum negative integer such that
// FLOAT16_RADIX raised to the power of
// one less than that integer is a
// normalized float16

#define HALF_MAX_EXP	16		// Maximum positive integer such that
// FLOAT16_RADIX raised to the power of
// one less than that integer is a
// normalized float16

#define FLOAT16_MIN_10_EXP	-4		// Minimum positive integer such
// that 10 raised to that power is
// a normalized float16

#define FLOAT16_MAX_10_EXP	4		// Maximum positive integer such
// that 10 raised to that power is
// a normalized float16


//---------------------------------------------------------------------------
//
// Implementation --
//
// Representation of a float:
//
//	We assume that a float, f, is an IEEE 754 single-precision
//	floating point number, whose bits are arranged as follows:
//
//	    31 (msb)
//	    | 
//	    | 30     23
//	    | |      | 
//	    | |      | 22                    0 (lsb)
//	    | |      | |                     |
//	    X XXXXXXXX XXXXXXXXXXXXXXXXXXXXXXX
//
//	    s e        m
//
//	S is the sign-bit, e is the exponent and m is the significand.
//
//	If e is between 1 and 254, f is a normalized number:
//
//	            s    e-127
//	    f = (-1)  * 2      * 1.m
//
//	If e is 0, and m is not zero, f is a denormalized number:
//
//	            s    -126
//	    f = (-1)  * 2      * 0.m
//
//	If e and m are both zero, f is zero:
//
//	    f = 0.0
//
//	If e is 255, f is an "infinity" or "not a number" (NAN),
//	depending on whether m is zero or not.
//
//	Examples:
//
//	    0 00000000 00000000000000000000000 = 0.0
//	    0 01111110 00000000000000000000000 = 0.5
//	    0 01111111 00000000000000000000000 = 1.0
//	    0 10000000 00000000000000000000000 = 2.0
//	    0 10000000 10000000000000000000000 = 3.0
//	    1 10000101 11110000010000000000000 = -124.0625
//	    0 11111111 00000000000000000000000 = +infinity
//	    1 11111111 00000000000000000000000 = -infinity
//	    0 11111111 10000000000000000000000 = NAN
//	    1 11111111 11111111111111111111111 = NAN
//
// Representation of a float16:
//
//	Here is the bit-layout for a float16 number, h:
//
//	    15 (msb)
//	    | 
//	    | 14  10
//	    | |   |
//	    | |   | 9        0 (lsb)
//	    | |   | |        |
//	    X XXXXX XXXXXXXXXX
//
//	    s e     m
//
//	S is the sign-bit, e is the exponent and m is the significand.
//
//	If e is between 1 and 30, h is a normalized number:
//
//	            s    e-15
//	    h = (-1)  * 2     * 1.m
//
//	If e is 0, and m is not zero, h is a denormalized number:
//
//	            S    -14
//	    h = (-1)  * 2     * 0.m
//
//	If e and m are both zero, h is zero:
//
//	    h = 0.0
//
//	If e is 31, h is an "infinity" or "not a number" (NAN),
//	depending on whether m is zero or not.
//
//	Examples:
//
//	    0 00000 0000000000 = 0.0
//	    0 01110 0000000000 = 0.5
//	    0 01111 0000000000 = 1.0
//	    0 10000 0000000000 = 2.0
//	    0 10000 1000000000 = 3.0
//	    1 10101 1111000001 = -124.0625
//	    0 11111 0000000000 = +infinity
//	    1 11111 0000000000 = -infinity
//	    0 11111 1000000000 = NAN
//	    1 11111 1111111111 = NAN
//
// Conversion:
//
//	Converting from a float to a float16 requires some non-trivial bit
//	manipulations.  In some cases, this makes conversion relatively
//	slow, but the most common case is accelerated via table lookups.
//
//	Converting back from a float16 to a float is easier because we don't
//	have to do any rounding.  In addition, there are only 65536
//	different float16 numbers; we can convert each of those numbers once
//	and store the results in a table.  Later, all conversions can be
//	done using only simple table lookups.
//
//---------------------------------------------------------------------------


//--------------------
// Simple constructors
//--------------------

inline float16::float16 ()
{
	// no initialization
}


//----------------------------
// Half-from-float constructor
//----------------------------

inline float16::float16 (float f)
{
	if (f == 0)
	{
		//
		// Common special case - zero.
		// For speed, we don't preserve the zero's sign.
		//

		_h = 0;
	}
	else
	{
		//
		// We extract the combined sign and exponent, e, from our
		// floating-point number, f.  Then we convert e to the sign
		// and exponent of the float16 number via a table lookup.
		//
		// For the most common case, where a normalized float16 is produced,
		// the table lookup returns a non-zero value; in this case, all
		// we have to do, is round f's significand to 10 bits and combine
		// the result with e.
		//
		// For all other cases (overflow, zeroes, denormalized numbers
		// resulting from underflow, infinities and NANs), the table
		// lookup returns zero, and we call a longer, non-inline function
		// to do the float-to-float16 conversion.
		//

		uif x;

		x.f = f;

		//register int e = (x.i >> 23) & 0x000001ff;
		int e = (x.i >> 23) & 0x000001ff;

		e = _eLut[e];

		if (e)
		{
			//
			// Simple case - round the significand and
			// combine it with the sign and exponent.
			//

			_h = e + (((x.i & 0x007fffff) + 0x00001000) >> 13);
		}
		else
		{
			//
			// Difficult case - call a function.
			//

			_h = convert (x.i);
		}
	}
}


//------------------------------------------
// Half-to-float conversion via table lookup
//------------------------------------------

inline float16::operator float () const
{
	return _toFloat[_h].f;
}


//-------------------------
// Round to n-bit precision
//-------------------------

inline float16 float16::round (unsigned int n) const
{
	//
	// Parameter check.
	//

	if (n >= 10)
		return *this;

	//
	// Disassemble h into the sign, s,
	// and the combined exponent and significand, e.
	//

	unsigned short s = _h & 0x8000;
	unsigned short e = _h & 0x7fff;

	//
	// Round the exponent and significand to the nearest value
	// where ones occur only in the (10-n) most significant bits.
	// Note that the exponent adjusts automatically if rounding
	// up causes the significand to overflow.
	//

	e >>= 9 - n;
	e  += e & 1;
	e <<= 9 - n;

	//
	// Check for exponent overflow.
	//

	if (e >= 0x7c00)
	{
		//
		// Overflow occurred -- truncate instead of rounding.
		//

		e = _h;
		e >>= 10 - n;
		e <<= 10 - n;
	}

	//
	// Put the original sign bit back.
	//

	float16 h;
	h._h = s | e;

	return h;
}


//-----------------------
// Other inline functions
//-----------------------

inline float16	float16::operator - () const
{
	float16 h;
	h._h = _h ^ 0x8000;
	return h;
}


inline float16& float16::operator = (float16 h)
{
	_h = h._h;
	return *this;
}


inline float16& float16::operator = (float f)
{
	*this = float16 (f);
	return *this;
}


inline float16 &
float16::operator += (float16 h)
{
	*this = float16 (float (*this) + float (h));
	return *this;
}


inline float16& float16::operator += (float f)
{
	*this = float16 (float (*this) + f);
	return *this;
}


inline float16& float16::operator -= (float16 h)
{
	*this = float16 (float (*this) - float (h));
	return *this;
}


inline float16& float16::operator -= (float f)
{
	*this = float16 (float (*this) - f);
	return *this;
}


inline float16& float16::operator *= (float16 h)
{
	*this = float16 (float (*this) * float (h));
	return *this;
}


inline float16 &
float16::operator *= (float f)
{
	*this = float16 (float (*this) * f);
	return *this;
}


inline float16& float16::operator /= (float16 h)
{
	*this = float16 (float (*this) / float (h));
	return *this;
}


inline float16& float16::operator /= (float f)
{
	*this = float16 (float (*this) / f);
	return *this;
}


inline bool float16::isFinite () const
{
	unsigned short e = (_h >> 10) & 0x001f;
	return e < 31;
}


inline bool float16::isNormalized () const
{
	unsigned short e = (_h >> 10) & 0x001f;
	return e > 0 && e < 31;
}


inline bool float16::isDenormalized () const
{
	unsigned short e = (_h >> 10) & 0x001f;
	unsigned short m =  _h & 0x3ff;
	return e == 0 && m != 0;
}


inline bool float16::isZero () const
{
	return (_h & 0x7fff) == 0;
}


inline bool float16::isNan () const
{
	unsigned short e = (_h >> 10) & 0x001f;
	unsigned short m =  _h & 0x3ff;
	return e == 31 && m != 0;
}


inline bool float16::isInfinity () const
{
	unsigned short e = (_h >> 10) & 0x001f;
	unsigned short m =  _h & 0x3ff;
	return e == 31 && m == 0;
}


inline bool float16::isNegative () const
{
	return (_h & 0x8000) != 0;
}


inline float16 float16::posInf ()
{
	float16 h;
	h._h = 0x7c00;
	return h;
}


inline float16 float16::negInf ()
{
	float16 h;
	h._h = 0xfc00;
	return h;
}


inline float16 float16::qNan ()
{
	float16 h;
	h._h = 0x7fff;
	return h;
}


inline float16 float16::sNan ()
{
	float16 h;
	h._h = 0x7dff;
	return h;
}


inline unsigned short float16::bits () const
{
	return _h;
}


inline void float16::setBits (unsigned short bits)
{
	_h = bits;
}



//template <class T> class float16Function
//{
//public:
//
//	//------------
//	// Constructor
//	//------------
//
//	template <class Function>
//	float16Function (Function f, float16 domainMin = -FLOAT16_MAX, float16 domainMax =  FLOAT16_MAX, T defaultValue = 0, T posInfValue  = 0, T negInfValue  = 0, T nanValue     = 0);
//
//	//-----------
//	// Evaluation
//	//-----------
//
//	T		operator () (float16 x) const;
//
//private:
//
//	T		_lut[1 << 16];
//};


//namespace std 
//{
//
//	template <>
//	class numeric_limits <float16>
//	{
//	public:
//
//		static const bool is_specialized = true;
//#undef min
//#undef max
//		static float16 min () throw () {return FLOAT16_NRM_MIN;}
//		static float16 max () throw () {return FLOAT16_MAX;}
//
//		static const int digits = HALF_MANT_DIG;
//		static const int digits10 = FLOAT16_DIG;
//		static const bool is_signed = true;
//		static const bool is_integer = false;
//		static const bool is_exact = false;
//		static const int radix = FLOAT16_RADIX;
//		static float16 epsilon () throw () {return HALF_EPSILON;}
//		static float16 round_error () throw () {return HALF_EPSILON / 2;}
//
//		static const int min_exponent = FLOAT16_MIN_EXP;
//		static const int min_exponent10 = FLOAT16_MIN_10_EXP;
//		static const int max_exponent = HALF_MAX_EXP;
//		static const int max_exponent10 = FLOAT16_MAX_10_EXP;
//
//		static const bool has_infinity = true;
//		static const bool has_quiet_NaN = true;
//		static const bool has_signaling_NaN = true;
//		static const float_denorm_style has_denorm = denorm_present;
//		static const bool has_denorm_loss = false;
//		static float16 infinity () throw () {return float16::posInf();}
//		static float16 quiet_NaN () throw () {return float16::qNan();}
//		static float16 signaling_NaN () throw () {return float16::sNan();}
//		static float16 denorm_min () throw () {return FLOAT16_MIN;}
//
//		static const bool is_iec559 = false;
//		static const bool is_bounded = false;
//		static const bool is_modulo = false;
//
//		static const bool traps = true;
//		static const bool tinyness_before = false;
//		static const float_round_style round_style = round_to_nearest;
//	};
//
//
//}; // namespace std
//
//
//
////---------------
//// Implementation
////---------------
//
//template <class T> template <class Function> 
//float16Function<T>::float16Function (Function f,  float16 domainMin, float16 domainMax,   T defaultValue, T posInfValue, T negInfValue,T nanValue)
//{
//	for (int i = 0; i < (1 << 16); i++)
//	{
//		float16 x;
//		x.setBits (i);
//
//		if (x.isNan())
//			_lut[i] = nanValue;
//		else if (x.isInfinity())
//			_lut[i] = x.isNegative()? negInfValue: posInfValue;
//		else if (x < domainMin || x > domainMax)
//			_lut[i] = defaultValue;
//		else
//			_lut[i] = f (x);
//	}
//}
//
//
//template <class T> inline T float16Function<T>::operator () (float16 x) const
//{
//	return _lut[x.bits()];
//}

//---------------------------------------------------------------------------
//
//	float16Function<T> -- a class for fast evaluation
//			   of float16 --> T functions
//
//	The constructor for a float16Function object,
//
//	    float16Function (function,
//			  domainMin, domainMax,
//			  defaultValue,
//			  posInfValue, negInfValue,
//			  nanValue);
//
//	evaluates the function for all finite float16 values in the interval
//	[domainMin, domainMax], and stores the results in a lookup table.
//	For finite float16 values that are not in [domainMin, domainMax], the
//	constructor stores defaultValue in the table.  For positive infinity,
//	negative infinity and NANs, posInfValue, negInfValue and nanValue
//	are stored in the table.
//
//	The tabulated function can then be evaluated quickly for arbitrary
//	float16 values by calling the the float16Function object's operator()
//	method.
//
//	Example:
//
//	    #include <math.h>
//	    #include <float16Function.h>
//
//	    float16Function<float16> hsin (sin);
//
//	    float16Function<float16> hsqrt (sqrt,		// function
//				      0, float16_MAX,	// domain
//				      float16::qNan(),	// sqrt(x) for x < 0
//				      float16::posInf(),	// sqrt(+inf)
//				      float16::qNan(),	// sqrt(-inf)
//				      float16::qNan());	// sqrt(nan)
//
//	    float16 x = hsin (1);
//	    float16 y = hsqrt (3.5);

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif