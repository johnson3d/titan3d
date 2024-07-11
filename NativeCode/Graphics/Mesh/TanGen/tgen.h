/**
 * TGen - Simple Tangent Generator
 *
 * 2016 by Max Limper, Fraunhofer IGD
 *
 * This code is public domain.
 * 
 */

#ifndef TGEN_H
#define TGEN_H

#include <cstdint>
#include <vector>


namespace tgen
{
    
    //-------------------------------------------------------------------------

    typedef std::size_t VIndexT;
    typedef double      RealT;
    
    //-------------------------------------------------------------------------

    /**
     * Computes tangents and bitangents for each corner of a triangle.
     * In an indexed triangle list, each entry corresponds to one corner.
     *
     * Requirements for input:
     * - triIndicesPos and triIndicesUV must be of the same size
     * - triIndicesPos refers to (at maximum) num3DVertices different elements
     * - triIndicesUV  refers to (at maximum) numUVVertices different elements
     * - positions3D must have a size of num3DVertices*3
     * - uvs2D       must have a size of numUVVertices*2
     *
     * Output:
     * - cTangents3D   has numTriIndices*3 entries, contains per-corner tangents
     * - cBitangents3D has numTriIndices*3 entries, contains per-corner bitangents
     */
    void computeCornerTSpace(const std::vector<VIndexT> & triIndicesPos,
                             const std::vector<VIndexT> & triIndicesUV,
                             const std::vector<RealT>   & positions3D,
                             const std::vector<RealT>   & uvs2D,
                             std::vector<RealT>         & cTangents3D,
                             std::vector<RealT>         & cBitangents3D);

    //-------------------------------------------------------------------------

    /**
     * Computes per-vertex tangents and bitangents, for each UV vertex.
     * This is done by averaging vectors across each wedge (all vertex instances
     * sharing a common UV vertex).
     *
     * The basic method used here currently makes the assumption that UV
     * vertices are not being re-used across multiple 3D vertices.
     * However, the multi-indexed structure used here allows a single 3D vertex
     * to be split in UV space (to enable usage of UV charts without explicitly
     * cutting / splitting the 3D mesh).
     *
     * Requirements about input:
     * - triIndicesUV refers to (at maximum) numUVVertices different elements
     * - cTangents3D   has numTriIndices*3 entries, contains per-corner tangents
     * - cBitangents3D has numTriIndices*3 entries, contains per-corner bitangents     
     *
     * Output:
     * - vTangents3D   has numUVVertices*3 entries
     * - vBitangents3D has numUVVertices*3 entries
     */
    void computeVertexTSpace(const std::vector<VIndexT> & triIndicesUV,
                             const std::vector<RealT>   & cTangents3D,
                             const std::vector<RealT>   & cBitangents3D,
                             std::size_t                  numUVVertices,
                             std::vector<RealT>         & vTangents3D,
                             std::vector<RealT>         & vBitangents3D);

    //-------------------------------------------------------------------------

    /**
     * Makes the given tangent frames orthogonal.
     *
     * Input arrays must have the same number of (numUVVertices*3) elements.
     */
    void orthogonalizeTSpace(const std::vector<RealT> & normals3D,
                             std::vector<RealT>       & tangents3D,
                             std::vector<RealT>       & bitangents3D);

    //-------------------------------------------------------------------------

    /**
     * Makes the given tangent frames orthogonal.
     *
     * Input arrays must have the same number of (numUVVertices*3) elements.
     *
     * The output will be an array with 4-component versions of the tangents,
     * where the first three components are equivalent to the input tangents
     * and the fourth component contains a factor for flipping a computed
     * bitangent, if the original tangent frame was right-handed.
     * Concretely speaking, the 3D bitangent can be obtained as:
     *   bitangent = tangent4.w * (normal.cross(tangent4.xyz))
     */
    void computeTangent4D(const std::vector<RealT> & normals3D,
                          const std::vector<RealT> & tangents3D,
                          const std::vector<RealT> & bitangents3D,
                          std::vector<RealT>       & tangents4D);
    
    //-------------------------------------------------------------------------

}

#endif //TGEN_H
