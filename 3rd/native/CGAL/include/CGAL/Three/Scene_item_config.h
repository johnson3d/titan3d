// Copyright (c) 2009-2011  GeometryFactory Sarl (France)
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Three/include/CGAL/Three/Scene_item_config.h $
// $Id: include/CGAL/Three/Scene_item_config.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Laurent RINEAU

#ifndef SCENE_ITEM_CONFIG_H
#define SCENE_ITEM_CONFIG_H

#include <CGAL/license/Three.h>


#include <QtCore/qglobal.h>

#ifdef demo_framework_EXPORTS
#  define scene_item_EXPORTS
#endif

#ifdef scene_item_EXPORTS
#  define SCENE_ITEM_EXPORT Q_DECL_EXPORT
#else
#  define SCENE_ITEM_EXPORT Q_DECL_IMPORT
#endif

#endif // SCENE_ITEM_CONFIG_H
