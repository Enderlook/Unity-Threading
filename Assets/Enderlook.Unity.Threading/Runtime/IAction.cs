namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// This interface is used to create delegate-like objects.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Execute the stored action.
        /// </summary>
        void Invoke();
    }
}