using System.Threading;

namespace Enderlook.Unity.Threading
{
    internal struct ReadWriterLock
    {
        private int locked;
        private int readers;
        private bool reserved;

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
            Lock();
            readers--;
            reserved = true;
            Unlock();

            while (true)
            {
                Lock();
                if (readers > 0)
                    Unlock();
                else
                {
                    reserved = false;
                    break;
                }
            }
        }

        public void WriteBegin()
        {
            while (true)
            {
                Lock();
                if (readers > 0)
                    Unlock();
                else if (reserved)
                    Unlock();
                else
                {
                    reserved = false;
                    break;
                }
            }
        }

        public void WriteEnd() => Unlock();
    }
}