#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix _WorldViewProjection;
float4x4 _WorldInverseTranspose;

float4 _AmbientColor;
float _AmbientIntensity;

float3 _DiffuseLightDirection = float3(1, 0, 0);
float4 _DiffuseColor = float4(1, 1, 1, 1);
float _DiffuseIntensity = 1.0;
float _Alpha = 1.0;

float _FarClip;

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD1;
	float Depth : COLOR1;
};

struct PS_OUTPUT
{
	float4 Color: SV_Target0;
	float4 Depth: SV_Target1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, _WorldViewProjection);

	float3 normal = mul(input.Normal, _WorldInverseTranspose);

	float4 diffuseLightIntensity = dot(normal, -_DiffuseLightDirection);

	output.Color = saturate(input.Color + _DiffuseColor * _DiffuseIntensity * diffuseLightIntensity);
//	output.Color = input.Color + saturate(_DiffuseIntensity * _DiffuseColor * diffuseLightIntensity); // input.Color + (_DiffuseIntensity * _DiffuseColor * diffuseLightIntensity);

	output.Normal = normal;
	output.Depth = 1.0 - (output.Position.z / _FarClip);

	return output;
}

PS_OUTPUT MainPS(VertexShaderOutput input) : COLOR
{

	PS_OUTPUT output = (PS_OUTPUT)0;

	output.Color = float4(saturate(input.Color + _AmbientColor * _AmbientIntensity).rgb, _Alpha);
	output.Depth = float4(input.Depth, input.Depth, input.Depth, 1.0);

	return output;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};