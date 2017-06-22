using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
namespace EW.Xna.Platforms.Content
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ContentTypeReaderManager
    {
        private static readonly object _locker;

        private static readonly Dictionary<Type, ContentTypeReader> _contentReadersCache;

        private Dictionary<Type, ContentTypeReader> _contentReaders;

        private static readonly string _assemblyName;

        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<string, Func<ContentTypeReader>> typeCreators = new Dictionary<string, Func<ContentTypeReader>>();

        static ContentTypeReaderManager()
        {
            _locker = new object();
            _contentReadersCache = new Dictionary<Type, ContentTypeReader>(255);
            _assemblyName = typeof(ContentTypeReaderManager).Assembly.FullName;
        }

        /// <summary>
        /// 加载资源用到的读取器
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal ContentTypeReader[] LoadAssetReaders(ContentReader reader)
        {
            var numberOfReaders = reader.Read7BitEncodedInt();
            var contentReaders = new ContentTypeReader[numberOfReaders];
            var needInitialize = new BitArray(numberOfReaders);

            _contentReaders = new Dictionary<Type, ContentTypeReader>(numberOfReaders);

            lock (_locker)
            {
                for(var i = 0; i < numberOfReaders; i++)
                {
                    string originalReaderTypeStr = reader.ReadString();
                    Func<ContentTypeReader> readerFunc;
                    if(typeCreators.TryGetValue(originalReaderTypeStr,out readerFunc))
                    {
                        contentReaders[i] = readerFunc();
                        needInitialize[i] = true;
                    }
                    else
                    {
                        string readerTypeStr = originalReaderTypeStr;
                        readerTypeStr = PrepareType(readerTypeStr);
                        readerTypeStr = "EW.Xna.Platforms.Content.EffectReader, EW.Xna.Platforms";
                        var l_readerType = Type.GetType(readerTypeStr);
                        if (l_readerType != null)
                        {
                            ContentTypeReader typeReader;
                            if(!_contentReadersCache.TryGetValue(l_readerType,out typeReader))
                            {
                                try
                                {
                                    typeReader = l_readerType.GetDefaultConstructor().Invoke(null) as ContentTypeReader;
                                }
                                catch(TargetInvocationException ex)
                                {

                                }
                                needInitialize[i] = true;
                                _contentReadersCache.Add(l_readerType, typeReader);
                            }
                            contentReaders[i] = typeReader;
                        }
                        else
                        {
                            throw new Exception("Could not find ContentTypeReader Type,Please ensure the name of the Assembly that contains the Type matches the assembly in the full type name:"+originalReaderTypeStr + "("+readerTypeStr+")");
                        }
                    }

                    var targetType = contentReaders[i].TargetT;
                    if (targetType != null)
                        _contentReaders.Add(targetType, contentReaders[i]);

                    reader.ReadInt32();
                }

                //Initialize any new readers
                for(var i = 0; i < contentReaders.Length; i++)
                {
                    if (needInitialize.Get(i))
                        contentReaders[i].Initialize(this);
                }
            }

            return contentReaders;
        }


        public static string PrepareType(string type)
        {
            int count = type.Split(new[] { "[[" }, StringSplitOptions.None).Length-1;

            string preparedType = type;
            for(int i = 0; i < count; i++)
            {
                preparedType = Regex.Replace(preparedType, @"\[(.+?), Version=.+?\]", "[$1]");
            }

            //Handle non generic types
            if (preparedType.Contains("PublicKeyToken"))
                preparedType = Regex.Replace(preparedType, @"(.+?), Version=.+?$", "$1");
            return preparedType;
        }
    }
}