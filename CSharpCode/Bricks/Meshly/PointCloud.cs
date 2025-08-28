using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Meshly
{
    public class TtPointCloud
    {
        public unsafe static bool BuildTetrahedron(Vector3[] points, List<FTetrahedron> Tetras)
        {
            fixed (Vector3* pPoints = &points[0])
            {
                Support.TtBlobObject outObj = new Support.TtBlobObject();
                if (VPointCloud.BuildTetrahedron(pPoints, points.Length, outObj.mCoreObject))
                {
                    using (IO.TtMemReader reader = IO.TtMemReader.CreateInstance((byte*)outObj.DataPointer, outObj.Size))
                    {
                        int num = 0;
                        reader.Read(out num);
                        for (int i = 0; i<num; i++)
                        {
                            FTetrahedron tetra = new FTetrahedron();
                            reader.Read(out tetra);
                            Tetras.Add(tetra);
                        }
                    }
                    return true;
                }
                return false;
            }
        }
    }
}
