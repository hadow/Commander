using System;


namespace EW.Mobile.Platforms.Content
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ContentTypeReader
    {

        private Type _targetT;

        public Type TargetT
        {
            get { return _targetT; }
        }

        protected ContentTypeReader(Type targetT)
        {
            _targetT = targetT;
        }

        /// <summary>
        /// 是否可反序列化至已存在的对象
        /// </summary>
        public virtual bool CanDeserializeIntoExistingObject { get { return false; } }

        protected internal virtual void Initialize(ContentTypeReaderManager manager) { }

        protected internal abstract object Read(ContentReader input, object existingInstance);

        

    }


    public abstract class ContentTypeReader<T> : ContentTypeReader
    {
        protected ContentTypeReader() : base(typeof(T)) { }

        protected internal override object Read(ContentReader input, object existingInstance)
        {
            if (existingInstance == null)
                return Read(input, default(T));
            return Read(input, (T)existingInstance);
        }

        protected internal abstract T Read(ContentReader input, T existingInstance);
    }
}