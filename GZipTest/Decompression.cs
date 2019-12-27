using System;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace GZipTest
{
    class Decompression : GZipAbstract
    {
        // конструктор
        public Decompression(string input, string output) : base (input, output)
        {
        }

        public override void Execute()
        {
            Console.WriteLine("\nStarting decompression. Please, wait...\n");

            Thread readThread = new Thread(new ThreadStart(Read));
            readThread.Start();

            Thread[] decompressionThreads = new Thread[threads];
            for (int i = 0; i < threads; i++)
            {
                decompressionThreads[i] = new Thread(new ParameterizedThreadStart(Decompress));
                exitThread[i] = new ManualResetEvent(false);
                decompressionThreads[i].Start(i);
            }

            Thread writeThread = new Thread(new ThreadStart(Write));
            writeThread.Start();

            WaitHandle.WaitAll(exitThread);
            writeQueue.Close();
        }


        protected override void Read()
        {
            ByteBlocks byteblock;
            BinaryFormatter bformatter = new BinaryFormatter();
            using (FileStream sFileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            {
                while (sFileStream.Position < sFileStream.Length)
                {
                    byteblock = (ByteBlocks)bformatter.Deserialize(sFileStream);
                    readQueue.AddToQueue(byteblock);
                }
                readQueue.Close();
            }
        }

        private void Decompress(object threadNumber)
        {
            ByteBlocks byteblock;
            int bytesRead;
            while (readQueue.DeleteFromQueue(out byteblock))
            {
                using (MemoryStream memoryStream = new MemoryStream(byteblock.getBuffer))
                {
                    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[bufferSize];
                        bytesRead = gzipStream.Read(buffer, 0, buffer.Length);
                        byte[] lastBuffer = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, lastBuffer, 0, bytesRead);
                        ByteBlocks newblock = new ByteBlocks(byteblock.getId, lastBuffer);
                        writeQueue.AddToQueue(newblock);
                    }
                }
            }
            exitThread[(int)threadNumber].Set();
        }

        protected override void Write()
        {
            using (FileStream rFileStream = new FileStream(resultFile, FileMode.Create, FileAccess.Write))
            {
                ByteBlocks byteblock;
                while (writeQueue.DeleteFromQueue(out byteblock))
                {
                    byte[] buffer = byteblock.getBuffer;
                    rFileStream.Write(buffer, 0, buffer.Length);
                }
            }
            returnValue = 0;
            Console.WriteLine("Decompression is done.");
        }
    }
}
