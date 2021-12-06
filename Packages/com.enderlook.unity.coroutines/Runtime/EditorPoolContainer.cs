using System;

namespace Enderlook.Unity.Threading
{
#if UNITY_EDITOR
    /// <summary>
    /// Unity Editor Only.
    /// </summary>
    internal struct EditorPoolContainer : IComparable<EditorPoolContainer>
    {
        private readonly string Name;
        private readonly Func<int> count;
        private int currentCount;
        private string countString;

        public EditorPoolContainer(string name, Func<int> count)
        {
            Name = name;
            this.count = count;
            int c = count();
            currentCount = c;
            countString = c.ToString();
        }

        public void Get(out string name, out string count)
        {
            name = Name;
            int c = this.count();
            if (currentCount != c)
            {
                currentCount = c;
                countString = c.ToString();
            }
            count = countString;
        }

        int IComparable<EditorPoolContainer>.CompareTo(EditorPoolContainer other) => Name.CompareTo(other.Name);
    }
#endif
}