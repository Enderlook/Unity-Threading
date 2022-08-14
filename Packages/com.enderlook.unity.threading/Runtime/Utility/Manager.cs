using Enderlook.Collections.LowLevel;

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    [AddComponentMenu("")] // Not show in menu
    [DefaultExecutionOrder(int.MaxValue)]
    internal sealed class Manager : MonoBehaviour
    {
        private static bool isExiting;

        public static Manager Shared { get; private set; }

        private static Pack onUpdate = Pack.Create();
        public static event Action OnUpdate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => onUpdate.Add(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => onUpdate.Remove(value);
        }

        private static Pack onFixedUpdate = Pack.Create();
        public static event Action OnFixedUpdate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => onFixedUpdate.Add(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => onFixedUpdate.Remove(value);
        }

        private static Pack onLateUpdate = Pack.Create();
        public static event Action OnLateUpdate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => onLateUpdate.Add(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => onLateUpdate.Remove(value);
        }

        private static Pack onEndOfFrame = Pack.Create();
        public static event Action OnEndOfFrame {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => onEndOfFrame.Add(value);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => onEndOfFrame.Remove(value);
        }

        private static event Action onInitialize;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Lock(ref int @lock)
        {
            while (Interlocked.Exchange(ref @lock, 1) == 1) ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Unlock(ref int @lock) => @lock = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize1()
        {
#if UNITY_EDITOR
            if (isExiting)
                return;
#endif
            if (Shared == null)
            {
                GameObject gameObject = new GameObject("Enderlook.Unity.Scheduling.Manager");
#if UNITY_EDITOR
                gameObject.hideFlags = HideFlags.HideAndDontSave;
#endif
                DontDestroyOnLoad(gameObject);
                Shared = gameObject.AddComponent<Manager>();

                Action action = Interlocked.Exchange(ref onInitialize, null);
                action?.Invoke();
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void Initialize2()
        {
            isExiting = false;
            UnityEditor.EditorApplication.playModeStateChanged +=
                (UnityEditor.PlayModeStateChange playModeState) => isExiting = playModeState == UnityEditor.PlayModeStateChange.ExitingPlayMode;
        }
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (Shared != null && Shared != this)
            {
                Debug.LogError(nameof(Manager) + " should not be added manually.", this);
                Destroy(this);
            }
            else
            {
                WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
                StartCoroutine(Work());
                IEnumerator Work()
                {
                    while (true)
                    {
                        yield return endOfFrame;
                        onEndOfFrame.Execute();
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnApplicationQuit() => isExiting = true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDestroy()
        {
            if (isExiting)
                return;
            Shared = null;
            Debug.LogError($"{nameof(Manager)} should not be destroyed. This has triggered undefined behaviour.", this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDisable()
        {
            if (isExiting)
                return;

            gameObject.SetActive(true);
            enabled = true;
            Debug.LogError($"{nameof(Manager)} should not be disabled. This has triggered undefined behaviour.", this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update() => onUpdate.Execute();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void LateUpdate() => onLateUpdate.Execute();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate() => onFixedUpdate.Execute();

        public static void OnInitialized(Action callback)
        {
            if (callback is null)
                ThrowArgumenNullException();

            if (Shared != null)
                callback();
            else
                onInitialize += callback;
        }

        private static void ThrowArgumenNullException() => throw new ArgumentNullException("callback");

        private struct Pack
        {
            private RawList<Action> list;
            private int toAddLock;
            private RawList<Action> toAdd;
            private int toRemoveLock;
            private RawList<Action> toRemove;

            private Pack(RawList<Action> list, RawList<Action> toAdd, RawList<Action> toRemove)
            {
                this.list = list;
                this.toAdd = toAdd;
                this.toRemove = toRemove;
                toAddLock = 0;
                toRemoveLock = 0;
            }

            public static Pack Create()
                => new Pack(RawList<Action>.Create(), RawList<Action>.Create(), RawList<Action>.Create());

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Execute()
            {
                Lock(ref toAddLock);
                {
                    this.list.AddRange(toAdd.AsSpan());
                    toAdd.Clear();
                }
                Unlock(ref toAddLock);

                Span<Action> list = this.list.AsSpan();

                Lock(ref toRemoveLock);
                {
                    Span<Action> toRemove = this.toRemove.AsSpan();
                    int toRemoveCount = toRemove.Length;
                    if (toRemoveCount > 0)
                    {
                        int j = 0;
                        for (int i = 0; i < list.Length; i++)
                        {
                            Action element = list[i];
                            for (int k = 0; k < toRemoveCount; k++)
                            {
                                if (element == toRemove[k])
                                {
                                    if (toRemoveCount > 1)
                                    {
                                        toRemove[k] = toRemove[--toRemoveCount];
                                        goto continue_;
                                    }
                                    else
                                    {
                                        list.Slice(i).CopyTo(list.Slice(j));
                                        goto double_break;
                                    }
                                }
                            }
                            list[j++] = element;
                        continue_:;
                        }

                    double_break:
                        RawList<Action> list_ = RawList<Action>.From(this.list.UnderlyingArray, j);
                        this.list = list_;
                        list = list_.AsSpan();
                        this.toRemove.Clear();
                    }
                }
                Unlock(ref toRemoveLock);

                for (int i = 0; i < list.Length; i++)
                    list[i]();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(Action value)
            {
                if (value is null)
                    return;

                Lock(ref toAddLock);
                {
                    toAdd.Add(value);
                }
                Unlock(ref toAddLock);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Remove(Action value)
            {
                if (value is null)
                    return;

                Lock(ref toRemoveLock);
                {
                    toRemove.Add(value);
                }
                Unlock(ref toRemoveLock);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Lock(ref int @lock)
            {
                while (Interlocked.Exchange(ref @lock, 1) == 1) ;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Unlock(ref int @lock) => @lock = 0;
        }
    }
}
