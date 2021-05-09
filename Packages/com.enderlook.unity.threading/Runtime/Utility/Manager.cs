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

        public static event Action OnUpdate;
        public static event Action OnFixedUpdate;
        public static event Action OnLateUpdate;
        public static event Action OnEndOfFrame;
        private static event Action onInitialize;

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
                        OnEndOfFrame?.Invoke();
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (isExiting)
                return;
#endif
            Shared = null;
            Debug.LogError($"{nameof(Manager)} should not be destroyed. This has triggered undefined behaviour.", this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDisable()
        {
#if UNITY_EDITOR
            if (isExiting)
                return;
#endif
            gameObject.SetActive(true);
            enabled = true;
            Debug.LogError($"{nameof(Manager)} should not be disabled. This has triggered undefined behaviour.", this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update() => OnUpdate?.Invoke();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void LateUpdate() => OnLateUpdate?.Invoke();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate() => OnFixedUpdate?.Invoke();

        public static void OnInitialized(Action manager)
        {
            if (manager is null)
                ThrowArgumenNullException(manager);

            if (Shared != null)
                manager();
            else
                onInitialize += manager;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumenNullException(Action manager) => throw new ArgumentNullException(nameof(manager));
    }
}
