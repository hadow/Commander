using System;
using EW.Xna.Platforms.Graphics;
using EW.FileSystem;
namespace EW.Graphics
{
    public interface IModel
    {
        uint Frames { get; }
        uint Sections { get; }

        float[] TransformationMatrix(uint section, uint frame);

        float[] Size { get; }

        float[] Bounds(uint frame);

        ModelRenderData RenderData(uint section);
    }

    public interface IModelCache : IDisposable
    {
        IModel GetModelSequence(string model, string sequence);

        bool HasModelSequence(string model, string sequence);

        DynamicVertexBuffer VertexBuffer { get; }
    }


    public interface IModelSequenceLoader
    {
        Action<string> OnMissingModelError { get; set; }
        IModelCache CachModels(IReadOnlyFileSystem fileSystem, ModData modData, IReadOnlyDictionary<string, MiniYamlNode> modelDefinitions);

    }

    public struct ModelRenderData
    {
        public readonly int Start;
        public readonly int Count;
        public readonly Sheet Sheet;

        public ModelRenderData(int start, int count, Sheet sheet)
        {
            Start = start;
            Count = count;
            Sheet = sheet;
        }


    }


    public class PlaceholderModelSequenceLoader : IModelSequenceLoader
    {
        public Action<string> OnMissingModelError { get; set; }

        class PlaceholderModelCache : IModelCache
        {
            public DynamicVertexBuffer VertexBuffer { get { throw new NotImplementedException(); } }

            public void Dispose() { }

            public IModel GetModelSequence(string model,string sequence)
            {
                throw new NotImplementedException();
            }


            public bool HasModelSequence(string model,string sequence)
            {
                throw new NotImplementedException();
            }

        }

        public PlaceholderModelSequenceLoader(ModData modData) { }


        public IModelCache CachModels(IReadOnlyFileSystem fileSystem,ModData modData,IReadOnlyDictionary<string,MiniYamlNode> modelDefinitions)
        {
            return new PlaceholderModelCache();
        }
    }

}