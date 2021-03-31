using Enderlook.Unity.Coroutines;
using Enderlook.Unity.Jobs;

using System.Collections;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity
{
    [AddComponentMenu("")] // Not show in menu
    [DefaultExecutionOrder(int.MaxValue)]
    internal sealed class Manager : MonoBehaviour
    {
        private static bool isExiting;

        public static Manager Shared { get; private set; }

        internal CoroutinesManager CoroutinesManager { get; private set; }

        public int MilisecondsExecutedPerFrameOnPoll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CoroutinesManager.MilisecondsExecutedPerFrameOnPoll;
            set {
                CoroutinesManager manager = CoroutinesManager;
                manager.MilisecondsExecutedPerFrameOnPoll = value;
                CoroutinesManager = manager;
            }
        }

        public float MinimumPercentOfExecutionsPerFrameOnPoll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CoroutinesManager.MinimumPercentOfExecutionsPerFrameOnPoll;
            set {
                CoroutinesManager manager = CoroutinesManager;
                manager.MinimumPercentOfExecutionsPerFrameOnPoll = value;
                CoroutinesManager = manager;
            }
        }

        private static void Initialize()
        {
            GameObject gameObject = new GameObject("Enderlook.Unity.Threading.Manager");
#if UNITY_EDITOR
            gameObject.hideFlags = HideFlags.HideAndDontSave;
#endif
            DontDestroyOnLoad(gameObject);
            Shared = gameObject.AddComponent<Manager>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize1()
        {
#if UNITY_EDITOR
            if (isExiting)
                return;
#endif
            if (Shared == null)
            {
                GameObject gameObject = new GameObject("Enderlook.Unity.Threading.Manager");
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
                CoroutinesManager = CoroutinesManager.Create(this);
                StartCoroutine(Work());
                IEnumerator Work()
                {
                    while (true)
                    {
                        yield return Wait.ForEndOfFrame;
                        CoroutinesManager.OnEndOfFrame();
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
            CoroutinesManager.Dispose();
            Initialize();
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
            JobManager.Update();
            CoroutinesManager.OnUpdate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void LateUpdate()
        {
            CoroutinesManager.OnLateUpdate();
            CoroutinesManager.OnPoll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate() => CoroutinesManager.OnFixedUpdate();
    }
}
