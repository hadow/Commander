using System.Runtime.InteropServices;
namespace EW.Graphics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex
	{

		public readonly float X, Y, Z, S, T, U, V, P, C;

		public Vertex(float x, float y, float z, float s, float t, float u, float v, float p, float c)
		{
			X = x;
			Y = y;
			Z = z;
			S = s;
			T = t;
			U = u;
			V = v;
			P = p;
			C = c;
		}

	}


}
