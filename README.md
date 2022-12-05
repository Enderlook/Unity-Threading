# Unity Threading

Provides helper APIs for multithreading in Unity.

### Simple Threading

Provides helper methods to access to the Unity main thread.

```cs
namespace Enderlook.Unity.Threading
{
    /// A helper class to execute actions on the Unity main thread.
    public static class UnityThread
    {
        /// Synchronization context used by Unity.
        public static SynchronizationContext UnitySynchronizationContext { get; }

        /// Thread Id used by Unity main thread.
        public static int UnityThreadId { get; }

        /// Task Scheduler used by Unity.
        public static TaskScheduler UnityTaskScheduler { get; }

        /// A <see cref="TaskFactory"/> where all tasks are run on the main (Unity) thread by using <see cref="UnityTaskScheduler"/>.
        public static TaskFactory Factory { get; }

        /// Check if we are currently running in main (Unity) thread.
        public static bool IsMainThread { get; }

        /// Determines if we are in the Unity synchronization context.
        public static bool IsUnitySynchronizationContext { get; }

        /// Subscribe delegates to execute in the Unity thread on each frame.
        public static event Action OnUpdate;

        /// Subscribe delegates to execute in the Unity thread on each physics update.
        public static event Action OnFixedUpdate;

        /// Subscribe delegates to execute in the Unity thread on each frame after update calls are executed.
        public static event Action OnLateUpdate;

        /// Subscribe delegates to execute in the Unity thread on each end of frame.
        public static event Action OnEndOfFrame;

        /// Executes the specified action on the Unity thread.
        /// The action will not be executed instantaneously, but later.
        public static void RunLater(Action action);
        public static void RunLater(SendOrPostCallback action, object state);
        public static void RunLater<T>(Action<T> action, T state);

        /// Executes the specified action on the Unity thread.
        /// The action will be immediately and this thread will wait until completion.
        public static void RunNow(Action action);
        public static void RunNow(SendOrPostCallback action, object state);
        public static void RunNow<T>(Action<T> action, T state);
        public static T RunNow<T>(Func<T> function);
        public static U RunNow<T, U>(Func<T, U> function, T state);
    }
}
```

### Async Threading

Provides helper methods to access to the Unity main thread or background threads using an `async/await` style.

Example:
```cs
await Switch.ToBackground;
ExpensiveOperation();

await Switch.ToUnity;
GameObject gameobject = new GameObject();
```

```cs
namespace Enderlook.Unity.Threading
{
    /// A helper class which allows to switch to a particular thread.
    public static class Switch
    {
        /// Switches to a background pool thread.
        public static ThreadSwitcherBackground ToBackground { get; }
        
        /// Switches to a background long duration thread.
        public static ThreadSwitcherLongBackground ToLongBackground { get; }

#if UNITY_EDITOR
        /// Switches to a background pool thread using the editor preferences instead of the standalone platform.
        /// This API only exists inside the Unity Editor.
        public static ThreadSwitcherBackground ToBackgroundEditor { get; }

        /// Switches to a background long duration thread using the editor preferences instead of the standalone platform.
        /// This API only exists inside the Unity Editor.
        public static ThreadSwitcherLongBackground ToLongBackgroundEditor { get; }
#endif

        /// Switch to the Unity thread.
        public static ThreadSwitcherUnity ToUnity  { get; }
    }
}
```

### Unity Coroutines

Provides helper methods to reduce allocations in Unity coroutines.

```cs
namespace Enderlook.Unity.Coroutines
{
    /// Provides a pool of waiters for Unity coroutines.
    public static class Wait
    {
        public static readonly WaitForEndOfFrame ForEndOfFrame = new WaitForEndOfFrame();
        public static readonly WaitForFixedUpdate ForFixedUpdate = new WaitForFixedUpdate();
        public static readonly WaitForUpdate ForUpdate = new WaitForUpdate();
        
        public static WaitForSeconds ForSeconds(float seconds);
        public static WaitUntilPooled Until(Func<bool> predicate);
        public static WaitWhilePooled While(Func<bool> predicate);
        public static WaitForJobComplete For(JobHandle handle);
        public static WaitForTaskComplete For(Task task);
        public static WaitForTaskComplete<T> For<T>(Task<T> task);
        public static WaitForValueTaskComplete For(ValueTask task);
        public static WaitForValueTaskComplete<T> For<T>(ValueTask<T> task);
        public static WaitForSecondsRealtimePooled ForRealtime(float seconds);
    }
    
    /// Allow get awaiter from <see cref="Coroutine"/>.
    public static class CoroutineAwaiterExtension
    {
        /// Convert a <see cref="Coroutine"/> to a task.
        /// The return type can be awaited.
        public static CoroutineAwaiter GetAwaiter(this Coroutine coroutine);
    }
}
```

### New Coroutines

It also provides a reimplementation of Unity coroutines that returns a value type instead to avoid allocations.
Rather than using `IEnumerator`, it requires `IEnumerator<ValueYieldInstruction>` or any type which implement `IEnumerator<ValueYieldInstruction>`.

Example:
```cs
IEnumerator<ValueYieldInstruction> Work()
{
    yield return Yield.BackgroundPoll;
    ExpensiveOperation();
    
    yield return Yield.ToEndOfFrame;
    GameObject gameObject = new GameObject();
}
```

```cs
namespace Enderlook.Unity.Coroutines
{
    /// Provides a pool of waiters for value coroutines.
    public static class Yield
    {
        /// Executes the code in a background thread.
        /// If the platform doesn't support multithreading, this fallback to <see cref="Poll"/>.
        public static ValueYieldInstruction BackgroundPoll { get; }

        /// Suspend coroutine execution during the given amount of realtime seconds.
        public static ValueYieldInstruction ForRealtimeSeconds(float seconds);

        /// Suspend coroutine execution during the given amount of seconds.
        public static ValueYieldInstruction ForSeconds(float seconds);

        /// Suspend coroutine execution until the <paramref name="coroutine"/> finalizes.
        public static ValueYieldInstruction From(IEnumerator<ValueYieldInstruction> coroutine);

        /// Suspend coroutine execution until the <paramref name="coroutine"/> finalizes.
        public static ValueYieldInstruction From(IEnumerator coroutine);

        /// Suspend coroutine execution if the frame is delayed.
        /// Code is run in the main thread.
        public static ValueYieldInstruction Poll { get; }

        /// Suspend coroutine execution until reach a background thread.
        public static ValueYieldInstruction ToBackground { get; }

        /// Suspend coroutine execution until end of frame.
        public static ValueYieldInstruction ToEndOfFrame { get; }

        /// Suspend coroutine execution until next fixed update.
        public static ValueYieldInstruction ToFixedUpdate { get; }
        
        /// Suspend coroutine execution after the next update is executed.
        public static ValueYieldInstruction ToLateUpdate { get; }

        /// Suspend coroutine execution until reach a long background thread.
        public static ValueYieldInstruction ToLongBackground { get; }

        /// Suspend coroutine execution until reach Unity thread.
        public static ValueYieldInstruction ToUnity { get; }

        /// Suspend coroutine execution until next update.
        public static ValueYieldInstruction ToUpdate { get; }

        /// Suspend coroutine execution until <paramref name="predicate"/> returns <see langword="true"/>.
        public static ValueYieldInstruction Until(Func<bool> predicate);

        /// Suspend coroutine execution while <paramref name="predicate"/> returns <see langword="true"/>.
        public static ValueYieldInstruction While(Func<bool> predicate);
    }
    
    /// Represent the handler of a value coroutine.
    /// This type can be awaited.
    public readonly partial struct ValueCoroutine : INotifyCompletion 
    {    
        /// Amount of miliseconds spent in executing global poll coroutines per frame.
        public static int MilisecondsExecutedPerFrameOnPoll { get; set; }

        /// Percentage of total execution that must be executed on per frame regardless of <see cref="MilisecondsExecutedPerFrameOnPoll"/> for global poll coroutines.
        public static float MinimumPercentOfExecutionsPerFrameOnPoll { get; set; }
        
        /// Initializes a coroutine.
        public static void Start<T>(T routine) where T : IEnumerator<ValueYieldInstruction>;
        public static void Start<T>(T routine, CancellationToken token) where T : IEnumerator<ValueYieldInstruction>;
        /// The creation of a handle has an small additional cost, a different method initialize coroutines and return a handle.
        public static ValueCoroutine StartWithHandle<T>(T routine) where T : IEnumerator<ValueYieldInstruction>;
        public static ValueCoroutine StartWithHandle<T>(T routine, CancellationToken token) where T : IEnumerator<ValueYieldInstruction>;
        
        /// Initializes a coroutine associated with an object.
        /// The corotuine execute as long the object exists.
        public static void Start<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>;
        public static void Start<T>(T routine, GameObject source) where T : IEnumerator<ValueYieldInstruction>;
        public static ValueCoroutine StartWithHandle<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>;
        public static ValueCoroutine StartWithHandle<T>(T routine, GameObject source) where T : IEnumerator<ValueYieldInstruction>;
        // `suspendWhenSourceIsDisabled` determines if the coroutine should be suspended while the object is not enabled.
        public static void Start<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false) where T : IEnumerator<ValueYieldInstruction>;
        public static ValueCoroutine StartWithHandle<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false) where T : IEnumerator<ValueYieldInstruction>;
    }
    
    /// You can create your own coroutine managers by using the following types which has methods to intialize coroutines and manage them similar to the static methods of `ValueCorotuine` but as instances of this types.
    public sealed partial class CoroutineManager { }
    /// Wraps a `CoroutineManager` and automatically managed its callbacks.
    public sealed partial class AutomaticCoroutineScheduler : MonoBehaviour { }
    
    /// Provides a configuration for the global coroutine manager.
    /// This object must be stored in the `Resources` folder.
    [CreateAssetMenu(fileName = "Global Coroutines Manager Configuration", menuName = "Enderlook/Coroutines/Global Coroutines Manager Unit")]
    internal sealed class GlobalCoroutinesManagerUnit : ScriptableObject
    {        
        [SerializeField, Min(0), Tooltip("Amount of miliseconds spent in executing poll coroutines.")]
        private int milisecondsExecutedPerFrameOnPoll;

        [SerializeField, Range(0, 1), Tooltip("Percentage of total executions that must be executed on poll coroutines regardless of timeout.")]
        private float minimumPercentOfExecutionsPerFrameOnPoll;
    }
}
```

### Unity Jobs

Helper methods to execute jobs in the Unity Job system.

```cs
namespace Enderlook.Unity.Jobs
{
    /// Helper methods for <see cref="JobHandle"/>.
    public static class JobManager
    {
        /// Enqueues an action to be execute when the job handle <paramref name="jobHandle"/> completes.
        /// Note that this action will not be executed immediately after <c><paramref name="jobHandle"/>.Complete()</c>, but may execute on the current or next frame.
        /// `canCompleteImmediately` if the job handle is already completed, this value determines if the action should run immediately or later (which may be in this or in the next frame).
        public static void OnComplete(this JobHandle jobHandle, Action onJobComplete, bool canCompleteImmediately = true);

        /// Automatically watches the completion of this job handle.
        /// Useful for fire and forget.
        /// Returns the same value.
        public static JobHandle WatchCompletion(this JobHandle jobHandle);
    }
    
    /// Allows to get an awaiter from <see cref="JobHandle"/>.
    public static class JobHandleAwaiterExtension
    {
        /// Wrap a job handle as a task.
        /// The return type can be awaited.
        public static JobHandleAwaiter GetAwaiter(this JobHandle jobHandle);
    }
    
    /// Represent an <see cref="IJob"/> that contains managed data.
    public interface IManagedJob
    {
        /// Action to execute.
        void Execute();
    }
    
    /// Helper methods for <see cref="IManagedJob"/>.
    public static partial class IManagedJobExtensions
    {
        /// Schedules a managed job.
        public static JobHandle Schedule<T>(this T job, JobHandle dependsOn = default) where T : IManagedJob;

        /// Wrap a job into a managed job.
        public static ManagedJob<T> AsManaged<T>(this T job) where T : IJob;

        /// Run a managed job.
        public static void Run<T>(this T job) where T : IManagedJob;
        
        /// Schedules actions.
        public static JobHandle Schedule(this Action job, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1>(this Action<T1> job, T1 p1, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2>(this Action<T1, T2> job, T1 p1, T2 p2, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3>(this Action<T1, T2, T3> job, T1 p1, T2 p2, T3 p3, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> job, T1 p1, T2 p2, T3 p3, T4 p4, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, JobHandle dependsOn = default);
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, JobHandle dependsOn = default);
    }
    
    /// Represent an <see cref="IJobParallelFor"/> that contains managed data.
    public interface IManagedJobParallelFor
    {
        /// Action to execute.
        void Execute(int index);
    }
    
    /// Helper methods for <see cref="IManagedJobParallelFor"/>.
    public static class IManagedJobParallelForExtensions
    {
        /// Schedules a managed job.
        public static JobHandle Schedule<T>(this T job, int arrayLength, int innerLoopBatchCount, JobHandle dependsOn = default) where T : IManagedJobParallelFor;
        
        /// Wrap a job into a managed job.
        public static ManagedJobParallelFor<T> AsManaged<T>(this T job) where T : IJobParallelFor;

        /// Run a managed job.
        public static void Run<T>(this T job, int arrayLength) where T : IManagedJobParallelFor;
    }
}
```