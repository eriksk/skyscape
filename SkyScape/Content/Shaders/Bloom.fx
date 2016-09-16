sampler mainTex : register(s0);

float _Amount;
float _Treshold;

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
	float4 blur = tex2D(mainTex, uv);

	for (int i = 0; i < 12; i++)
		blur += tex2D(mainTex, uv.xy + float2(directions[i] * _SampleDistance) * _Blur);

	blur /= 12.0;
	blur.a = 1.0;

	float rgb = (blur.r + blur.g + blur.b) / 3.0;

	if(rgb < _Treshold)
	{
		blur.rgb = 0.0;
	}

	float4 col = tex2D(mainTex, uv);
	return col + (blur * _Amount);
}


technique SSAO
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 main();
	}
}
