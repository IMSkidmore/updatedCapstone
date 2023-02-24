using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;

namespace StreamingLib
{
    public class SerialPipeMessage
    {
        public enum Type
        {
            None,
            PollingString,
            CommandString,
            BinaryData,
            OpenPort,
            ReplyString,
            ReplyTimeout,
            OpenError,
            OpenSuccess,
            ByeBye,
            FixedBinaryData,
            GetStreamingBinaryData,
        };

        public Type m_msgType;
        public int m_nTimeOut;
        public string m_s;
        public byte[] m_b;
        public bool m_bIsReply;

        public SerialPipeMessage()
        {
            m_msgType = Type.None;
            m_nTimeOut = 0;
            m_s = null;
            m_b = null;
            m_bIsReply = false;
        }

        public SerialPipeMessage(string s, bool bPoll, int timeout)
        {
            if (bPoll)
                m_msgType = Type.PollingString;
            else
                m_msgType = Type.CommandString;
            m_s = s;
            m_nTimeOut = timeout;
            m_bIsReply = false;
        }

        public SerialPipeMessage(byte[] b, int len, int timeout)
        {
            m_msgType = Type.BinaryData;
            m_b = new byte[len];
            for (int i = 0; i < len; i++)
            {
                m_b[i] = b[i];
            }
            m_nTimeOut = timeout;
            m_bIsReply = false;
        }

        public SerialPipeMessage(Type t, byte[] b, int len, int timeout)
        {
            m_msgType = t;
            m_b = new byte[len];
            for (int i = 0; i < len; i++)
            {
                m_b[i] = b[i];
            }
            m_nTimeOut = timeout;
            m_bIsReply = false;
        }

        public SerialPipeMessage(Type t, string s)
        {
            m_msgType = t;
            m_s = s;
            m_bIsReply = true;
        }

        public byte[] Marshall()
        {
            ByteStream b = new ByteStream();

            b.AddInt((int)m_msgType);
            b.AddInt(m_nTimeOut);
            if (m_bIsReply)
                b.AddInt(1);
            else
                b.AddInt(0);

            if (m_msgType != Type.ReplyTimeout)
            {
                if ((m_msgType == Type.BinaryData) ||
                    (m_msgType == Type.FixedBinaryData) ||
                    (m_msgType == Type.GetStreamingBinaryData))
                    b.AddByteArray(m_b);
                else
                    b.AddString(m_s);
            }
            return b.getRawArray();
        }

        static public SerialPipeMessage UnMarshall(byte[] b)
        {
            ByteStream bs = new ByteStream(b);

            SerialPipeMessage msg = new SerialPipeMessage();
            msg.m_msgType = (Type)bs.GetIntFromStream();
            msg.m_nTimeOut = bs.GetIntFromStream();
            msg.m_bIsReply = (bs.GetIntFromStream() != 0);
            if (msg.m_msgType != Type.ReplyTimeout)
            {
                if ((msg.m_msgType == Type.BinaryData) ||
                    (msg.m_msgType == Type.FixedBinaryData) ||
                    (msg.m_msgType == Type.GetStreamingBinaryData))
                    msg.m_b = bs.GetByteArrayFromStream();
                else
                    msg.m_s = bs.GetStringFromStream();
            }
            return msg;
        }

        public Type MsgType
        {
            get
            {
                return m_msgType;
            }
        }

        public string StringData
        {
            get
            {
                return m_s;
            }
            set
            {
                m_s = value;
            }
        }

        public static SerialPipeMessage WaitForMsg(NamedPipeClientStream pipeStream)
        {
            byte[] b = new byte[4096];

            int numBytes = pipeStream.Read(b, 0, b.Length);
            if (pipeStream.IsMessageComplete)
            {
                SerialPipeMessage msg = SerialPipeMessage.UnMarshall(b);
                return msg;
            }
            return null;
        }

        public static void SendReply(NamedPipeClientStream pipeStream, SerialPipeMessage.Type t, string s)
        {
            SerialPipeMessage outmsg = new SerialPipeMessage(t, s);
            byte[] outb = outmsg.Marshall();
            pipeStream.Write(outb, 0, outb.Length);
        }

        public static SerialPipeMessage WaitForMsg(NamedPipeServerStream pipeStream)
        {
            byte[] b = new byte[4096];

            int numBytes = pipeStream.Read(b, 0, b.Length);
            if (pipeStream.IsMessageComplete)
            {
                SerialPipeMessage msg = SerialPipeMessage.UnMarshall(b);
                return msg;
            }
            return null;
        }

        public static SerialPipeMessage CheckForMsg(NamedPipeServerStream pipeStream)
        {
            byte[] b = new byte[4096];

            pipeStream.ReadTimeout = 500;

            int numBytes = pipeStream.Read(b, 0, b.Length);
            if (pipeStream.IsMessageComplete)
            {
                SerialPipeMessage msg = SerialPipeMessage.UnMarshall(b);
                return msg;
            }
            return null;
        }
    }
}
