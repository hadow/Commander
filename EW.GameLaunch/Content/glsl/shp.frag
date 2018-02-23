precision lowp float;
uniform sampler2D DiffuseTexture, Palette;
uniform bool EnableDepthPreview;
uniform float DepthTextureScale;

varying vec4 vTexCoord;
varying vec2 vTexMetadata;
varying vec4 vChannelMask;
varying vec4 vDepthMask;

void main()
{
     vec4 x = texture2D(DiffuseTexture, vTexCoord.st);
	 vec2 p = vec2(dot(vChannelMask, x), vTexMetadata.s);
	 vec4 c = texture2D(Palette, p);
	// Discard any transparent fragments (both color and depth)
	if (c.a == 0.0)
		discard;

	// Convert to window coords
	//gl_FragDepthEXT = 0.5 * depth + 0.5;
	if (EnableDepthPreview)
	{
	}
	else
		gl_FragColor = c;
}
