using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using EW.Framework;
using EW.Framework.Graphics;
namespace EW.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class RgbaColorRenderer:Renderer.IBatchRenderer
    {

        static readonly Vector2 Offset = new Vector2(0.5f, 0.5f);

        readonly Renderer renderer;

        readonly IShader shader;

        readonly Vertex[] vertices;

        readonly Action renderAction;
        int nv = 0;

        public RgbaColorRenderer(Renderer renderer,IShader shader)
        {
            this.renderer = renderer;
            this.shader = shader;
            vertices = new Vertex[renderer.TempBufferSize];
            renderAction = () => renderer.DrawBatch(vertices, nv, PrimitiveType.TriangleList);
        }

        public void Flush()
        {
            if (nv > 0)
            {
                renderer.Device.SetBlendMode(BlendMode.Alpha);
                shader.Render(renderAction);
                renderer.Device.SetBlendMode(BlendMode.None);

                nv = 0;
            }
        }

        public void DrawRect(Vector3 tl,Vector3 br,float width,Color color){

            var tr = new Vector3(br.X, tl.Y, tl.Z);
            var bl = new Vector3(tl.X, br.Y, br.Z);

            DrawPolygon(new[] { tl, tr, br, bl }, width, color);

        }


        public void DrawPolygon(Vector3[] vertices,float width,Color color)
        {
            DrawConnectedLine(vertices, width, color, true);
        }

        public void DrawPolygon(Vector2[] vertices,float width,Color color)
        {
            DrawConnectedLine(vertices.Select(v => new Vector3(v, 0)).ToArray(), width, color, true);
        }

        public void DrawLine(IEnumerable<Vector3> points,float width,Color color,bool connectSegments = false)
        {
            if (!connectSegments)
                DrawDisconnectedLine(points, width, color);
            else
                DrawConnectedLine(points as Vector3[] ?? points.ToArray(), width, color, false);
        }

        void DrawDisconnectedLine(IEnumerable<Vector3> points,float width,Color color)
        {
            using(var e = points.GetEnumerator())
            {
                if (!e.MoveNext())
                    return;

                var lastPoint = e.Current;
                while (e.MoveNext())
                {
                    var point = e.Current;
                    DrawLine(lastPoint, point, width, color);
                    lastPoint = point;
                }
            }
        }


        public void DrawLine(Vector3 start,Vector3 end,float width,Color startColor,Color endColor)
        {
            renderer.CurrentBatchRenderer = this;

            if (nv + 6 > renderer.TempBufferSize)
                Flush();

            var delta = (end - start) / (end - start).XY.Length;
            var corner = width / 2 * new Vector3(-delta.Y, delta.X, delta.Z);

            startColor = Util.PremultiplyAlpha(startColor);

            var sr = startColor.R / 255.0f;
            var sg = startColor.G / 255.0f;
            var sb = startColor.B / 255.0f;
            var sa = startColor.A / 255.0f;

            endColor = Util.PremultiplyAlpha(endColor);

            var er = endColor.R / 255.0f;
            var eg = endColor.G / 255.0f;
            var eb = endColor.B / 255.0f;
            var ea = endColor.A / 255.0f;

            vertices[nv++] = new Vertex(start - corner + Offset, sr, sg, sb, sa, 0, 0);
            vertices[nv++] = new Vertex(start + corner + Offset, sr, sg, sb, sa, 0, 0);
            vertices[nv++] = new Vertex(end + corner + Offset, er, eg, eb, ea, 0, 0);
            vertices[nv++] = new Vertex(end + corner + Offset, er, eg, eb, ea, 0, 0);
            vertices[nv++] = new Vertex(end - corner + Offset, er, eg, eb, ea, 0, 0);
            vertices[nv++] = new Vertex(start - corner + Offset, sr, sg, sb, sa, 0, 0);
        }

        public void DrawLine(Vector3 start,Vector3 end,float width,Color color)
        {
            renderer.CurrentBatchRenderer = this;
            if (nv + 6 > renderer.TempBufferSize)
                Flush();

            var delta = (end - start) / (end - start).XY.Length;
            var corner = width / 2 * new Vector2(-delta.Y, delta.X);

            color = Util.PremultiplyAlpha(color);
            var r = color.R / 255.0f;
            var g = color.G / 255.0f;
            var b = color.B / 255.0f;
            var a = color.A / 255.0f;

            vertices[nv++] = new Vertex(start - corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(start + corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(end + corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(end + corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(end - corner + Offset, r, g, b, a, 0, 0);
            vertices[nv++] = new Vertex(start - corner + Offset, r, g, b, a, 0, 0);
        }


        void DrawConnectedLine(Vector3[] points, float width, Color color, bool closed)
        {
            // Not a line
            if (points.Length < 2)
                return;

            // Single segment
            if (points.Length == 2)
            {
                DrawLine(points[0], points[1], width, color);
                return;
            }

            renderer.CurrentBatchRenderer = this;
            color = Util.PremultiplyAlpha(color);
            var r = color.R / 255.0f;
            var g = color.G / 255.0f;
            var b = color.B / 255.0f;
            var a = color.A / 255.0f;

            var start = points[0];
            var end = points[1];
            var dir = (end - start) / (end - start).XY.Length;
            var corner = width / 2 * new Vector3(-dir.Y, dir.X, dir.Z);

            // Corners for start of line segment
            var ca = start - corner;
            var cb = start + corner;

            // Segment is part of closed loop
            if (closed)
            {
                var prev = points[points.Length - 1];
                var prevDir = (start - prev) / (start - prev).XY.Length;
                var prevCorner = width / 2 * new Vector3(-prevDir.Y, prevDir.X, prevDir.Z);
                ca = IntersectionOf(start - prevCorner, prevDir, start - corner, dir);
                cb = IntersectionOf(start + prevCorner, prevDir, start + corner, dir);
            }

            var limit = closed ? points.Length : points.Length - 1;
            for (var i = 0; i < limit; i++)
            {
                var next = points[(i + 2) % points.Length];
                var nextDir = (next - end) / (next - end).XY.Length;
                var nextCorner = width / 2 * new Vector3(-nextDir.Y, nextDir.X, nextDir.Z);

                // Vertices for the corners joining start-end to end-next
                var cc = closed || i < limit ? IntersectionOf(end + corner, dir, end + nextCorner, nextDir) : end + corner;
                var cd = closed || i < limit ? IntersectionOf(end - corner, dir, end - nextCorner, nextDir) : end - corner;

                // Fill segment
                if (nv + 6 > renderer.TempBufferSize)
                    Flush();

                vertices[nv++] = new Vertex(ca + Offset, r, g, b, a, 0, 0);
                vertices[nv++] = new Vertex(cb + Offset, r, g, b, a, 0, 0);
                vertices[nv++] = new Vertex(cc + Offset, r, g, b, a, 0, 0);
                vertices[nv++] = new Vertex(cc + Offset, r, g, b, a, 0, 0);
                vertices[nv++] = new Vertex(cd + Offset, r, g, b, a, 0, 0);
                vertices[nv++] = new Vertex(ca + Offset, r, g, b, a, 0, 0);

                // Advance line segment
                end = next;
                dir = nextDir;
                corner = nextCorner;

                ca = cd;
                cb = cc;
            }
        }

        /// <summary>
		/// Calculate the 2D intersection of two lines.
		/// Will behave badly if the lines are parallel.
		/// Z position is the average of a and b (ignores actual intersection point if it exists)
		/// </summary>
		Vector3 IntersectionOf(Vector3 a, Vector3 da, Vector3 b, Vector3 db)
        {
            var crossA = a.X * (a.Y + da.Y) - a.Y * (a.X + da.X);
            var crossB = b.X * (b.Y + db.Y) - b.Y * (b.X + db.X);
            var x = da.X * crossB - db.X * crossA;
            var y = da.Y * crossB - db.Y * crossA;
            var d = da.X * db.Y - da.Y * db.X;
            return new Vector3(x / d, y / d, 0.5f * (a.Z + b.Z));
        }

        public void FillRect(Vector3 tl,Vector3 br,Color color)
        {
            var tr = new Vector3(br.X, tl.Y, tl.Z);
            var bl = new Vector3(tl.X, br.Y, br.Z);

            FillRect(tl, tr, br, bl, color);
        }

        public void FillRect(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color)
        {
            renderer.CurrentBatchRenderer = this;
            if (nv + 6 > renderer.TempBufferSize)
                Flush();

            color = Util.PremultiplyAlpha(color);
            var cr = color.R / 255.0f;
            var cg = color.G / 255.0f;
            var cb = color.B / 255.0f;
            var ca = color.A / 255.0f;

            vertices[nv++] = new Vertex(a + Offset, cr, cg, cb, ca, 0, 0);
            vertices[nv++] = new Vertex(b + Offset, cr, cg, cb, ca, 0, 0);
            vertices[nv++] = new Vertex(c + Offset, cr, cg, cb, ca, 0, 0);
            vertices[nv++] = new Vertex(c + Offset, cr, cg, cb, ca, 0, 0);
            vertices[nv++] = new Vertex(d + Offset, cr, cg, cb, ca, 0, 0);
            vertices[nv++] = new Vertex(a + Offset, cr, cg, cb, ca, 0, 0);
        }

        public void SetViewportParams(Size screen,float depthScale,float depthOffset,float zoom,Int2 scroll)
        {
            shader.SetVec("Scroll",scroll.X,scroll.Y,scroll.Y);
            shader.SetVec("r1",zoom*2f/screen.Width,-zoom*2f/screen.Height,-depthScale*zoom/screen.Height);
            shader.SetVec("r2",-1,1,1-depthOffset);
        }

        public void SetDepthPreviewEnabled(bool enabled)
        {
            shader.SetBool("EnableDepthPreview",enabled);
        }

        public void FillEllipse(Vector3 tl, Vector3 br, Color color, int vertices = 32)
        {
            // TODO: Create an ellipse polygon instead
            var a = (br.X - tl.X) / 2;
            var b = (br.Y - tl.Y) / 2;
            var xc = (br.X + tl.X) / 2;
            var yc = (br.Y + tl.Y) / 2;
            for (var y = tl.Y; y <= br.Y; y++)
            {
                var z = Vector2.Lerp(tl.Z, br.Z, (y - tl.Y) / (br.Y - tl.Y));
                var dx = a * (float)Math.Sqrt(1 - (y - yc) * (y - yc) / b / b);
                DrawLine(new Vector3(xc - dx, y, z), new Vector3(xc + dx, y, z), 1, color);
            }
        }
    }
}