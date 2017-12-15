using System;
using System.Runtime.InteropServices;

namespace EW.OpenGLES.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public readonly float X, Y, Z, S, T, U, V, P, C;


        public Vertex(Vector3 xyz,float s,float t,float u,float v,float p,float c) : this(xyz.X, xyz.Y, xyz.Z, s, t, u, v, p, c) { }


        public Vertex(float x, float y, float z, float s, float t, float u, float v, float p, float c) {

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