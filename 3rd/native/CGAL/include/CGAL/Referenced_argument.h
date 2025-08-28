// Copyright (c) 2009-2014 INRIA Sophia-Antipolis (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Kernel_d/include/CGAL/Referenced_argument.h $
// $Id: include/CGAL/Referenced_argument.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)    : Samuel Hornus, Olivier Devillers

#ifndef REFERENCED_ARGUMENT_H
#define REFERENCED_ARGUMENT_H

template< typename T>
struct Referenced_argument
{
    Referenced_argument() : arg_() {}
    template<typename A>
    Referenced_argument(A a) : arg_(a) {}
    template<typename A, typename B>
    Referenced_argument(A a, B b) : arg_(a, b) {}
    T & arg() { return arg_; }

protected:
    T arg_;
};

#endif // REFERENCED_ARGUMENT_H
