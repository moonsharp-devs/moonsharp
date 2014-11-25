using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.CoreLib.IO
{
	class FileUserData : FileUserDataBase
	{
		Stream m_Stream;
		StreamReader m_Reader;
		StreamWriter m_Writer;
		bool m_Closed = false;

		public FileUserData(string filename, Encoding encoding, string mode)
		{
			m_Stream = new FileStream(filename, ParseFileMode(mode));

			if (m_Stream.CanRead)
				m_Reader = new StreamReader(m_Stream, encoding);

			if (m_Stream.CanWrite)
				m_Writer = new StreamWriter(m_Stream, encoding);
		}

		private FileMode ParseFileMode(string mode)
		{
			mode = mode.Replace("b", "");

			if (mode == "r")
				return FileMode.Open;
			else if (mode == "r+")
				return FileMode.OpenOrCreate;
			else if (mode == "w")
				return FileMode.Create;
			else if (mode == "w+")
				return FileMode.Truncate;
			else
				return FileMode.Append;
		}

		protected override bool Eof()
		{
			if (m_Reader != null)
				return m_Reader.EndOfStream;
			else
				return false;
		}

		protected override string ReadLine()
		{
			return m_Reader.ReadLine();
		}

		protected override string ReadToEnd()
		{
			return m_Reader.ReadToEnd();
		}

		protected override string ReadBuffer(int p)
		{
			char[] buffer = new char[p];
			int length = m_Reader.ReadBlock(buffer, 0, p);
			return new string(buffer, 0, length);
		}

		protected override char Peek()
		{
			return (char)m_Reader.Peek();
		}

		protected override void Write(string value)
		{
			m_Writer.Write(value);
		}

		public override void close()
		{
			if (m_Reader != null)
				m_Reader.Dispose();

			if (m_Writer != null)
				m_Writer.Dispose();

			m_Stream.Dispose();

			m_Closed = true;
		}

		public override void flush()
		{
			if (m_Writer != null)
				m_Writer.Flush();
		}

		public override void lines()
		{
			throw new NotImplementedException();
		}

		public override long seek(string whence, long offset)
		{
			if (whence != null)
			{
				if (whence == "set")
				{
					m_Stream.Seek(offset, SeekOrigin.Begin);
				}
				else if (whence == "cur")
				{
					m_Stream.Seek(offset, SeekOrigin.Current);
				}
				else if (whence == "end")
				{
					m_Stream.Seek(offset, SeekOrigin.End);
				}
				else
				{
					return -1;
				}
			}

			return m_Stream.Position;
		}

		public override void setvbuf(string mode, int size)
		{
			m_Writer.AutoFlush = (mode == "no" || mode == "line");
		}

		protected internal override bool isopen()
		{
			return !m_Closed;
		}
	}
}
