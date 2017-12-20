using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.FileSystem;
using System.IO;
using EW.OpenGLES.Graphics;

namespace EW.Mods.Cnc.Graphics
{
    public class VoxelModelSequenceLoader:IModelSequenceLoader
    {

        public Action<string> OnMissingModelError { get; set; }

        public VoxelModelSequenceLoader(ModData modData)
        {

        }

        public IModelCache CachModels(IReadOnlyFileSystem fileSystem,ModData modData,IReadOnlyDictionary<string,MiniYamlNode> modelSequences)
        {
            var cache = new VoxelModelCache(fileSystem);
            foreach(var kv in modelSequences)
            {
                modData.LoadScreen.Display();
                try
                {
                    cache.CacheModel(kv.Key, kv.Value.Value);
                }
                catch(FileNotFoundException ex)
                {
                    OnMissingModelError(ex.Message);
                }
            }
            cache.LoadComplete();

            return cache;
        }




    }


    public class VoxelModelCache : IModelCache
    {
        readonly VoxelLoader loader;

        readonly Dictionary<string, Dictionary<string, IModel>> models = new Dictionary<string, Dictionary<string, IModel>>();

        public VoxelModelCache(IReadOnlyFileSystem fileSystem)
        {
            loader = new VoxelLoader(fileSystem);
        }


        public IVertexBuffer<Vertex> VertexBuffer { get { return loader.VertexBuffer; } }

        public void CacheModel(string model,MiniYaml definition)
        {
            models.Add(model, definition.ToDictionary(my => LoadVoxel(model, my)));
        }
        public bool HasModelSequence(string model,string sequence)
        {
            if (!models.ContainsKey(model))
                throw new InvalidOperationException("Model '{0}' does not have any sequences defined.".F(model));
            return models[model].ContainsKey(sequence);
        }

        IModel LoadVoxel(string unit,MiniYaml info)
        {
            var vxl = unit;
            var hva = unit;
            if(info.Value != null)
            {
                var fields = info.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length >= 1)
                    vxl = hva = fields[0].Trim();
            }

            return loader.Load(vxl, hva);
                
        }

        public IModel GetModelSequence(string model,string sequence)
        {
            try
            {
                return models[model][sequence];
            }
            catch (KeyNotFoundException)
            {
                if (models.ContainsKey(model))
                    throw new InvalidOperationException("Model '{0}' does not have a sequence '{1}'".F(model, sequence));
                else
                    throw new InvalidOperationException("Model '{0}' does not have any sequence defined.".F(model));
            }
        }

        public void LoadComplete()
        {
            loader.RefreshBuffer();
            loader.Finish();
        }

        public void Dispose()
        {
            loader.Dispose();
        }
    }


}