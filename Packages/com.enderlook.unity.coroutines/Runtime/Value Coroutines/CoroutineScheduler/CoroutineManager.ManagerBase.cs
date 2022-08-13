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

            public abstract int PollCount { get; }

            public abstract void OnPoll(int until, bool guaranteMinimumExecution);

#if !UNITY_WEBGL
            public abstract void BackgroundResume();
#endif

            public abstract void Dispose(ref RawQueue<ValueTask> tasks);
        }
    }
}