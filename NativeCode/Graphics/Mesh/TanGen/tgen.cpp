/**
 * TGen - Simple Tangent Generator
 *
 * 2016 by Max Limper, Fraunhofer IGD
 *
 * This code is public domain.
 * 
 */

#include "tgen.h"
#include <cmath>


// local utility definitions
namespace
{
    const tgen::RealT DenomEps = 1e-10;

    //-------------------------------------------------------------------------

    inline void addVec3(const tgen::RealT * a,
                        const tgen::RealT * b,
                        tgen::RealT       * result)
    {
        result[0] = a[0] + b[0];
        result[1] = a[1] + b[1];
        result[2] = a[2] + b[2];
    }

    //-------------------------------------------------------------------------

    inline void subVec3(const tgen::RealT * a,
                        const tgen::RealT * b,
                        tgen::RealT       * result)
    {
        result[0] = a[0] - b[0];
        result[1] = a[1] - b[1];
        result[2] = a[2] - b[2];
    }
    
    //-------------------------------------------------------------------------

    inline void multVec3(const tgen::RealT * a,
                         const tgen::RealT   s,
                         tgen::RealT       * result)
    {
        result[0] = a[0] * s;
        result[1] = a[1] * s;
        result[2] = a[2] * s;
    }

    //-------------------------------------------------------------------------

    void normalizeVec3(tgen::RealT * v)
    {
        tgen::RealT len = std::sqrt(v[0]*v[0] + v[1]*v[1] + v[2]*v[2]);

        multVec3(v, 1.0 / len, v);
    }

    //-------------------------------------------------------------------------

    inline tgen::RealT dotProd(const tgen::RealT * a,
                               const tgen::RealT * b )
    {
        return a[0]*b[0] + a[1]*b[1] + a[2]*b[2];
    }

    //-------------------------------------------------------------------------

    inline void crossProd(const tgen::RealT * a,
                          const tgen::RealT * b,
                          tgen::RealT       * result)
    {
        result[0] = a[1] * b[2] - a[2] * b[1];
        result[1] = a[2] * b[0] - a[0] * b[2];
        result[2] = a[0] * b[1] - a[1] * b[0];
    }
    
    //-------------------------------------------------------------------------

    inline void subVec2(const tgen::RealT * a,
                        const tgen::RealT * b,
                        tgen::RealT       * result)
    {
        result[0] = a[0] - b[0];
        result[1] = a[1] - b[1];
    }

} //anonymous namespace


namespace tgen
{

    //-------------------------------------------------------------------------

    void computeCornerTSpace(const std::vector<VIndexT> & triIndicesPos,
                             const std::vector<VIndexT> & triIndicesUV,
                             const std::vector<RealT>   & positions3D,
                             const std::vector<RealT>   & uvs2D,
                             std::vector<RealT>         & cTangents3D,
                             std::vector<RealT>         & cBitangents3D)
    {
        const std::size_t numCorners = triIndicesPos.size();

        cTangents3D.resize(  numCorners * 3);
        cBitangents3D.resize(numCorners * 3);

        RealT edge3D[3][3], edgeUV[3][2],
              tmp0[3],      tmp1[3];

        for (std::size_t i = 0; i < triIndicesPos.size(); i += 3)
        {            
            const VIndexT vertexIndicesPos[3]  = { triIndicesPos[i  ],
                                                   triIndicesPos[i+1],
                                                   triIndicesPos[i+2]  };

            const VIndexT vertexIndicesUV[3]   = { triIndicesUV[i  ],
                                                   triIndicesUV[i+1],
                                                   triIndicesUV[i+2]  };
            
            // compute derivatives of positions and UVs along the edges
            for (std::size_t j = 0; j < 3; ++j)
            {                
                const std::size_t next = (j + 1) % 3;

                const VIndexT v0PosIdx = vertexIndicesPos[j];
                const VIndexT v1PosIdx = vertexIndicesPos[next];
                const VIndexT v0UVIdx  = vertexIndicesUV[j];
                const VIndexT v1UVIdx  = vertexIndicesUV[next];

                subVec3(&positions3D[v1PosIdx * 3],
                        &positions3D[v0PosIdx * 3],
                        edge3D[j]);

                subVec2(&uvs2D[v1UVIdx * 2],
                        &uvs2D[v0UVIdx * 2],
                        edgeUV[j]);
            }
            
            // compute per-corner tangent and bitangent (not normalized),
            // using the derivatives of the UVs
            // http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-13-normal-mapping/
            for (std::size_t j = 0; j < 3; ++j)
            {
                const std::size_t prev = (j + 2) % 3;

                const RealT * dPos0    = edge3D[j];
                const RealT * dPos1Neg = edge3D[prev];
                const RealT * dUV0     = edgeUV[j];
                const RealT * dUV1Neg  = edgeUV[prev];

                RealT * resultTangent   = &cTangents3D[  (i + j) * 3];
                RealT * resultBitangent = &cBitangents3D[(i + j) * 3];

                RealT denom = (dUV0[0] * -dUV1Neg[1] - dUV0[1] * -dUV1Neg[0]);
                RealT r     = std::abs(denom) > DenomEps ? 1.0 / denom : 0.0;

                multVec3(dPos0,    -dUV1Neg[1] * r, tmp0);
                multVec3(dPos1Neg, -dUV0[1]    * r, tmp1);
                subVec3(tmp0, tmp1, resultTangent);

                multVec3(dPos1Neg, -dUV0[0]    * r, tmp0);
                multVec3(dPos0,    -dUV1Neg[0] * r, tmp1);
                subVec3(tmp0, tmp1, resultBitangent);
            }
        }
    }

    //-------------------------------------------------------------------------

     void computeVertexTSpace(const std::vector<VIndexT> & triIndicesUV,
                              const std::vector<RealT>   & cTangents3D,
                              const std::vector<RealT>   & cBitangents3D,
                              std::size_t                  numUVVertices,
                              std::vector<RealT>         & vTangents3D,
                              std::vector<RealT>         & vBitangents3D )
     {
         vTangents3D.resize(  numUVVertices * 3, 0.0);
         vBitangents3D.resize(numUVVertices * 3, 0.0);


         // average tangent vectors for each "wedge" (UV vertex)
         // this assumes that we do not use different vertex positions
         // for the same UV coordinate (example: mirrored parts)

         for (std::size_t i = 0; i < triIndicesUV.size(); ++i)
         {
             const VIndexT uvIdx = triIndicesUV[i];

             RealT * cornerTangent   = &vTangents3D[  uvIdx*3];
             RealT * cornerBitangent = &vBitangents3D[uvIdx*3];

             addVec3(&cTangents3D[  i*3], cornerTangent,   cornerTangent  );
             addVec3(&cBitangents3D[i*3], cornerBitangent, cornerBitangent);
         }


         // normalize results

         for (VIndexT i = 0; i < numUVVertices; ++i)
         {
             normalizeVec3(&vTangents3D[  i * 3]);
             normalizeVec3(&vBitangents3D[i * 3]);
         }        
     }

     //-------------------------------------------------------------------------
     
     void orthogonalizeTSpace(const std::vector<RealT> & normals3D,
                              std::vector<RealT>       & tangents3D,
                              std::vector<RealT>       & bitangents3D)
     {
         const std::size_t numVertices = normals3D.size() / 3;

         RealT correction[3];
         for (VIndexT i = 0; i < numVertices; ++i)
         {
             const RealT * nV = &normals3D[   i*3];

             RealT * bV = &bitangents3D[i*3];
             RealT * tV = &tangents3D[i*3];
             
             RealT d = dotProd(nV, tV);

             multVec3(nV, d, correction);
             subVec3(tV, correction, tV);
             normalizeVec3(tV);

             crossProd(nV, tV, bV);
         }
     }

     //-------------------------------------------------------------------------

     void computeTangent4D(const std::vector<RealT> & normals3D,
                           const std::vector<RealT> & tangents3D,
                           const std::vector<RealT> & bitangents3D,
                           std::vector<RealT>       & tangents4D)
     {
         const std::size_t numVertices = normals3D.size() / 3;

         tangents4D.resize(numVertices * 4);

         RealT cross[3];
         for (VIndexT i = 0; i < numVertices; ++i)
         {
             crossProd(&normals3D[i*3], &tangents3D[i*3], cross);

             RealT sign = dotProd(cross, &bitangents3D[i*3]) > 0.0 ? 1.0 : -1.0;

             tangents4D[i*4  ] = tangents3D[i*3+0];
             tangents4D[i*4+1] = tangents3D[i*3+1];
             tangents4D[i*4+2] = tangents3D[i*3+2];
             tangents4D[i*4+3] = sign;
         }
     }

     //-------------------------------------------------------------------------

} //namespace tgen
