using System;
using System.Text;
using Java.Net;
using Java.Nio;

namespace XamarinAndroidVPNExample.VPNService
{
    public class Packet : Java.Lang.Object
    {
        public const int IP4_HEADER_SIZE = 20;
        public const int TCP_HEADER_SIZE = 20;
        public const int UDP_HEADER_SIZE = 8;

        public IP4Header ip4Header;
        public TCPHeader tcpHeader;
        public UDPHeader udpHeader;
        public ByteBuffer backingBuffer;

        public Packet(ByteBuffer buffer)
        {
            try
            {
                this.ip4Header = new IP4Header(buffer);
                if (this.ip4Header.protocol == IP4Header.TransportProtocol.TCP)
                {
                    this.tcpHeader = new TCPHeader(buffer);
                    this.IsTCP = true;
                }
                else if (ip4Header.protocol == IP4Header.TransportProtocol.UDP)
                {
                    this.udpHeader = new UDPHeader(buffer);
                    this.IsUDP = true;
                }
                this.backingBuffer = buffer;
            }
            catch (Exception ex)
            {
                throw new UnknownHostException();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Packet{");
            sb.Append("ip4Header=").Append(ip4Header);
            if (IsTCP) sb.Append(", tcpHeader=").Append(tcpHeader);
            else if (IsUDP) sb.Append(", udpHeader=").Append(udpHeader);
            sb.Append(", payloadSize=").Append(backingBuffer.Limit() - backingBuffer.Position());
            sb.Append('}');
            return sb.ToString();
        }

        public bool IsTCP { get; set; }

        public bool IsUDP { get; set; }

        public void SwapSourceAndDestination()
        {
            InetAddress newSourceAddress = ip4Header.destinationAddress;
            ip4Header.destinationAddress = ip4Header.sourceAddress;
            ip4Header.sourceAddress = newSourceAddress;

            if (IsUDP)
            {
                int newSourcePort = udpHeader.destinationPort;
                udpHeader.destinationPort = udpHeader.sourcePort;
                udpHeader.sourcePort = newSourcePort;
            }
            else if (IsTCP)
            {
                int newSourcePort = tcpHeader.destinationPort;
                tcpHeader.destinationPort = tcpHeader.sourcePort;
                tcpHeader.sourcePort = newSourcePort;
            }
        }

        public void updateTCPBuffer(ByteBuffer buffer, byte flags, long sequenceNum, long ackNum, int payloadSize)
        {
            buffer.Position(0);
            FillHeader(buffer);
            backingBuffer = buffer;

            tcpHeader.flags = flags;
            backingBuffer.Put(IP4_HEADER_SIZE + 13, (sbyte)flags);

            tcpHeader.sequenceNumber = sequenceNum;
            backingBuffer.PutInt(IP4_HEADER_SIZE + 4, (int)sequenceNum);

            tcpHeader.acknowledgementNumber = ackNum;
            backingBuffer.PutInt(IP4_HEADER_SIZE + 8, (int)ackNum);

            // Reset header size, since we don't need options
            byte dataOffset = (byte)(TCP_HEADER_SIZE << 2);
            tcpHeader.dataOffsetAndReserved = dataOffset;
            backingBuffer.Put(IP4_HEADER_SIZE + 12, (sbyte)dataOffset);

            updateTCPChecksum(payloadSize);

            int ip4TotalLength = IP4_HEADER_SIZE + TCP_HEADER_SIZE + payloadSize;
            backingBuffer.PutShort(2, (short)ip4TotalLength);
            ip4Header.totalLength = ip4TotalLength;

            updateIP4Checksum();
        }

        public void updateUDPBuffer(ByteBuffer buffer, int payloadSize)
        {
            buffer.Position(0);
            FillHeader(buffer);
            backingBuffer = buffer;

            int udpTotalLength = UDP_HEADER_SIZE + payloadSize;
            backingBuffer.PutShort(IP4_HEADER_SIZE + 4, (short)udpTotalLength);
            udpHeader.length = udpTotalLength;

            // Disable UDP checksum validation
            backingBuffer.PutShort(IP4_HEADER_SIZE + 6, (short)0);
            udpHeader.checksum = 0;

            int ip4TotalLength = IP4_HEADER_SIZE + udpTotalLength;
            backingBuffer.PutShort(2, (short)ip4TotalLength);
            ip4Header.totalLength = ip4TotalLength;

            updateIP4Checksum();
        }

        private void updateIP4Checksum()
        {
            ByteBuffer buffer = backingBuffer.Duplicate();
            buffer.Position(0);

            // Clear previous checksum
            buffer.PutShort(10, (short)0);

            int ipLength = ip4Header.headerLength;
            int sum = 0;
            while (ipLength > 0)
            {
                sum += BitUtils.GetUnsignedShort(buffer.Short);
                ipLength -= 2;
            }
            while (sum >> 16 > 0)
                sum = (sum & 0xFFFF) + (sum >> 16);

            sum = ~sum;
            ip4Header.headerChecksum = sum;
            backingBuffer.PutShort(10, (short)sum);
        }

        private void updateTCPChecksum(int payloadSize)
        {
            int sum = 0;
            int tcpLength = TCP_HEADER_SIZE + payloadSize;

            // Calculate pseudo-header checksum
            ByteBuffer buffer = ByteBuffer.Wrap(ip4Header.sourceAddress.GetAddress());
            sum = BitUtils.GetUnsignedShort(buffer.Short) + BitUtils.GetUnsignedShort(buffer.Short);

            buffer = ByteBuffer.Wrap(ip4Header.destinationAddress.GetAddress());
            sum += BitUtils.GetUnsignedShort(buffer.Short) + BitUtils.GetUnsignedShort(buffer.Short);

            sum += ((int)IP4Header.TransportProtocol.TCP) + tcpLength;

            buffer = backingBuffer.Duplicate();
            // Clear previous checksum
            buffer.PutShort(IP4_HEADER_SIZE + 16, (short)0);

            // Calculate TCP segment checksum
            buffer.Position(IP4_HEADER_SIZE);
            while (tcpLength > 1)
            {
                sum += BitUtils.GetUnsignedShort(buffer.Short);
                tcpLength -= 2;
            }
            if (tcpLength > 0)
                sum += BitUtils.GetUnsignedByte((byte)buffer.Get()) << 8;

            while (sum >> 16 > 0)
                sum = (sum & 0xFFFF) + (sum >> 16);

            sum = ~sum;
            tcpHeader.checksum = sum;
            backingBuffer.PutShort(IP4_HEADER_SIZE + 16, (short)sum);
        }

        private void FillHeader(ByteBuffer buffer)
        {
            ip4Header.FillHeader(buffer);
            if (IsUDP)
                udpHeader.FillHeader(buffer);
            else if (IsTCP)
                tcpHeader.FillHeader(buffer);
        }

        public class IP4Header
        {
            public byte version;
            public byte IHL;
            public int headerLength;
            public short typeOfService;
            public int totalLength;

            public int identificationAndFlagsAndFragmentOffset;

            public short TTL;
            private short protocolNum;
            public TransportProtocol protocol;
            public int headerChecksum;

            public InetAddress sourceAddress;
            public InetAddress destinationAddress;

            public int optionsAndPadding;

            public enum TransportProtocol
            {
                TCP = 6,
                UDP = 17,
                Other = 0xFF
            }

            public IP4Header(ByteBuffer buffer)
            {
                try
                {
                    byte versionAndIHL = (byte)buffer.Get();
                    this.version = (byte)(versionAndIHL >> 4);
                    this.IHL = (byte)(versionAndIHL & 0x0F);
                    this.headerLength = this.IHL << 2;

                    this.typeOfService = BitUtils.GetUnsignedByte((byte)buffer.Get());
                    this.totalLength = BitUtils.GetUnsignedShort(buffer.Short);

                    this.identificationAndFlagsAndFragmentOffset = buffer.Int;

                    this.TTL = BitUtils.GetUnsignedByte((byte)buffer.Get());
                    this.protocolNum = BitUtils.GetUnsignedByte((byte)buffer.Get());
                    this.protocol = (TransportProtocol)protocolNum;
                    this.headerChecksum = BitUtils.GetUnsignedShort(buffer.Short);

                    byte[] addressBytes = new byte[4];
                    buffer.Get(addressBytes, 0, 4);
                    this.sourceAddress = InetAddress.GetByAddress(addressBytes);

                    buffer.Get(addressBytes, 0, 4);
                    this.destinationAddress = InetAddress.GetByAddress(addressBytes);

                    //this.optionsAndPadding = buffer.getInt();
                }
                catch
                {
                    throw new UnknownHostException();
                }
            }

            public int GetNumber()
            {
                return this.protocolNum;
            }

            public void FillHeader(ByteBuffer buffer)
            {
                buffer.Put((sbyte)(this.version << 4 | this.IHL));
                buffer.Put((sbyte)this.typeOfService);
                buffer.PutShort((short)this.totalLength);

                buffer.PutInt(this.identificationAndFlagsAndFragmentOffset);

                buffer.Put((sbyte)this.TTL);
                buffer.Put((sbyte)this.protocol);
                buffer.PutShort((short)this.headerChecksum);

                buffer.Put(this.sourceAddress.GetAddress());
                buffer.Put(this.destinationAddress.GetAddress());
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder("IP4Header{");
                sb.Append("version=").Append(version);
                sb.Append(", IHL=").Append(IHL);
                sb.Append(", typeOfService=").Append(typeOfService);
                sb.Append(", totalLength=").Append(totalLength);
                sb.Append(", identificationAndFlagsAndFragmentOffset=").Append(identificationAndFlagsAndFragmentOffset);
                sb.Append(", TTL=").Append(TTL);
                sb.Append(", protocol=").Append(protocolNum).Append(":").Append(protocol);
                sb.Append(", headerChecksum=").Append(headerChecksum);
                sb.Append(", sourceAddress=").Append(sourceAddress.HostAddress);
                sb.Append(", destinationAddress=").Append(destinationAddress.HostAddress);
                sb.Append('}');
                return sb.ToString();
            }
        }

        public class TCPHeader
        {
            public const int FIN = 0x01;
            public const int SYN = 0x02;
            public const int RST = 0x04;
            public const int PSH = 0x08;
            public const int ACK = 0x10;
            public const int URG = 0x20;

            public int sourcePort;
            public int destinationPort;

            public long sequenceNumber;
            public long acknowledgementNumber;

            public byte dataOffsetAndReserved;
            public int headerLength;
            public byte flags;
            public int window;

            public int checksum;
            public int urgentPointer;

            public byte[] optionsAndPadding;

            public TCPHeader(ByteBuffer buffer)
            {
                this.sourcePort = BitUtils.GetUnsignedShort(buffer.Short);
                this.destinationPort = BitUtils.GetUnsignedShort(buffer.Short);

                this.sequenceNumber = BitUtils.GetUnsignedInt(buffer.Int);
                this.acknowledgementNumber = BitUtils.GetUnsignedInt(buffer.Int);

                this.dataOffsetAndReserved = (byte)buffer.Get();
                this.headerLength = (this.dataOffsetAndReserved & 0xF0) >> 2;
                this.flags = (byte)buffer.Get();
                this.window = BitUtils.GetUnsignedShort(buffer.Short);

                this.checksum = BitUtils.GetUnsignedShort(buffer.Short);
                this.urgentPointer = BitUtils.GetUnsignedShort(buffer.Short);

                int optionsLength = this.headerLength - TCP_HEADER_SIZE;
                if (optionsLength > 0)
                {
                    optionsAndPadding = new byte[optionsLength];
                    buffer.Get(optionsAndPadding, 0, optionsLength);
                }
            }

            public bool isFIN()
            {
                return (flags & FIN) == FIN;
            }

            public bool isSYN()
            {
                return (flags & SYN) == SYN;
            }

            public bool isRST()
            {
                return (flags & RST) == RST;
            }

            public bool isPSH()
            {
                return (flags & PSH) == PSH;
            }

            public bool isACK()
            {
                return (flags & ACK) == ACK;
            }

            public bool isURG()
            {
                return (flags & URG) == URG;
            }

            public void FillHeader(ByteBuffer buffer)
            {
                buffer.PutShort((short)sourcePort);
                buffer.PutShort((short)destinationPort);

                buffer.PutInt((int)sequenceNumber);
                buffer.PutInt((int)acknowledgementNumber);

                buffer.Put((sbyte)dataOffsetAndReserved);
                buffer.Put((sbyte)flags);
                buffer.PutShort((short)window);

                buffer.PutShort((short)checksum);
                buffer.PutShort((short)urgentPointer);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder("TCPHeader{");
                sb.Append("sourcePort=").Append(sourcePort);
                sb.Append(", destinationPort=").Append(destinationPort);
                sb.Append(", sequenceNumber=").Append(sequenceNumber);
                sb.Append(", acknowledgementNumber=").Append(acknowledgementNumber);
                sb.Append(", headerLength=").Append(headerLength);
                sb.Append(", window=").Append(window);
                sb.Append(", checksum=").Append(checksum);
                sb.Append(", flags=");
                if (isFIN()) sb.Append(" FIN");
                if (isSYN()) sb.Append(" SYN");
                if (isRST()) sb.Append(" RST");
                if (isPSH()) sb.Append(" PSH");
                if (isACK()) sb.Append(" ACK");
                if (isURG()) sb.Append(" URG");
                sb.Append('}');
                return sb.ToString();
            }
        }

        public class UDPHeader
        {
            public int sourcePort;
            public int destinationPort;

            public int length;
            public int checksum;

            public UDPHeader(ByteBuffer buffer)
            {
                this.sourcePort = BitUtils.GetUnsignedShort(buffer.Short);
                this.destinationPort = BitUtils.GetUnsignedShort(buffer.Short);

                this.length = BitUtils.GetUnsignedShort(buffer.Short);
                this.checksum = BitUtils.GetUnsignedShort(buffer.Short);
            }

            public void FillHeader(ByteBuffer buffer)
            {
                buffer.PutShort((short)this.sourcePort);
                buffer.PutShort((short)this.destinationPort);

                buffer.PutShort((short)this.length);
                buffer.PutShort((short)this.checksum);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder("UDPHeader{");
                sb.Append("sourcePort=").Append(sourcePort);
                sb.Append(", destinationPort=").Append(destinationPort);
                sb.Append(", length=").Append(length);
                sb.Append(", checksum=").Append(checksum);
                sb.Append('}');
                return sb.ToString();
            }
        }

        public static class BitUtils
        {
            public static short GetUnsignedByte(byte value)
            {
                return (short)(value & 0xFF);
            }

            public static int GetUnsignedShort(short value)
            {
                return value & 0xFFFF;
            }

            public static long GetUnsignedInt(int value)
            {
                return value & 0xFFFFFFFFL;
            }
        }
    }
}
