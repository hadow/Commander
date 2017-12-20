using System;
using System.Linq;
using EW.Graphics;
using EW.Mods.Cnc.FileFormats;
namespace EW.Mods.Cnc.Graphics
{
    struct Limb
    {
        public float Scale;
        public float[] Bounds;
        public byte[] Size;
        public ModelRenderData RenderData;
    }

    public class Voxel:IModel
    {
        readonly Limb[] limbData;
        readonly float[] transforms;
        readonly uint frames;
        readonly uint limbs;

        uint IModel.Frames { get { return frames; } }

        uint IModel.Sections { get { return limbs; } }


        public Voxel(VoxelLoader loader,VxlReader vxl,HvaReader hva)
        {
            if (vxl.LimbCount != hva.LimbCount)
                throw new InvalidOperationException("Voxel and hva limb counts don't match");

            transforms = hva.Transforms;
            frames = hva.FrameCount;
            limbs = hva.LimbCount;

            limbData = new Limb[vxl.LimbCount];
            for (var i = 0; i < vxl.LimbCount; i++)
            {
                var vl = vxl.Limbs[i];
                var l = new Limb();
                l.Scale = vl.Scale;
                l.Bounds = (float[])vl.Bounds.Clone();
                l.Size = (byte[])vl.Size.Clone();
                l.RenderData = loader.GenerateRenderData(vxl.Limbs[i]);
                limbData[i] = l;
            }
        }

        public float[] TransformationMatrix(uint limb, uint frame)
        {
            if (frame >= frames)
                throw new ArgumentOutOfRangeException("frame", "Only {0} frames exist.".F(frames));
            if (limb >= limbs)
                throw new ArgumentOutOfRangeException("limb", "Only {1} limbs exist.".F(limbs));

            var l = limbData[limb];
            var t = new float[16];
            Array.Copy(transforms, 16 * (limbs * frame + limb), t, 0, 16);

            // Fix limb position
            t[12] *= l.Scale * (l.Bounds[3] - l.Bounds[0]) / l.Size[0];
            t[13] *= l.Scale * (l.Bounds[4] - l.Bounds[1]) / l.Size[1];
            t[14] *= l.Scale * (l.Bounds[5] - l.Bounds[2]) / l.Size[2];

            // Center, flip and scale
            t = Util.MatrixMultiply(t, Util.TranslationMatrix(l.Bounds[0], l.Bounds[1], l.Bounds[2]));
            t = Util.MatrixMultiply(Util.ScaleMatrix(l.Scale, -l.Scale, l.Scale), t);

            return t;
        }

        public ModelRenderData RenderData(uint limb)
        {
            return limbData[limb].RenderData;
        }


        public float[] Size
        {
            get
            {
                return limbData.Select(a => a.Size.Select(b => a.Scale * b).ToArray())
                    .Aggregate((a, b) => new float[]
                    {
                        Math.Max(a[0], b[0]),
                        Math.Max(a[1], b[1]),
                        Math.Max(a[2], b[2])
                    });
            }
        }

        public float[] Bounds(uint frame)
        {
            var ret = new float[] { float.MaxValue, float.MaxValue, float.MaxValue,
                float.MinValue, float.MinValue, float.MinValue };

            for (uint j = 0; j < limbs; j++)
            {
                var l = limbData[j];
                var b = new float[]
                {
                    0, 0, 0,
                    (l.Bounds[3] - l.Bounds[0]),
                    (l.Bounds[4] - l.Bounds[1]),
                    (l.Bounds[5] - l.Bounds[2])
                };

                // Calculate limb bounding box
                var bb = Util.MatrixAABBMultiply(TransformationMatrix(j, frame), b);
                for (var i = 0; i < 3; i++)
                {
                    ret[i] = Math.Min(ret[i], bb[i]);
                    ret[i + 3] = Math.Max(ret[i + 3], bb[i + 3]);
                }
            }

            return ret;
        }
    }
}