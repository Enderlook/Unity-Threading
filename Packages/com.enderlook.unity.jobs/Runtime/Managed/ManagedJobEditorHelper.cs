using Enderlook.Unity.Threading;

using System;
using System.Collections.Generic;
using System.Threading;

namespace Enderlook.Unity.Jobs
{
#if UNITY_EDITOR
    internal static class ManagedJobEditorHelper
    {
        private static readonly SortedSet<EditorPoolContainer> containers = new SortedSet<EditorPoolContainer>();
        private static int containersLock;

        internal static void AddPoolContainer(string name, Func<int> count)
        {
            while (Interlocked.Exchange(ref containersLock, 1) != 0) ;
            containers.Add(new EditorPoolContainer(name, count));
            containersLock = 0;
        }
    }
#endif
}