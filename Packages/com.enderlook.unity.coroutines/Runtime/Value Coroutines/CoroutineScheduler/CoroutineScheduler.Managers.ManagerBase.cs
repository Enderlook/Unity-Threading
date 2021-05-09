namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutineScheduler
    {
        internal partial class Managers
        {
            private abstract class ManagerBase
            {
                public abstract void OnUpdate();

                public abstract void OnFixedUpdate();

                public abstract void OnLateUpdate();

                public abstract void OnEndOfFrame();

                public abstract int PollCount();

                public abstract bool OnPoll(int until, ref int i, int to);

                public abstract void Free();

                public abstract void OnBackground();

                public abstract bool OnBackgroundFlushErrors();
            }
        }
    }
}