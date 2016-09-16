using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Cameras
{
    public class FpsCameraController
    {
        private Camera _camera;
        private MouseState _oldMouse;
        private float _yaw, _pitch;

        public float HorizontalMultiplier { get; set; } = 1f;
        public float VerticalMultiplier { get; set; } = 1f;
        public float MouseSpeed { get; set; } = 1f;

        public float MoveSpeed { get; set; } = 1f;
        public float SprintMultiplier { get; set; } = 7f;

        public FpsCameraController(Camera camera)
        {
            _camera = camera;
            Mouse.SetPosition(1280 / 2, 720 / 2);
        }

        public void Update(float dt)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            var screenCenter = new Vector2(1280/2f, 720/2f);

            var horizontal = (screenCenter.X - mouse.X) * HorizontalMultiplier;
            var vertical = (screenCenter.Y - mouse.Y) * VerticalMultiplier;

            var strafeLeft = keyboard.IsKeyDown(Keys.A) ? -1f : 0f;
            var strafeRight = keyboard.IsKeyDown(Keys.D) ? 1f : 0f;

            var strafe = strafeLeft + strafeRight;

            var forward = keyboard.IsKeyDown(Keys.W) ? 1f : 0f;
            var backward = keyboard.IsKeyDown(Keys.S) ? -1f : 0f;
            var up = keyboard.IsKeyDown(Keys.Space) ? 1f : 0f;
            var down = keyboard.IsKeyDown(Keys.LeftControl) ? -1f : 0f;

            var linear = forward + backward;
            
            var direction = new Vector3(strafe, 0f, -linear);

            var transformedDirection = Vector3.Transform(direction, Matrix.CreateFromQuaternion(_camera.Transform.Rotation));
            transformedDirection.Y = up + down;


            var moveMultiplier = 1f;

            if (keyboard.IsKeyDown(Keys.LeftShift))
                moveMultiplier = SprintMultiplier;

            _camera.Transform.Position += transformedDirection * MoveSpeed * moveMultiplier * 0.01f * dt;

            _yaw += horizontal * MouseSpeed * 0.0005f * dt;
            _pitch += vertical * MouseSpeed * 0.0005f * dt;

            _pitch = MathHelper.Clamp(_pitch, -MathHelper.PiOver2 * 0.99f, MathHelper.PiOver2 * 0.99f);

            const float roll = 0f;

            _camera.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, roll);

            _oldMouse = mouse;
            Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
        }
    }
}
