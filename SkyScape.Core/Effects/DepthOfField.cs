using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SkyScape.Core.Effects
{
    public class DepthOfField : Effect
    {
        public Texture DepthTexture { get; set; }
        public float Distance { get; set; } = 2f;
        private float _distance = 2f;
        public float Range { get; set; } = 128f;

        public float DistanceChangeSpeed { get; set; } = 0.1f;

        public float FarClip { get; set; }

        public float Blur { get; set; } = 6f;
        public float BlurSampleDistance { get; set; } = 0.001f;


        public DepthOfField(Effect effect)
            :base(effect)
        {
        }

        public void Update(float dt)
        {
            _distance = MathHelper.Lerp(_distance, Distance, DistanceChangeSpeed);
        }

        protected override bool OnApply()
        {
            Parameters["_Distance"].SetValue(_distance);
            Parameters["depthTex"].SetValue(DepthTexture);
            Parameters["_Blur"].SetValue(Blur);
            Parameters["_SampleDistance"].SetValue(BlurSampleDistance);
            Parameters["_Range"].SetValue(Range);
            Parameters["_Far"].SetValue(FarClip);
            return base.OnApply();
        }
    }
}
