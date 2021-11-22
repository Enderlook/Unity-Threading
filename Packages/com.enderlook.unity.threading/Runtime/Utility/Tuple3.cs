using Enderlook.Pools;

namespace Enderlook.Unity.Threading
{
    internal sealed class Tuple3<T1, T2, T3>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;

        public static Tuple3<T1, T2, T3> Rent(T1 item1, T2 item2, T3 item3)
        {
            Tuple3<T1, T2, T3> tuple = ObjectPool<Tuple3<T1, T2, T3>>.Shared.Rent();
            tuple.Item1 = item1;
            tuple.Item2 = item2;
            tuple.Item3 = item3;
            return tuple;
        }

        public static Tuple3<T1, T2, T3> Rent(T1 item1, T2 item2)
        {
            Tuple3<T1, T2, T3> tuple = ObjectPool<Tuple3<T1, T2, T3>>.Shared.Rent();
            tuple.Item1 = item1;
            tuple.Item2 = item2;
            return tuple;
        }

        public void Return()
        {
            Item1 = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>.
            Item2 = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>.
            Item3 = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>.
            ObjectPool<Tuple3<T1, T2, T3>>.Shared.Return(this);
        }
    }
}
