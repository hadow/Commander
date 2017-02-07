precision mediump float;

varying vec3 vertexE;
varying vec3 normalE;

uniform vec3 light;

void main()
{
	vec3 L = normalize (light - vertexE);
    vec3 E = normalize (-vertexE);
    vec3 R = normalize (-reflect (L, normalE));

    vec4 amb = vec4 (.2, .2, .2, 1.0);
    vec4 diff = vec4 (.8, .8, .8, 1.0) * max(dot(normalE,L), 0.0);
    diff = clamp (diff, 0.0, 1.0);

    gl_FragColor = amb + diff;
}
