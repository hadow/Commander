using System;
using System.Collections;
using System.Collections.Generic;
namespace EW.Mobile.Platforms.Content
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


        static ContentTypeReaderManager()
        {
            _locker = new object();
            _contentReadersCache = new Dictionary<Type, ContentTypeReader>(255);
            _assemblyName = typeof(ContentTypeReaderManager).Assembly.FullName;
        }
    }
}