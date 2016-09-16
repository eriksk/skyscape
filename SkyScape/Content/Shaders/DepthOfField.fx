sampler mainTex : register(s0);
sampler depthTex;

float _Far;
float _Distance;
float _Range;

float _Blur;
float _SampleDistance;

static const float2 directions[12] = {
	1, 0,
	0.8660254, 0.5,
	0.5, 0.8660254,
	-4.371139E-08, 1,
	-0.5000001, 0.8660254,
	-0.8660254, 0.5000001,
	-1, -8.742278E-08,
	-0.8660254, -0.5,
	-0.4999999, -0.8660254,
	1.192488E-08, -1,
	0.4999999, -0.8660254,
	0.8660253, -0.5000002
};

float4 main(float4 pos : SV_POSITION, float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
	float depth = (1.0 - tex2D(depthTex, uv).r);
	float blurFactor = smoothstep(0, _Range, abs(_Distance - (depth * _Far)));

	float4 b = tex2D(mainTex, uv);

	for (int i = 0; i < 12; i++)
		b += tex2D(mainTex, uv.xy + float2(directions[i] * _SampleDistance) * _Blur);

	b /= 12.0;

	float4 col = tex2D(mainTex, uv);
	return lerp(col, b, blurFactor);
}


technique SSAO
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 main();
	}
}
