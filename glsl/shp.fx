//-----------------------------------------------------------------------------
// SpriteEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"
struct VSInputT
{
	float4 aVertexPosition : POSITION0;
	float4 aVertexTexCoord : TEXCOORD0;
	float2 aVertexTexMetadata : NORMAL;	
};



BEGIN_CONSTANTS

	float3 Scroll				_vs(c0) _cb(c0);
	float3 r1					_vs(c1) _cb(c1);
	float3 r2					_vs(c2) _cb(c2);
	float DepthTextureScale		_ps(c0) _cb(c3);
	bool EnableDepthPreview		_ps(c1) _cb(c4);
	
//MATRIX_CONSTANTS

//    float4x4 MatrixTransform    _vs(c3) _cb(c0);

END_CONSTANTS


DECLARE_TEXTURE(DiffuseTexture, 0);
DECLARE_TEXTURE(Palette,1);


struct VSOutput
{
	float4 position		: SV_Position;
	//float4 color		: COLOR0;
    float2 texCoord		: TEXCOORD0;
};



VSOutput SpriteVertexShader(VSInputT input)
{
	VSOutput output;
    output.position = mul(input.aVertexPosition, float4(0,0,0,0));
	//output.color = color;
	output.texCoord = input.aVertexTexCoord+input.aVertexTexMetadata + Scroll + r1 + r2;
	return output;
}


float4 SpritePixelShader(VSOutput input) : SV_Target0
{
    //return SAMPLE_TEXTURE(DiffuseTexture, input.texCoord) * input.color;
	//EnableDepthPreview = false;
	if(EnableDepthPreview)
		return float4(0,0,0,0);
	return (SAMPLE_TEXTURE(DiffuseTexture,input.texCoord) + SAMPLE_TEXTURE(Palette,input.texCoord))*DepthTextureScale;
}

TECHNIQUE( SpriteBatch, SpriteVertexShader, SpritePixelShader );