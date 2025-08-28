// Copyright (c) 2016 GeometryFactory (France)
//  All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Installation/include/CGAL/mutex.h $
// $Id: include/CGAL/mutex.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial

#ifndef CGAL_MUTEX_H
#define CGAL_MUTEX_H

#include <CGAL/config.h>

#ifdef CGAL_HAS_THREADS
#include <mutex>
#define CGAL_MUTEX std::mutex
#define CGAL_SCOPED_LOCK(M) std::unique_lock<std::mutex> scoped_lock(M)
#endif
#endif // CGAL_MUTEX_H
