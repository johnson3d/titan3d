// Copyright (c) 2013 GeometryFactory (France). All rights reserved.
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Circular_kernel_3/include/CGAL/Circular_kernel_3/Intersection_traits.h $
// $Id: include/CGAL/Circular_kernel_3/Intersection_traits.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Philipp Möller and Sebastien Loriot

#ifndef CGAL_CIRCULAR_KERNEL_3_INTERSECTION_TRAITS_H
#define CGAL_CIRCULAR_KERNEL_3_INTERSECTION_TRAITS_H

#include <CGAL/license/Circular_kernel_3.h>


#include <CGAL/Intersection_traits.h>

#include <variant>
#include <utility>

namespace CGAL {

template <typename SK, typename T1, typename T2, typename T3=void*>
struct SK3_Intersection_traits
{};

// Intersection_traits for the circular kernel

// The additional CGAL_ADDITIONAL_VARIANT_FOR_ICL ( = int) in the variant
// has the only purpose to work around a bug of the Intel compiler,
// which without it produces the error
// /usr/include/boost/type_traits/has_nothrow_copy.hpp(36): internal error: bad pointer
// template struct has_nothrow_copy_constructor : public integral_constant{};
// See also https://github.com/CGAL/cgal/issues/1581

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Sphere_3, typename SK::Line_3>
{
  typedef std::variant<
            std::pair< typename SK::Circular_arc_point_3, unsigned int >
            CGAL_ADDITIONAL_VARIANT_FOR_ICL
          > type;
};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Line_3, typename SK::Sphere_3>
  : SK3_Intersection_traits<SK, typename SK::Sphere_3, typename SK::Line_3> {};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Circle_3, typename SK::Plane_3>
{
  typedef std::variant<
            std::pair< typename SK::Circular_arc_point_3, unsigned int >,
            typename SK::Circle_3
            CGAL_ADDITIONAL_VARIANT_FOR_ICL
          > type; };

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Plane_3, typename SK::Circle_3>
  : SK3_Intersection_traits<SK, typename SK::Circle_3, typename SK::Plane_3> {};


template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Circle_3, typename SK::Sphere_3>
{
  typedef std::variant<
            std::pair< typename SK::Circular_arc_point_3, unsigned int >,
            typename SK::Circle_3
            CGAL_ADDITIONAL_VARIANT_FOR_ICL
          > type;
};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Sphere_3, typename SK::Circle_3>
  : SK3_Intersection_traits<SK, typename SK::Circle_3, typename SK::Sphere_3> {};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Circle_3, typename SK::Circle_3>
{
  typedef std::variant<
            std::pair <typename SK::Circular_arc_point_3, unsigned int >,
            typename SK::Circle_3
            CGAL_ADDITIONAL_VARIANT_FOR_ICL
          > type;
};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Circle_3, typename SK::Line_3>
{
  typedef std::variant<
            std::pair <typename SK::Circular_arc_point_3, unsigned int >
            CGAL_ADDITIONAL_VARIANT_FOR_ICL
          > type;
};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Line_3, typename SK::Circle_3>
  : SK3_Intersection_traits<SK, typename SK::Circle_3, typename SK::Line_3> {};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Circular_arc_3, typename SK::Circular_arc_3>
{
  typedef std::variant<
            typename SK::Circle_3,
            std::pair <typename SK::Circular_arc_point_3, unsigned int >,
            typename SK::Circular_arc_3
            CGAL_ADDITIONAL_VARIANT_FOR_ICL
          > type;
};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Circular_arc_3, typename SK::Plane_3>
{
  typedef std::variant<
            std::pair <typename SK::Circular_arc_point_3, unsigned int >,
            typename SK::Circular_arc_3
            CGAL_ADDITIONAL_VARIANT_FOR_ICL
          > type;
};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Plane_3, typename SK::Circular_arc_3>
  : SK3_Intersection_traits<SK, typename SK::Circular_arc_3, typename SK::Plane_3> {};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Line_arc_3, typename SK::Line_arc_3>
{
  typedef std::variant<
            std::pair <typename SK::Circular_arc_point_3, unsigned int >,
            typename SK::Line_arc_3
            CGAL_ADDITIONAL_VARIANT_FOR_ICL
          > type;
};

//struct to factorize the following specializations
template <typename SK>
struct SK3_intersect_ternary
{
  typedef std::variant<
            typename SK::Circle_3,
            typename SK::Plane_3,
            typename SK::Sphere_3,
            std::pair< typename SK::Circular_arc_point_3, unsigned >,
            int
          > type;
};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Sphere_3, typename SK::Sphere_3, typename SK::Sphere_3>
  : SK3_intersect_ternary<SK> {};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Sphere_3, typename SK::Sphere_3, typename SK::Plane_3>
  : SK3_intersect_ternary<SK> {};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Plane_3, typename SK::Sphere_3, typename SK::Sphere_3>
  : SK3_intersect_ternary<SK> {};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Plane_3, typename SK::Plane_3, typename SK::Sphere_3>
  : SK3_intersect_ternary<SK> {};

template <typename SK>
struct SK3_Intersection_traits<SK, typename SK::Sphere_3, typename SK::Plane_3, typename SK::Plane_3>
  : SK3_intersect_ternary<SK> {};

} //end of namespace CGAL

#endif // CGAL_CIRCULAR_KERNEL_2_INTERSECTION_TRAITS_H
