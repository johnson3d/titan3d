// Copyright (c) 2010-2011 CNRS and LIRIS' Establishments (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Combinatorial_map/include/CGAL/Info_for_cell_attribute.h $
// $Id: include/CGAL/Info_for_cell_attribute.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Guillaume Damiand <guillaume.damiand@liris.cnrs.fr>
//
#ifndef CGAL_INFO_FOR_CELL_ATTRIBUTE_H
#define CGAL_INFO_FOR_CELL_ATTRIBUTE_H 1

namespace CGAL {

  /// Info associated with a cell_attribute.
  template <typename Info>
  class Info_for_cell_attribute
  {
  public:
    /// Constructor without parameter.
    Info_for_cell_attribute()=default; // default => zero-initializing built-in types

    /// Constructor with an info in parameter.
    Info_for_cell_attribute(const Info& ainfo) : minfo(ainfo)
    {}

    /// Get the info associated with the cell_attribute.
    Info& info()
    { return minfo; }

    /// Get the info associated with the cell_attribute.
    const Info& info() const
    { return minfo; }

  protected:
    /// The info associated with the cell_attribute.
    Info minfo;
  };

} // namespace CGAL

#endif // CGAL_INFO_FOR_CELL_ATTRIBUTE_H //
// EOF //
