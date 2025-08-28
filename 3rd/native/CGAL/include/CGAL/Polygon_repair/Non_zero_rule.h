// Copyright (c) 2024 GeometryFactory.
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Polygon_repair/include/CGAL/Polygon_repair/Non_zero_rule.h $
// $Id: include/CGAL/Polygon_repair/Non_zero_rule.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Andreas Fabri

#ifndef CGAL_POLYGON_REPAIR_NON_ZERO_RULE_H
#define CGAL_POLYGON_REPAIR_NON_ZERO_RULE_H

#include <CGAL/license/Polygon_repair.h>

namespace CGAL {

namespace Polygon_repair {

/// \addtogroup PkgPolygonRepairRules
/// @{

/*!
  Tag class to select the non zero rule when calling `CGAL::Polygon_repair::repair()`.
  The non-zero rule results in areas with a non-zero winding number.
  */
  struct Non_zero_rule {};

///@}

} // namespace Polygon_repair

} // namespace CGAL

#endif  // CGAL_POLYGON_REPAIR_NON_ZERO_RULE_H
