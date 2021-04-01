using System;
using System.Collections;
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
        public static event Action<Manager> ToInitialize;

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
                        Action<Manager> action = Interlocked.Exchange(ref ToInitialize, null);
                        action(this);
                        OnEndOfFrame();
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
        private void Update()
        {
            Action<Manager> action = Interlocked.Exchange(ref ToInitialize, null);
            action(this);
            OnUpdate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void LateUpdate()
        {
            Action<Manager> action = Interlocked.Exchange(ref ToInitialize, null);
            action(this);
            OnLateUpdate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            Action<Manager> action = Interlocked.Exchange(ref ToInitialize, null);
            action(this);
            OnFixedUpdate();
        }
    }
}
