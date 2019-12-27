using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    abstract class GZipAbstract
    {
        protected static int returnValue = 1;
        protected static int bufferSize = 64 * 1024;
        protected Queue readQueue = new Queue();
        protected Queue writeQueue = new Queue();

        //исходный и создаваемый файлы
        protected static string sourceFile;
        protected static string resultFile;

        // Compress/Decompress потоки в зависимости от ПК
        protected static int threads = (Environment.ProcessorCount - 2) > 0 ? Environment.ProcessorCount - 2 : 1;
        // События для каждого потока
        protected static ManualResetEvent[] exitThread = new ManualResetEvent[threads];

        public GZipAbstract(string input, string output)
        {
            sourceFile = input;
            resultFile = output;
        }

        public int ReturnValue()
        {
            return returnValue;
        }

        abstract public void Execute();
        abstract protected void Read();
        abstract protected void Write();
    }
}

