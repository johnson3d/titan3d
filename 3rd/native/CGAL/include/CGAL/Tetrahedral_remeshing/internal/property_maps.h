// Copyright (c) 2020 GeometryFactory (France) and Telecom Paris (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Tetrahedral_remeshing/include/CGAL/Tetrahedral_remeshing/internal/property_maps.h $
// $Id: include/CGAL/Tetrahedral_remeshing/internal/property_maps.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Jane Tournois

#ifndef CGAL_TETRAHEDRAL_REMESHING_PROPERTY_MAPS_H
#define CGAL_TETRAHEDRAL_REMESHING_PROPERTY_MAPS_H

#include <CGAL/license/Tetrahedral_remeshing.h>

#include <CGAL/property_map.h>


namespace CGAL
{
namespace Tetrahedral_remeshing
{
namespace internal
{

template<typename Tr>
struct All_cells_selected
{
  using key_type = typename Tr::Cell_handle;
  using value_type = bool;
  using reference = bool;
  using category = boost::read_write_property_map_tag;

  friend value_type get(const All_cells_selected&, const key_type& c)
  {
    using SI = typename Tr::Cell::Subdomain_index;
    return c->subdomain_index() != SI();
  }
  friend void put(All_cells_selected&, const key_type&, const value_type)
  {} //nothing to do : subdomain indices are updated in remeshing};
};

} // end namespace internal
} // end namespace Tetrahedral_remeshing
} // end namespace CGAL

#endif // CGAL_TETRAHEDRAL_REMESHING_PROPERTY_MAPS_H
