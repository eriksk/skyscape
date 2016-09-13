using Microsoft.Xna.Framework;
using SkyScape.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Cameras
{
    public class Camera
    {
        public Transform Transform;

        public float FOV { get; set; } = MathHelper.PiOver4;
        public float NearClip { get; set; } = 0.1f;
        public float FarClip { get; set; } = 1000f;
        public float AspectRatio { get; }

        public Camera(float screenWidth, float screenHeight)
        {
            AspectRatio = screenWidth / screenHeight;
            Transform = new Transform();
        }

        public Matrix View
        {
            get
            {
                Vector3 direction = Vector3.Transform(Vector3.Forward, Matrix.CreateFromQuaternion(Transform.Rotation));
                Vector3 lookAtTarget = Transform.Position + direction;
                return Matrix.CreateLookAt(Transform.Position, lookAtTarget, Vector3.Up);
            }
        }

        public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(FOV, AspectRatio, NearClip, FarClip);

        public void Apply()
        {
        }
    }
}
