using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Components
{
    public class Transform
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;

        public Matrix View =>
            Matrix.CreateFromQuaternion(Rotation) *
            Matrix.CreateScale(Scale) *
            Matrix.CreateTranslation(Position);

        public Vector3 Forward => Vector3.Transform(Vector3.Forward, Matrix.CreateFromQuaternion(Rotation));
        public Vector3 Backward => Vector3.Transform(Vector3.Backward, Matrix.CreateFromQuaternion(Rotation));
        public Vector3 Left => Vector3.Transform(Vector3.Left, Matrix.CreateFromQuaternion(Rotation));
        public Vector3 Right => Vector3.Transform(Vector3.Right, Matrix.CreateFromQuaternion(Rotation));
        public Vector3 Up => Vector3.Transform(Vector3.Up, Matrix.CreateFromQuaternion(Rotation));
        public Vector3 Down => Vector3.Transform(Vector3.Down, Matrix.CreateFromQuaternion(Rotation));
    }
}
