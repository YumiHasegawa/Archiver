using System;

namespace GZipTest
{
    // объект доступен для сериализации
    [Serializable]

    class ByteBlocks
    {
        // конструктор
        public ByteBlocks() : this(0, new byte[0])
        {
        }

        int id;
        byte[] buffer;

        public int getId { 
            get { return id; } 
        }
        public byte[] getBuffer { 
            get { return buffer; } 
        }

        public ByteBlocks(int id, byte[] buffer)
        {
            this.id = id;
            this.buffer = buffer;
        }
    }
}
