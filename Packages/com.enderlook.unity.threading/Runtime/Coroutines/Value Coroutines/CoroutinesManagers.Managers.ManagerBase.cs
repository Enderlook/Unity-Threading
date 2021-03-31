namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutinesManager
    {
        internal partial class Managers
        {
            private abstract class ManagerBase
            {
                public abstract void OnUpdate();

                public abstract void OnFixedUpdate();

                public abstract void OnLateUpdate();

                public abstract void OnEndOfFrame();

                public abstract bool OnPoll(int until, float percentOfExecutions);

                public abstract void Free();
            }
        }
    }
}