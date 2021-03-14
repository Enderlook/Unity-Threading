using UnityEngine;

namespace Enderlook.Unity.Threading.Jobs
{
    [DefaultExecutionOrder(int.MaxValue)]
    internal sealed class Manager : MonoBehaviour
    {
        private static bool isExiting;

        public static Manager Shared { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if UNITY_EDITOR
            if (isExiting)
                return;
#endif
            if (Shared == null)
            {
                GameObject gameObject = new GameObject("Enderlook.Unity.Threading.Manager");
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
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (isExiting)
                return;
#endif
            Shared = null;
            Debug.LogError($"{nameof(Manager)} should not be destroyed.", this);
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
            Debug.LogError($"{nameof(Manager)} should not be disabled.", this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update() => JobManager.Update();
    }
}
