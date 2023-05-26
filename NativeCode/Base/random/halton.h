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
email: lxu@math.fsu.com, okten@math.fsu.edu
*/

#ifndef _HALTON_H
#define _HALTON_H

#include <climits>
#include <cstdint>
#include <vector>

#include "mt.h"

#define MAX_DIM 4096 // Support up to 4096 dimensions if memory permits
#define WIDTH 64

class halton
{
public:
	halton(bool isMaster = true);
	~halton()
	{
		if (isMasterThread && isRandomlyPermuted)
		{
			if (ppm)
			{
				for (size_t i = 0; i < dim; i++)
					delete[] * (ppm + i);
				delete[] ppm;
				ppm = nullptr;
			}
		}
	}
	void init(size_t dim, bool rs, bool rp);
	void configure();
	void init_expansion();
	void set_dim(size_t d);
	void set_base();
	void set_start();
	void alter_start(size_t d, uint64_t rs);
	void set_permutation();
	void set_permute_flag(bool rp) { isRandomlyPermuted = rp; }
	void set_random_start_flag(bool rs) { isRandomStart = rs; }
	void set_power_buffer();
	void clear_buffer();
	uint64_t rnd_start(double r, uint64_t base);

	void genHalton();

	inline uint64_t permute(size_t i, uint8_t j);
	uint64_t get_start(size_t d) { return start[d - 1]; }
	void get_prime();
	double get_rnd(size_t d);

private:
	size_t dim;
	std::vector<uint64_t> start;
	std::vector<uint64_t> base;
	std::vector<std::vector<double>> rnd;
	std::vector<std::vector<uint64_t>> digit;
	std::vector<std::vector<uint64_t>> pwr;
	uint64_t **ppm;
	MersenneTwister *pmt; //Pseudorandom number generator handler
	bool isRandomlyPermuted;
	bool isRandomStart;
	bool isPowerInitialized;
	bool isBaseInitialized;
	bool isMasterThread;
	bool isPermutationReady;
};

#endif
