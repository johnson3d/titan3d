using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.Animation.Pose;
using EngineNS.GamePlay.Component;

namespace EngineNS.Bricks.Animation.Skeleton
{
    public interface ISocketable
    {
        string SocketName { get; set; }
        IScocket ParentSocket { get; set; }
    }
    public delegate void MeshSpaceMatrixChange(Matrix matrix);
    public interface IScocket
    {
        Matrix MeshSpaceMatrix { get; }
        event MeshSpaceMatrixChange OnMeshSpaceMatrixChange;
    }
    public class GSocketPlacement : GamePlay.Component.GPlacementComponent
    {
        public Matrix SocketCharacterSpaceMatrix = Matrix.Identity;
        public override Matrix WorldMatrix
        {
            get
            {
                if (mHostContainer == null)
                    return Transform;

                if (mHostContainer == Host)
                {
                    if (Host.Parent == null || Host.Parent.Placement == null)
                        return Transform;
                    else
                    {
                        return Transform * ParentWorldMatrix;
                    }
                }
                else
                {
                    IPlaceable host = null;
                    var comp = mHostContainer as GComponent;
                    host = comp.HostContainer as IPlaceable;
                    if (host == null || host.Placement == null)
                        return Transform* SocketCharacterSpaceMatrix;
                    return Transform * SocketCharacterSpaceMatrix* ParentWorldMatrix;
                }
            }
        }
    }
}
