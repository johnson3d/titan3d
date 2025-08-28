// Copyright (c) 2009,2010,2012,2015  GeometryFactory Sarl (France)
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Three/include/CGAL/Three/Scene_zoomable_item_interface.h $
// $Id: include/CGAL/Three/Scene_zoomable_item_interface.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Maxime GIMENO

#ifndef SCENE_ZOOMABLE_ITEM_INTERFACE_H
#define SCENE_ZOOMABLE_ITEM_INTERFACE_H

#include <CGAL/license/Three.h>
#include <QtPlugin>
#include <QPoint>
namespace CGAL
{
namespace Three {
  class Viewer_interface;


//! This class provides a function to move the camera orthogonaly to the wanted region
class Scene_zoomable_item_interface {
public:
  virtual ~Scene_zoomable_item_interface(){}
 //! Move the camera orthogonaly to the region defined by `point`
 virtual void zoomToPosition(const QPoint& point, CGAL::Three::Viewer_interface*)const = 0;
};
}
}

Q_DECLARE_INTERFACE(CGAL::Three::Scene_zoomable_item_interface, "com.geometryfactory.CGALLab.ZoomInterface/1.0")
#endif // SCENE_ZOOMABLE_ITEM_INTERFACE_H
