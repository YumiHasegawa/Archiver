using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    class Queue
    {
        public bool closeQueue = false;
        private int idCount = 0;
        private Queue<ByteBlocks> queueByteBlocks = new Queue<ByteBlocks>();
        private int maxObjects = 30;
        private int queueCount = 0;

        public void Close()
        {
            lock (queueByteBlocks)
            {
                closeQueue = true;
                Monitor.PulseAll(queueByteBlocks);
            }
        }

        public void AddToQueue(ByteBlocks byteblock)
        {
            int id = byteblock.getId;
            lock (queueByteBlocks)
            {
                while (queueCount >= maxObjects || id != idCount)
                {
                    Monitor.Wait(queueByteBlocks);
                }
                queueByteBlocks.Enqueue(byteblock);
                idCount++;
                queueCount++;
                Monitor.PulseAll(queueByteBlocks);
            }
        }

        public bool DeleteFromQueue(out ByteBlocks byteblock)
        {
            lock (queueByteBlocks)
            {
                while (queueCount == 0)
                {
                    if (closeQueue)
                    {
                        byteblock = new ByteBlocks();
                        return false;
                    }
                    Monitor.Wait(queueByteBlocks);
                }
                byteblock = queueByteBlocks.Dequeue();
                queueCount--;
                Monitor.PulseAll(queueByteBlocks);
                return true;
            }
        }
    }
}

