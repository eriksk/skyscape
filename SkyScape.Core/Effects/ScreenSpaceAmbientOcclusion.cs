using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Effects
{
    public class ScreenSpaceAmbientOcclusion : Effect
    {

        public Texture2D RandomTextureSampler { get; set; }
        public Texture2D DepthTexture { get; set; }
        public float TotalStrength { get; set; } = 2.0f;
        public float Base { get; set; } = -0.3333344f;
        public float Area { get; set; } = -0.2091671f;
        public float Falloff { get; set; } = 9.999204E-07f;
        public float Radius { get; set; } = -0.04813343f;

        public float Blur { get; set; } = 0.8f;
        public float BlurSampleDistance { get; set; } = 0.001f;

        public ScreenSpaceAmbientOcclusion(Effect effect)
            :base(effect)
        {
            
        }

        protected override bool OnApply()
        {
            Parameters["depthTex"].SetValue(DepthTexture);
            Parameters["_RandomTextureSampler"].SetValue(RandomTextureSampler);
            Parameters["total_strength"].SetValue(TotalStrength);
            Parameters["base"].SetValue(Base);
            Parameters["area"].SetValue(Area);
            Parameters["falloff"].SetValue(Falloff);
            Parameters["radius"].SetValue(Radius);

            Parameters["_Blur"].SetValue(Blur);
            Parameters["_SampleDistance"].SetValue(BlurSampleDistance);

            return base.OnApply();
        }
    }
}
