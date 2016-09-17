#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture _MainTex;
sampler2D textureSampler = sampler_state {
	Texture = (_MainTex);
	MagFilter = Point;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

// Camera
matrix _WorldViewProjection;
float4x4 _WorldInverseTranspose;
float _FarClip;

// Colors
float _Alpha = 1.0;

// Ambient Light
float4 _AmbientColor;
float _AmbientIntensity;

// Directional Diffuse Light
float3 _DiffuseLightDirection = float3(1, 0, 0);
float4 _DiffuseColor = float4(1, 1, 1, 1);
float _DiffuseIntensity = 1.0;

// Fog
float _FogEnabled;
float _FogStart;
float _FogEnd;
float3 _FogColor;

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
	float2 Uv : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD1;
	float Depth : COLOR1;
	float2 Uv : TEXCOORD0;
	float FogFactor : TEXCOORD2;
};

struct PS_OUTPUT
{
	float4 Color: SV_Target0;
	float4 Depth: SV_Target1;
};

float ComputeFogFactor(float d)
{
	//d is the distance to the geometry sampling from the camera
	//this simply returns a value that interpolates from 0 to 1 
	//with 0 starting at FogStart and 1 at FogEnd 
	return clamp((d - _FogStart) / (_FogEnd - _FogStart), 0, 1) * _FogEnabled;
}


VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, _WorldViewProjection);
	output.Uv = input.Uv;

	float3 normal = mul(input.Normal, _WorldInverseTranspose);
	float4 diffuseLightIntensity = dot(normal, -_DiffuseLightDirection);
	output.Color = saturate(input.Color + _DiffuseColor * _DiffuseIntensity * diffuseLightIntensity);

	output.Normal = normal;
	output.Depth = 1.0 - (output.Position.z / _FarClip);

	output.FogFactor = ComputeFogFactor(length(output.Position.z));

	return output;
}

PS_OUTPUT MainPS(VertexShaderOutput input) : COLOR
{

	PS_OUTPUT output = (PS_OUTPUT)0;

	float4 textureColor = tex2D(textureSampler, input.Uv);
	textureColor.a = 1.0;

	output.Color = lerp(float4(saturate((textureColor) + _AmbientColor * _AmbientIntensity).rgb, _Alpha), float4(_FogColor, _Alpha), input.FogFactor);
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