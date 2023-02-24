using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamingLib
{
    public class ByteStream
    {
        private int m_nOffset;
        private int m_nLength;
        private byte[] m_stream;

        public ByteStream()
        {
            m_nLength = 0;
            m_nOffset = 0;
            m_stream = new byte[1024];
        }

        public ByteStream(byte[] b)
        {
            m_nLength = b.Length;
            m_nOffset = 0;
            m_stream = new byte[b.Length];
            for(int i = 0 ; i < m_stream.Length; i++)
                m_stream[i] = b[i];
        }

        public byte[] getRawArray()
        {
            byte[] b = new byte[m_nOffset];

            for (int i = 0; i < m_nOffset; i++)
                b[i] = m_stream[i];

            return b;
        }

        private void Grow(int nLen)
        {
            byte[] nb = new byte[m_stream.Length + nLen];
            for(int i = 0 ; i < m_stream.Length; i++)
                nb[i] = m_stream[i];

            m_stream = nb;
        }

        public void AddInt(int val)
        {
            if (m_nOffset + 4 > m_stream.Length)
                Grow(32);

            // big endian
            m_stream[m_nOffset++] = (byte)(val >> 24);
            m_stream[m_nOffset++] = (byte)(val >> 16);
            m_stream[m_nOffset++] = (byte)(val >> 8);
            m_stream[m_nOffset++] = (byte)(val);
        }

        public void AddString(string s)
        {
            char[] c = s.ToCharArray();

            if (m_nOffset + c.Length > m_stream.Length)
                Grow(c.Length + 32);

            AddInt(c.Length);

            for(int i = 0; i < c.Length; i++)
                m_stream[m_nOffset++] = (byte)(c[i]);
        }

        public void AddByteArray(byte[] b)
        {
            if (m_nOffset + b.Length > m_stream.Length)
                Grow(b.Length + 32);

            AddInt(b.Length);

            for (int i = 0; i < b.Length; i++)
                m_stream[m_nOffset++] = (byte)(b[i]);
        }

        public int GetIntFromStream()
        {
            int val = 0;
            for (int i = 0; i < 4; i++)
            {
                if (i != 0) val <<= 8;
                val += (int)m_stream[m_nOffset++];
            }

            return val;
        }

        public string GetStringFromStream()
        {
            string s = "";
            int val = GetIntFromStream();

            for (int i = 0; i < val; i++)
            {
                s += (char)m_stream[m_nOffset++];
            }

            return s;
        }

        public byte[] GetByteArrayFromStream()
        {
            int val = GetIntFromStream();
            byte[] b = new byte[val];

            for (int i = 0; i < val; i++)
            {
                b[i] = m_stream[m_nOffset++];
            }

            return b;
        }
    }
}
