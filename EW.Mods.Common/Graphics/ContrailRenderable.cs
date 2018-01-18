using System;
using EW.Graphics;
using System.Drawing;
using System.Linq;
namespace EW.Mods.Common.Graphics
{
    
    public struct ContrailRenderable:IRenderable,IFinalizedRenderable
    {
        readonly World world;
        readonly Color color;
        readonly int zOffset;

        readonly WPos[] trail;
        readonly WDist width;

        int next;
        int length;
        int skip;


        public ContrailRenderable(World world,Color color,WDist width,int length,int skip,int zoffset):
        this(world,new WPos[length],width,0,0,skip,color,zoffset){}

        public ContrailRenderable(World world,WPos[] trail,WDist width,int next,int length,int skip,Color color,int zOffset)
        {
            this.world = world;
            this.trail = trail;
            this.width = width;
            this.next = next;
            this.length = length;
            this.skip = skip;
            this.color = color;
            this.zOffset = zOffset;

        }

        public void Render(WorldRenderer wr){

            //Need at least 4 points to smooth the contrail over
            if (length - skip < 4)
                return;

            var screenWidth = wr.ScreenVector(new WVec(width, WDist.Zero, WDist.Zero))[0];

            var wcr = WarGame.Renderer.WorldRgbaColorRenderer;


        }

        public WPos Pos{ get { return trail[Index(next - 1)]; }}


        public PaletteReference Palette{ get { return null; }}

        public int ZOffset{ get { return zOffset; }}

        public bool IsDecoration{ get { return true; }}

        public IRenderable WithPalette(PaletteReference newPalette) { return new ContrailRenderable(world, (WPos[])trail.Clone(), width, next, length, skip, color, zOffset); }


        public IRenderable WithZOffset(int newOffset) { return new ContrailRenderable(world, (WPos[])trail.Clone(), width, next, length, skip, color, newOffset); }

        public IRenderable AsDecoration() {return this; }


        public IRenderable OffsetBy(WVec vec) { return new ContrailRenderable(world, trail.Select(pos => pos + vec).ToArray(), width, next, length, skip, color, zOffset); }


        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }


        public void RenderDebugGeometry(WorldRenderer wr){}

        public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }


        int Index(int i){

            var j = i % trail.Length;
            return j < 0 ? j + trail.Length : j;
        }
    }
}
