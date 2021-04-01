using System.Threading;

namespace Enderlook.Unity
{
    internal struct ReadWriterLock
    {
        private int locked;
        private int readers;

        private void Lock()
        {
            while (Interlocked.Exchange(ref locked, 1) != 0) ;
        }

        private void Unlock() => locked = 0;

        public void ReadBegin()
        {
            Lock();
            readers++;
            Unlock();
        }

        public void ReadEnd()
        {
            Lock();
            readers--;
            Unlock();
        }

        public void UpgradeFromReaderToWriter()
        {
            ReadEnd();
            WriteBegin();
        }

        public void WriteBegin()
        {
            while (true)
            {
                Lock();
                if (readers > 0)
                    Unlock();
                else
                    break;
            }
        }

        public void WriteEnd() => Unlock();
    }
}