using Enderlook.Pools;

namespace Enderlook.Unity.Threading
{
    internal sealed class Tuple2<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public static Tuple2<T1, T2> Rent(T1 item1, T2 item2)
        {
            Tuple2<T1, T2> tuple = ObjectPool<Tuple2<T1, T2>>.Shared.Rent();
            tuple.Item1 = item1;
            tuple.Item2 = item2;
            return tuple;
        }

        public static Tuple2<T1, T2> Rent(T1 item1)
        {
            Tuple2<T1, T2> tuple = ObjectPool<Tuple2<T1, T2>>.Shared.Rent();
            tuple.Item1 = item1;
            return tuple;
        }

        public void Return()
        {
            Item1 = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>.
            Item2 = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>.
            ObjectPool<Tuple2<T1, T2>>.Shared.Return(this);
        }
    }
}
