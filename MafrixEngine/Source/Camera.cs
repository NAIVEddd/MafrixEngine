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
        public Vec3 right;
        private Vec3 rotateUp;
        public CameraCoordinate(Vec3 pos, Vec3 dir, Vec3 up)
        {
            position = pos;
            lookDir = Vector3D.Normalize(dir);
            this.up = Vector3D.Normalize(up);
            right = Vector3D.Cross(up, lookDir);
            this.up = -Vector3D.Cross(right, lookDir);
            rotateUp = new Vec3(0, -1, 0);
        }
        public void RotateAroundUp(float degree)
        {
            var v = Scalar.DegreesToRadians(degree);
            var newUp = Vector3D.Normalize(new Vec3(0, rotateUp.Y, 0));
            var mat = Matrix3X3.CreateFromAxisAngle(newUp, v);
            lookDir = Vector3D.Normalize(lookDir * mat);
            right = Vector3D.Normalize(right * mat);
            up = -Vector3D.Cross(right, lookDir);
        }
        public void RotateAroundRight(float degree)
        {
            var v = Scalar.DegreesToRadians(degree);
            var mat = Matrix3X3.CreateFromAxisAngle(right, v);
            up = Vector3D.Normalize(up * mat);
            lookDir = Vector3D.Normalize(lookDir * mat);
            rotateUp = rotateUp * mat;
            //right = Vector3D.Cross(up, lookDir);
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

        public void OnForward()
        {
            cameraCoordinate.position += cameraCoordinate.lookDir * 10.0f;
        }
        public void OnBackward()
        {
            cameraCoordinate.position -= cameraCoordinate.lookDir * 10.0f;
        }
        public void OnLeft()
        {
            cameraCoordinate.position -= cameraCoordinate.right * 10.0f;
        }
        public void OnRight()
        {
            cameraCoordinate.position += cameraCoordinate.right * 10.0f;
        }
        public void OnRotate(float x, float y)
        {
            if(Scalar.Abs(x) > Scalar.Abs(y))
            {
                x = Scalar.Min(1.5f, Scalar.Max(-1.5f, x));
                cameraCoordinate.RotateAroundUp(x);
            }
            else
            {
                y = Scalar.Min(0.75f, Scalar.Max(-0.75f, y));
                cameraCoordinate.RotateAroundRight(y);
            }
        }
    }
}
