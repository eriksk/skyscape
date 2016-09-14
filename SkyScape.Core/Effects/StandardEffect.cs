using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Effects
{
    public class StandardEffect : Effect
    {
        public StandardEffect(Effect effect)
            :base(effect)
        {
        }

        public Matrix World { get; private set; } = Matrix.Identity;
        public Matrix View { get; set; } = Matrix.Identity;
        public Matrix Projection { get; set; } = Matrix.Identity;

        public float AmbientIntensity { get; set; } = 0.3f;
        public Color AmbientColor { get; set; } = Color.White;

        public float DiffuseIntensity { get; set; } = 0.7f;
        public Color DiffuseColor { get; set; } = new Color(255, 242, 179);
        public Vector3 DiffuseLightDirection { get; set; } = (Vector3.Right + Vector3.Down + Vector3.Forward).Normalized();

        public float Alpha { get; set; } = 1f;

        public float FarClip { get; set; } = 100f;

        public void ApplyForModel(Matrix world)
        {
            World = world;
            OnApply();
        }

        protected override bool OnApply()
        {
            // Transform
            Parameters["_WorldViewProjection"].SetValue(World * View * Projection);

            // Ambient light
            Parameters["_AmbientIntensity"].SetValue(AmbientIntensity);
            Parameters["_AmbientColor"].SetValue(AmbientColor.ToVector4());

            // Diffuse light
            Parameters["_DiffuseIntensity"].SetValue(DiffuseIntensity);
            Parameters["_DiffuseColor"].SetValue(DiffuseColor.ToVector4());
            Parameters["_DiffuseLightDirection"].SetValue(DiffuseLightDirection);

            var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
            Parameters["_WorldInverseTranspose"].SetValue(worldInverseTranspose);

            // Depth
            Parameters["_FarClip"].SetValue(FarClip);

            // Color
            Parameters["_Alpha"].SetValue(Alpha);

            return base.OnApply();
        }
    }
}
