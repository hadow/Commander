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
	float2 aVertexTexMetadata : TEXCOORD1;	
};



BEGIN_CONSTANTS

	float3 Scroll				_vs(c0) _cb(c0);
	float3 r1					_vs(c1) _cb(c1);
	float3 r2					_vs(c2) _cb(c2);
	float DepthTextureScale		_ps(c0) _cb(c3);
	bool EnableDepthPreview		_ps(c1) _cb(c4);
	
MATRIX_CONSTANTS

    float4x4 MatrixTransform    _vs(c3) _cb(c0);

END_CONSTANTS


DECLARE_TEXTURE(DiffuseTexture, 0);
DECLARE_TEXTURE(Palette,1);


struct VSOutput
{
	float4 position		: SV_Position;
	//float4 color		: COLOR0;
    float4 vTexCoord		: TEXCOORD0;
	float2 vTexMetadata	: TEXCOORD1;
};



VSOutput SpriteVertexShader(VSInputT input)
{
	VSOutput output;
    //output.position = float4((input.aVertexPosition.xyz - Scroll.xyz)*r1+r2,1);
	output.position = mul(input.aVertexPosition, MatrixTransform);
	output.vTexCoord = input.aVertexTexCoord;
	output.vTexMetadata = input.aVertexTexMetadata;
	return output;
}


float4 SpritePixelShader(VSOutput input) : SV_Target0
{
	float4 c = SAMPLE_TEXTURE(Palette,input.vTexMetadata);
	return SAMPLE_TEXTURE(DiffuseTexture, input.vTexCoord)*c;
}

TECHNIQUE( SpriteBatch, SpriteVertexShader, SpritePixelShader );