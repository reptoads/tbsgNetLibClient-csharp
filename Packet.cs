using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace tbsgNetLib{

    public class Packet
    {
        public Packet()
        {
            this.bytes.Clear();
            readPos = 0;
        }

        public Packet(byte[] data)
        {
            Create(data);
        }

        public void Write<T>(T data) where T : IComparable<T>
        {
            var byteArray = GetBytes(data);
            Array.Reverse(byteArray, 0, byteArray.Length);
            this.bytes.AddRange(byteArray);
        }
        public void Write(string data)
        {
            var byteArray = Encoding.UTF8.GetBytes(NetUtils.Utf16ToUtf8(data));
            Write((uint)byteArray.Length);
            this.bytes.AddRange(byteArray);
        }

        public void Write(Packet packet)
        {
            this.bytes.AddRange(packet.Bytes);
        }
        public void Write(byte[] data)
        {
            this.bytes.AddRange(data);
        }

        public void Create(byte[] data)
        {
            this.bytes.Clear();
            this.bytes.AddRange(data);
            readPos = 0;
        }


        private int SizeOf<T>()
        {
            if (typeof(T).IsClass)
            {
                return Marshal.SizeOf(typeof(T));
            }else if (typeof(T) == typeof(bool))
            {
                return 1;
            }

            return Marshal.SizeOf(default(T));

        }

        public T Read<T>()
        {
            var size = SizeOf<T>();//Marshal.SizeOf(default(T));
            var range = bytes.GetRange(readPos, size).ToArray();
            Array.Reverse(range, 0, size);
            readPos += size;
            return FromBytes<T>(range);
        }

        public uint ReadUint()
        {
            var size = Marshal.SizeOf(default(uint));
            var range = bytes.GetRange(readPos, size).ToArray();
            Array.Reverse(range,0, size);
            var value = BitConverter.ToUInt32(range, 0);
            readPos += size;
            return value;
        }

        public string ReadString()
        {
            var size = Read<int>();
            char[] chars = new char[size];
            var range = bytes.GetRange(readPos, size).ToArray();
            //Array.Reverse(range, 0, size);
            System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
            int charLen = d.GetChars(range, 0, size, chars, 0);
            System.String result = new System.String(chars);
            readPos += size;
            return NetUtils.Utf8ToUtf16(result);
        }

        private static byte[] GetBytes<T>(T obj)
        {

            int size = Marshal.SizeOf(obj);

            byte[] arr = new byte[size];

            GCHandle h = default(GCHandle);

            try
            {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);

                Marshal.StructureToPtr<T>(obj, h.AddrOfPinnedObject(), false);
            }
            finally
            {
                if (h.IsAllocated)
                {
                    h.Free();
                }
            }
            return arr;
        }

        private static T FromBytes<T>(byte[] arr)
        {
   
            T str;
            GCHandle h = default(GCHandle);

            try
            {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);

                str = Marshal.PtrToStructure<T>(h.AddrOfPinnedObject());

            }
            finally
            {
                if (h.IsAllocated)
                {
                    h.Free();
                }
            }

            return str;
        }

        public byte[] Bytes => bytes.ToArray();

        private List<byte> bytes = new List<byte>();
        private int readPos = 0;
    };
}
