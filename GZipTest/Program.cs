using System;
using System.Collections.Generic;
using System.IO;

namespace GZipTest
{
    public class Program
    {
        static GZipAbstract gzipabstract;

        static int Main(string[] args)
        {
            try
            {
                // валидация входных параметров 
                Validation(args);

                //создаем объект производного класса
                if (args[0].ToLower() == "compress")
                    gzipabstract = new Compression(args[1], args[2]);
                else
                    gzipabstract = new Decompression(args[1], args[2]);
                gzipabstract.Execute();
                // далее выполняем код из Compression.cs | Decompression.cs
                // когда код исполнен, возвращаем значение returnValue (1-выполнено, 0-не выполнено)
                return gzipabstract.ReturnValue();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("ERROR: " + Ex.Message);
                return 1;
            }
        }

        private static void Validation(string[] args)
        {
            // если число указанных аргументов не равно 3
            if (args.Length != 3)
                throw new ArgumentException(string.Format("Wrong number of arguments: operation, source file, result file."));

            // если была неверно указана операция
            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
                throw new ArgumentException(string.Format("Operation {0} is invalid", args[0]));

            // если исходного файла не существует
            if (!File.Exists(args[1]))
                throw new ArgumentException("File {0} doesn't exist", args[1]);

            // если у исходного и конечного файла одинаковые имена  
            if (args[1] == args[2])
                throw new ArgumentException("Input and output files have same names");

            FileInfo fileIn = new FileInfo(args[1]);
            FileInfo fileOut = new FileInfo(args[2]);

            // если исходны файл весит 0 байт
            if (fileIn.Length == 0)
                throw new ArgumentException("Program cannot work with 0 byte size files ({0}).", "args[1]");

            // если файл уже был сжат 
            if (fileIn.Extension == ".gz" && args[0].ToLower() == "compress")
                throw new ArgumentException("File {0} has already compressed", args[1]);

            // если неверно указано расширение архива
            if (fileOut.Extension != ".gz" && args[0].ToLower() == "compress")
                throw new ArgumentException("File {0} should have .gz extention", args[2]);

            // если файл .gz весит меньше 13 байт (12 байт на сигнатуру)
            if (fileIn.Length < 13 && args[0] == "decompress")
                throw new Exception("Minimal file size to decompress = 13 byte");
        }
    }
}