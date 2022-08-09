using Enderlook.Collections.LowLevel;

using System.Threading.Tasks;

namespace Enderlook.Unity.Coroutines
{
    public sealed partial class CoroutineManager
    {
        private abstract class ManagerBase
        {
            public abstract void OnUpdate();

            public abstract void OnFixedUpdate();

            public abstract void OnLateUpdate();

            public abstract void OnEndOfFrame();

            public abstract int PollCount();

            public abstract bool OnPoll(int until, ref int i, int to);

#if !UNITY_WEBGL
            public abstract void BackgroundResume();
#endif

            public abstract void Dispose(ref RawQueue<ValueTask> tasks);
        }
    }
}