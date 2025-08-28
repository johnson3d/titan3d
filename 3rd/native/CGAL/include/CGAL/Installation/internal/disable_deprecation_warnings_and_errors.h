// Copyright (c) 2018 GeometryFactory (France). All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Installation/include/CGAL/Installation/internal/disable_deprecation_warnings_and_errors.h $
// $Id: include/CGAL/Installation/internal/disable_deprecation_warnings_and_errors.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
// Author: Mael Rouxel-Labbé

// Some tests are explicitly used to check the sanity of deprecated code and should not
// give warnings/errors on platforms that defined CGAL_NO_DEPRECATED_CODE CGAL-wide
// (or did not disable deprecation warnings).

#if !defined(CGAL_NO_DEPRECATION_WARNINGS)
  #define CGAL_NO_DEPRECATION_WARNINGS
#endif

#if defined(CGAL_NO_DEPRECATED_CODE)
  #undef CGAL_NO_DEPRECATED_CODE
#endif
