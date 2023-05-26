/*
A C++ program for Random-start randomly permuted Halton sequence.
Coded by Dr. Linlin Xu and Prof. Giray Okten.

Copyright (C) 2017, Linlin Xu and Giray Okten,
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

3. The names of its contributors may not be used to endorse or promote
products derived from this software without specific prior written
permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

References:
Ökten, G., Generalized von Neumann-Kakutani transformation and random-start
scrambled Halton sequences. Journal of Complexity, 2009,
Vol 25, No 4, 318--331.

Ökten, G., Shah, M. and Goncharov, Y., Random and deterministic digit
permutations of the Halton sequence. Monte Carlo and Quasi-Monte Carlo
Methods, Springer, 2010, 609-622.

Xu, L., & Ökten, G., High-performance financial Simulation Using
randomized quasi-Monte Carlo Methods. Quantitative Finance, 2015,
Vol 15, No 8, 1425-1436.

Any feedback is very welcome.
email: lxu@math.fsu.edu, okten@math.fsu.edu
*/

#include <cassert>
#include <cmath>
#include <cstring>
#include <algorithm>
#include "halton.h"


halton::halton(bool isMaster)
{
	isRandomStart = false;
	isRandomlyPermuted = false;
	isPowerInitialized = false;
	isMasterThread = isMaster;
	dim = 0;
	isPermutationReady = false;
	isBaseInitialized = false;
	pmt = MersenneTwister::Instance();
	ppm = nullptr;
}

void halton::set_power_buffer()
{
	for (size_t d = 0; d < dim; d++)
		for (uint8_t j = 0; j < WIDTH; j++)
		{
			if (j == 0)
				pwr[d][j] = base[d];
			else
				pwr[d][j] = pwr[d][j - 1] * base[d];
		}
	isPowerInitialized = true;
}

void halton::clear_buffer()
{
	for (auto &v : rnd)
		std::fill(begin(v), end(v), 0.0);
	for (auto &v : digit)
		std::fill(begin(v), end(v), 0);
}

void halton::init_expansion()
{
	size_t i;
	int8_t j;
	uint64_t n = 0;
	uint64_t d = 0;
	for (i = 0; i < dim; i++)
	{
		n = start[i] - 1;
		j = 0;
		while (n > 0)
		{
			digit[i][j] = n % base[i];
			n = n / base[i];
			j++;
		}
		j--;
		while (j >= 0)
		{
			d = digit[i][j];
			if (isRandomlyPermuted)
				d = permute(i, j);
			rnd[i][j] = rnd[i][j + 1] + d * 1.0 / pwr[i][j];
			j--;
		}
	}
}

void halton::genHalton()
{
	size_t i;
	int8_t j;
	uint64_t n = 0;
	uint64_t d = 0;

	for (i = 0; i < dim; i++)
	{
		j = 0;
		while (digit[i][j] + 1 >= base[i])
			j++;
		digit[i][j]++;
		d = digit[i][j];
		if (isRandomlyPermuted)
			d = permute(i, j);
		rnd[i][j] = rnd[i][j + 1] + d * 1.0 / pwr[i][j];
		for (j = j - 1; j >= 0; j--)
		{
			digit[i][j] = 0;
			d = 0;
			if (isRandomlyPermuted)
				d = permute(i, j);
			rnd[i][j] = rnd[i][j + 1] + d * 1.0 / pwr[i][j];
		}
	}
}

uint64_t inline halton::permute(size_t i, uint8_t j)
{
	return *(*(ppm + i) + digit[i][j]);
}

void halton::set_permutation()
{
	if (ppm)
	{
		for (size_t i = 0; i < dim; i++)
		{
			delete[] * (ppm + i);
			*(ppm + i) = nullptr;
		}
		delete[] ppm;
		ppm = nullptr;
	}
	ppm = new uint64_t* [dim];

	uint64_t j, k, tmp;

	for (size_t i = 0; i < dim; i++)
	{
		*(ppm + i) = new uint64_t[base[i]];
		for (j = 0; j < base[i]; j++)
			*(*(ppm + i) + j) = j;

		for (j = 1; j < base[i]; j++)
		{
			tmp = (uint64_t) floor(pmt->genrand64_real3() * base[i]);
			if (tmp != 0)
			{
				k = *(*(ppm + i) + j);
				*(*(ppm + i) + j) = *(*(ppm + i) + tmp);
				*(*(ppm + i) + tmp) = k;
			}
		}
	}
	isPermutationReady = true;
}

void halton::get_prime()
{
	int64_t n = dim;
	uint64_t prime = 1;
	size_t m = 0;
	do
	{
		prime++;
		base[m++]= prime;
		n--;
		for (uint64_t i = 2; i <= sqrt(prime * 1.0); i++)
			if (prime % i == 0)
			{
				n++;
				m--;
				break;
			}
	} while (n > 0);
}

void halton::set_dim(size_t d)
{
	assert(d <= MAX_DIM);
	dim = d;
}

void halton::set_start()
{
	for (size_t i = 0; i < dim; i++)
	{
		if (isRandomStart)
			start[i] = rnd_start(pmt->genrand64_real3(), base[i]);
		else
			start[i] = 1;
		//printf("%ulld\n", start[i]);
	}
}

void halton::alter_start(size_t d, uint64_t rs)
{
	start[d - 1] = rs;
}

void halton::set_base()
{
	get_prime();
	isBaseInitialized = true;
}

double halton::get_rnd(size_t d)
{
	return rnd[d - 1][0];
}

uint64_t halton::rnd_start(double r, uint64_t base)
{
	uint64_t z = 0;
	uint64_t cnt = 0;
	uint64_t b = base;
	while (r > 1e-16) // Potential deal loop?
	{
		cnt = 0;
		if (r >= 1.0 / b)
		{
			cnt = (uint64_t) floor(r * b);
			r = r - cnt * 1.0 / b;
			z += cnt * b / base;
		}
		b *= base;
	}
	return z;
}

void halton::init(size_t dim, bool rs, bool rp)
{
	set_dim(dim);
	rnd.resize(dim, std::vector<double>(WIDTH));
	digit.resize(dim, std::vector<uint64_t>(WIDTH));
	pwr.resize(dim, std::vector<uint64_t>(WIDTH));
	start.resize(dim);
	base.resize(dim);
	isPermutationReady = false;
	if (isMasterThread && !isBaseInitialized)
		set_base();
	set_random_start_flag(rs);
	set_permute_flag(rp);
	configure();
}

void halton::configure()
{
	if (isMasterThread)
		set_start();
	if (isMasterThread && !isPowerInitialized)
		set_power_buffer();
	clear_buffer();
	if (isMasterThread && isRandomlyPermuted && !isPermutationReady)
		set_permutation();
	init_expansion();
}
