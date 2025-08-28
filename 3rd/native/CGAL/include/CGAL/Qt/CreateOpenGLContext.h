// Copyright (c) 2015  GeometryFactory SARL (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/GraphicsView/include/CGAL/Qt/CreateOpenGLContext.h $
// $Id: include/CGAL/Qt/CreateOpenGLContext.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Laurent Rineau and Maxime Gimeno
#ifndef CGAL_QT_CREATE_OPENGL_CONTEXT_H
#define CGAL_QT_CREATE_OPENGL_CONTEXT_H

#include <CGAL/license/GraphicsView.h>


#include <QOpenGLContext>

namespace CGAL{
namespace Qt{
inline QOpenGLContext* createOpenGLContext()
{
    QOpenGLContext *context = new QOpenGLContext();
    QSurfaceFormat format;
    format.setVersion(2,1);
    format.setProfile(QSurfaceFormat::CompatibilityProfile);
    context->setFormat(format);
    context->create();
    return context;
}
} // namespace Qt
} // namespace CGAL
#endif
