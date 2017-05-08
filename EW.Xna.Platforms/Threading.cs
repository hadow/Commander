using System;
using System.Collections.Generic;
using System.Threading;
namespace EW.Xna.Platforms
{
    /// <summary>
    /// 
    /// </summary>
    internal class Threading
    {
        public const int kMaxWaitForUIThread = 750;

        static int mainThreadId;

#if ANDROID
        static List<Action> actions = new List<Action>();
#endif
        static Threading()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static bool IsOnUIThread()
        {
            return mainThreadId == Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Throws an exception if the code is not currently running on the UI thread
        /// </summary>
        public static void EnsureUIThread()
        {
            if (!IsOnUIThread())
                throw new InvalidOperationException("Operation not called on UI thread.");
        }

        internal static void BlockOnUIThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (IsOnUIThread())
            {
                action();
                return;
            }

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            Add(() =>
            {
#if ANDROID
                ((AndroidGameWindow)Game.Instance.Window).GameView.MakeCurrent();
#endif
                action();

                //将事件状态设置为有信号，从而允许一个或多个等待该事件的线程继续。
                resetEvent.Set();
            });
            //阻止当前线程，直到当前 ManualResetEventSlim 设置。
            resetEvent.Wait();

        }
#if ANDROID
        static void Add(Action action)
        {
            lock (actions)
            {
                actions.Add(action);
            }
                
        }


        /// <summary>
        /// 
        /// </summary>
        internal static void Run()
        {
            EnsureUIThread();
            lock (actions)
            {
                foreach(var action in actions)
                {
                    action();
                }
                actions.Clear();
            }
        }
#endif

    }
}