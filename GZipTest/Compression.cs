using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.IO.Compression;

namespace GZipTest
{
    class Compression : GZipAbstract
    {
        // конструктор
        public Compression(string input, string output) : base (input, output)
        {
        }

        public override void Execute()
        {
            Console.WriteLine("\nStarting compression. Please, wait...\n");
            
            // запускаем тред чтения исходного файла
            Thread readThread = new Thread(new ThreadStart(Read));
            readThread.Start();

            // запускаем потоки для компрессии считанных данных
            Thread[] compressionThreads = new Thread[threads];
            for (int i = 0; i < threads; i++)
            {
                compressionThreads[i] = new Thread(new ParameterizedThreadStart(Compress));
                exitThread[i] = new ManualResetEvent(false);
                compressionThreads[i].Start(i);
            }

            // запускаем тред записи в конечный файл
            Thread writeThread = new Thread(new ThreadStart(Write));
            writeThread.Start();

            /* Работа основного потока (самой программы) приостанавливается до тех пор, 
               пока треды компрессии не перейдут в сигнальное состояние. */
            WaitHandle.WaitAll(exitThread);

            /* Когда треды компрессии закончили работу (перешли в сигнальное состояние),
               это означает, что компрессия закончена и очередь записи можно закрыть. */
            writeQueue.Close();
            // далее ожидаем завершения работы треда записи
        }

        protected override void Read()
        {
            int bytesRead;
            byte[] buffer = new byte[bufferSize];
            ByteBlocks byteblock;
            int id = 0;
            using (FileStream input = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            {
                
                while ((bytesRead = input.Read(buffer, 0, bufferSize)) > 0)
                {
                    byte[] lastBuffer = new byte[bytesRead];
                    Buffer.BlockCopy(buffer, 0, lastBuffer, 0, bytesRead);
                    byteblock = new ByteBlocks(id, lastBuffer);
                    readQueue.AddToQueue(byteblock);
                    id++;
                }
                readQueue.Close();
            }
        }

        private void Compress(object threadNumber)
        {
            ByteBlocks byteblock;
            while (readQueue.DeleteFromQueue(out byteblock))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    using (BinaryWriter binaryWriter = new BinaryWriter(gzipStream))
                    {
                        binaryWriter.Write(byteblock.getBuffer, 0, byteblock.getBuffer.Length);
                    }
                    ByteBlocks newblock = new ByteBlocks(byteblock.getId, memoryStream.ToArray());
                    writeQueue.AddToQueue(newblock);
                }
            }
            exitThread[(int)threadNumber].Set();
        }

        protected override void Write()
        {
            using (FileStream rFileStream = new FileStream(resultFile, FileMode.Create, FileAccess.Write))
            {
                ByteBlocks byteblock;
                BinaryFormatter bformatter = new BinaryFormatter();

                while (writeQueue.DeleteFromQueue(out byteblock))
                {
                    bformatter.Serialize(rFileStream, byteblock);
                }
            }
            returnValue = 0;
            Console.WriteLine("Compression is done.");
        }
    }
}

