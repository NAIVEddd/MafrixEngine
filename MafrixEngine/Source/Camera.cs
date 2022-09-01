using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace MafrixEngine.Cameras
{
    using Vec3 = Vector3D<float>;
    using Mat4 = Matrix4X4<float>;

    public class CameraCoordinate
    {
        public Vec3 position;
        public Vec3 lookDir;
        public Vec3 up;
        public CameraCoordinate(Vec3 pos, Vec3 dir, Vec3 up)
        {
            position = pos;
            lookDir = Vector3D.Normalize(dir);
            this.up = Vector3D.Normalize(up);
        }
    }

    public class ProjectInfo
    {
        public float viewRadians;
        public float ratio;
        public float near;
        public float far;
        public ProjectInfo(float viewDegrees, float ratio, float far = 20000f, float near = 1f)
        {
            viewRadians = Scalar.DegreesToRadians(viewDegrees);
            this.ratio = ratio;
            this.near = near;
            this.far = far;
        }
    }

    public class Camera
    {
        private CameraCoordinate cameraCoordinate;
        private ProjectInfo projectInfo;

        public Camera(CameraCoordinate camCoord, ProjectInfo projectInfo)
        {
            cameraCoordinate = camCoord;
            this.projectInfo = projectInfo;
        }

        public void GetProjAndView(out Mat4 proj, out Mat4 view)
        {
            view = Matrix4X4.CreateLookAt<float>(cameraCoordinate.position, 
                                cameraCoordinate.position + cameraCoordinate.lookDir,
                                cameraCoordinate.up);
        proj = Matrix4X4.CreatePerspectiveFieldOfView<float>(projectInfo.viewRadians,
                projectInfo.ratio, projectInfo.near, projectInfo.far);
        proj.M11 *= -1.0f;
        }
    }
}
