sampler mainTex : register(s0);
sampler depthTex;
sampler _RandomTextureSampler;

float total_strength = 1.0;
float base = 0.2;
float area = 0.0075;
float falloff = 0.000001;
float radius = 0.0002;

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



float3 normal_from_depth(float depth, float2 texcoords) {

	const float2 offset1 = float2(0.0, 0.001);
	const float2 offset2 = float2(0.001, 0.0);

	float depth1 = tex2D(depthTex, texcoords + offset1).r;
	float depth2 = tex2D(depthTex, texcoords + offset2).r;

	float3 p1 = float3(offset1, depth1 - depth);
	float3 p2 = float3(offset2, depth2 - depth);

	float3 normal = cross(p1, p2);
	normal.z = -normal.z;

	return normalize(normal);
}

float4 main(float4 pos : SV_POSITION, float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
	const int samples = 16;
	float3 sample_sphere[samples] = {
		float3(0.5381, 0.1856,-0.4319), float3(0.1379, 0.2486, 0.4430),
		float3(0.3371, 0.5679,-0.0057), float3(-0.6999,-0.0451,-0.0019),
		float3(0.0689,-0.1598,-0.8547), float3(0.0560, 0.0069,-0.1843),
		float3(-0.0146, 0.1402, 0.0762), float3(0.0100,-0.1924,-0.0344),
		float3(-0.3577,-0.5301,-0.4358), float3(-0.3169, 0.1063, 0.0158),
		float3(0.0103,-0.5869, 0.0046), float3(-0.0897,-0.4940, 0.3287),
		float3(0.7119,-0.0154,-0.0918), float3(-0.0533, 0.0596,-0.5411),
		float3(0.0352,-0.0631, 0.5460), float3(-0.4776, 0.2847,-0.0271)
	};

	float3 random = normalize(tex2D(_RandomTextureSampler, uv * 4.0).rgb);

	float depth = tex2D(depthTex, uv).r;

	float3 position = float3(uv, depth);
	float3 normal = normal_from_depth(depth, uv);

	float radius_depth = radius / depth;
	float occlusion = 0.0;
	for (int i = 0; i < samples; i++) {

		float3 ray = radius_depth * reflect(sample_sphere[i], random);
		float3 hemi_ray = position + sign(dot(ray,normal)) * ray;

		float occ_depth = tex2D(depthTex, saturate(hemi_ray.xy)).r;
		float difference = depth - occ_depth;

		occlusion += step(falloff, difference) * (1.0 - smoothstep(falloff, area, difference));
	}

	float ao = 1.0 - total_strength * occlusion * (1.0 / samples);

	float final = 1.0 - saturate(ao + base);

	return float4(final, final, final, 1.0);
}


float4 apply(float4 pos : SV_POSITION, float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
	float4 col = tex2D(mainTex, uv);
	float4 ao = 0;

	for (int i = 0; i < 12; i++)
		ao += tex2D(depthTex, uv.xy + float2(directions[i] * _SampleDistance) * _Blur);

	ao /= 12.0;

	return col * ao;
}


technique SSAO
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 main();
	}
}

technique SSAO_Apply
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 apply();
	}
}