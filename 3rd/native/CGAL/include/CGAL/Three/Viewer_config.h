// Copyright (c) 2012-2015  GeometryFactory Sarl (France)
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Three/include/CGAL/Three/Viewer_config.h $
// $Id: include/CGAL/Three/Viewer_config.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Laurent RINEAU, Maxime Gimeno

#ifndef VIEWER_CONFIG_H
#define VIEWER_CONFIG_H

#include <CGAL/license/Three.h>


#include <QtCore/qglobal.h>

#ifdef demo_framework_EXPORTS
#  define viewer_EXPORTS
#endif

#ifdef viewer_EXPORTS
#  define VIEWER_EXPORT Q_DECL_EXPORT
#else
#  define VIEWER_EXPORT Q_DECL_IMPORT
#endif

#endif // VIEWER_CONFIG_H
