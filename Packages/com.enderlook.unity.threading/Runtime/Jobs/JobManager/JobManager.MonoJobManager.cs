using UnityEngine;

namespace Enderlook.Unity.Threading.Jobs
{
    public static partial class JobManager
    {
        [DefaultExecutionOrder(int.MaxValue)]
        private sealed class MonoJobManager : MonoBehaviour
        {
            private static MonoJobManager instance;

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Initialize()
            {
                if (instance == null)
                {
                    GameObject gameObject = new GameObject("JobManager");
                    DontDestroyOnLoad(gameObject);
                    instance = gameObject.AddComponent<MonoJobManager>();
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void Awake()
            {
                if (instance != this)
                {
                    Debug.LogError(nameof(MonoJobManager) + " should not be added manually.");
                    Destroy(this);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void Update() => JobManager.Update();
        }
    }
}
