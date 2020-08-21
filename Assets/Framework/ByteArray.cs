
using System;

namespace Part3
{
	public class ByteArray
	{
		public const int DEFAULT_SIZE = 1024;
		int initSize = 0;
		public byte[] bytes;
		public int readIndex = 0;
		public int WriteIndex = 0;
		private int capacity = 0;
		public int remain { get { return capacity - WriteIndex; } }
		public int length { get { return WriteIndex - readIndex; } }

		//构造函数
		public ByteArray(int size = DEFAULT_SIZE)
		{
			bytes = new byte[size];
			capacity = size;
			initSize = size;
			readIndex = 0;
			WriteIndex = 0;
		}

		//构造函数
		public ByteArray(byte[] defaultBytes)
		{
			bytes = defaultBytes;
			capacity = defaultBytes.Length;
			initSize = defaultBytes.Length;
			readIndex = 0;
			WriteIndex = defaultBytes.Length;
		}

		//重设尺寸
		public void ReSize(int size)
		{
			if (size < length) return;
			if (size < initSize) return;
			int n = 1;
			while (n < size) n *= 2;
			capacity = n;
			byte[] newBytes = new byte[capacity];
			Array.Copy(bytes, readIndex, newBytes, 0, WriteIndex - readIndex);
			bytes = newBytes;
			WriteIndex = length;
			readIndex = 0;
		}

		//写入数据
		public int Write(byte[] bs, int offset, int count)
		{
			if (remain < count)
			{
				ReSize(length + count);
			}
			Array.Copy(bs, offset, bytes, WriteIndex, count);
			WriteIndex += count;
			return count;
		}

		//读取数据
		public int Read(byte[] bs, int offset, int count)
		{
			count = Math.Min(count, length);
			Array.Copy(bytes, 0, bs, offset, count);
			readIndex += count;
			CheckAndMoveBytes();
			return count;
		}

		//检查并移动数据
		public void CheckAndMoveBytes()
		{
			if (length < 8)
			{
				MoveBytes();
			}
		}

		//移动数据
		public void MoveBytes()
		{
			Array.Copy(bytes, readIndex, bytes, 0, length);
			WriteIndex = length;
			readIndex = 0;
		}
		//读取Int16
		public Int16 ReadInt16()
		{
			if (length < 2) return 0;
			Int16 ret = BitConverter.ToInt16(bytes, readIndex);
			readIndex += 2;
			CheckAndMoveBytes();
			return ret;
		}

		//读取Int32
		public Int32 ReadInt32()
		{
			if (length < 4) return 0;
			Int32 ret = BitConverter.ToInt32(bytes, readIndex);
			readIndex += 4;
			CheckAndMoveBytes();
			return ret;
		}

		//打印缓冲区
		public override string ToString()
		{
			return BitConverter.ToString(bytes, readIndex, length);
		}

		//打印调试信息
		public string Debug()
		{
			return string.Format("readIdx({0}) writeIdx({1}) bytes({2})",
				readIndex,
				WriteIndex,
				BitConverter.ToString(bytes, 0, capacity)
			);
		}
	}
}

