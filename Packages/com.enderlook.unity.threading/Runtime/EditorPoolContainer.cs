using System;

namespace Enderlook.Unity
{
#if UNITY_EDITOR
    /// <summary>
    /// Unity Editor Only.
    /// </summary>
    internal struct EditorPoolContainer : IComparable<EditorPoolContainer>
    {
        private readonly string Name;
        private readonly Func<int> count;
        private readonly Action clear;
        private int currentCount;
        private string countString;

        public EditorPoolContainer(string name, Func<int> count, Action clear)
        {
            Name = name;
            this.count = count;
            this.clear = clear;
            int c = count();
            currentCount = c;
            countString = c.ToString();
        }

        public void Get(out string name, out Action clear, out string count)
        {
            name = Name;
            clear = this.clear;
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