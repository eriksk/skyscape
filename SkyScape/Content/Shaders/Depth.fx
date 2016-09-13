
struct VS_SHADOW_OUTPUT
{
	float4 Position : POSITION;
	float Depth : TEXCOORD0;
};

VS_SHADOW_OUTPUT RenderShadowMapVS(float4 vPos: POSITION)
{
	VS_SHADOW_OUTPUT Out;
	Out.Position = vPos;
	// Depth is Z/W.  This is returned by the pixel shader.
	// Subtracting from 1 gives us more precision in floating point.
	Out.Depth.x = 1 - (Out.Position.z / Out.Position.w);
	return Out;
}

technique ShadowMapRender
{
	pass P0
	{
		CullMode = NONE;
		ZEnable = TRUE;
		ZWriteEnable = TRUE;
		AlphaBlendEnable = FALSE;

		VertexShader = compile vs_2_0 RenderShadowMapVS();
	}
}
