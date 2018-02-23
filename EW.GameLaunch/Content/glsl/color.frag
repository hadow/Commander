precision highp float;
varying vec4 vColor;
uniform bool EnableDepthPreview;

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
	float depth = gl_FragCoord.z;

	// Convert to window coords
	//gl_FragDepthEXT = 0.5 * depth + 0.5;

	if (EnableDepthPreview)
	{

	}
	else
		gl_FragColor = vColor;
}