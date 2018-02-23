using System;
namespace EW.Primitives
{
    public sealed class DisposableAction:IDisposable
    {

        Action onDispose;
        Action onFinalize;
        bool disposed;

        public DisposableAction(Action onDispose,Action onFinalize)
        {
            this.onDispose = onDispose;
            this.onFinalize = onFinalize;

        }


        public void Dispose(){

            if (disposed)
                return;

            disposed = true;
            onDispose();
            GC.SuppressFinalize(this);
        }

        ~DisposableAction() { onFinalize(); }
    }
}
