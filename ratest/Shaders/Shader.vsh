attribute vec3 position;
attribute vec3 normal;
attribute vec2 texCoord;

varying vec3 vertexE;
varying vec3 normalE;

uniform vec3 light;
uniform mat4 projection;
uniform mat4 view;
uniform mat4 normalMatrix;

void main()
{
	vec4 vpos = vec4(position, 1.0);
	vertexE = (view * vpos).xyz;
	normalE = normalize((normalMatrix * vec4(normal, 1.0)).xyz);
    gl_Position = projection * vpos;
}
