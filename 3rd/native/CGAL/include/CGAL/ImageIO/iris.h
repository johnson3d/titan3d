// Copyright (c) 2005-2008 ASCLEPIOS Project, INRIA Sophia-Antipolis (France)
// All rights reserved.
//
// This file is part of the ImageIO Library, and as been adapted for CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/CGAL_ImageIO/include/CGAL/ImageIO/iris.h $
// $Id: include/CGAL/ImageIO/iris.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later
//
//
// Author(s)     :  ASCLEPIOS Project (INRIA Sophia-Antipolis), Laurent Rineau

#ifndef IRIS_H
#define IRIS_H

#include <CGAL/ImageIO.h>



PTRIMAGE_FORMAT createIrisFormat();
int readIrisImage( const char *name, _image *im );
int testIrisHeader(char *magic,const char *name);

#ifdef CGAL_HEADER_ONLY
#include <CGAL/ImageIO/iris_impl.h>
#endif // CGAL_HEADER_ONLY

#endif
