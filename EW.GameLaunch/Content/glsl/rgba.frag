precision highp float;
uniform sampler2D DiffuseTexture;
uniform bool EnableDepthPreview;

varying vec4 vTexCoord;

float jet_r(float x)
{
	return x < 0.7 ? 4.0 * x - 1.5 : -4.0 * x + 4.5;
}

float jet_g(float x)
{
	return x < 0.5 ? 4.0 * x - 0.5 : -4.0 * x + 3.5;
}

float jet_b(float x)
{
	return x < 0.3 ? 4.0 * x + 0.5 : -4.0 * x + 2.5;
}

void main()
{
	vec4 c = texture2D(DiffuseTexture, vTexCoord.st);
	// Discard any transparent fragments (both color and depth)
	if (c.a == 0.0)
		discard;

	float depth = gl_FragCoord.z;

	// Convert to window coords
	//gl_FragDepthEXT = 0.5 * depth + 0.5;

	if (EnableDepthPreview)
	{

	}
	else
		gl_FragColor = c;
}