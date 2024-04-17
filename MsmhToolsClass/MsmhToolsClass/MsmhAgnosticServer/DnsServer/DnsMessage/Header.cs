using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class Header
{
    public bool IsSuccess { get; private set; } = false;
    public ushort ID { get; set; } // 2 Bytes
    public DnsEnums.QR QR { get; set; } // 1 Bit / Query = 0 / Response = 1
    public DnsEnums.OperationalCode OperationalCode { get; set; } // 4 Bits / Query Type, 0 = Standard, 1 = Inverse, 2 = Server Status Request, 3-15 = Reserved For Future Use
    public DnsEnums.AA AA { get; set; } // 1 Bit / Authoritative Answer
    public DnsEnums.TC TC { get; set; } // 1 Bit / Truncate, 0 = Not Truncated, 1 = Truncated
    public DnsEnums.RD RD { get; set; } // 1 Bit / Recursion Is Desired, 0 = False, 1 = True (Must Be True For a Query)
    public DnsEnums.RA RA { get; set; } // 1 Bit / Recursion Is Available, 0 = False, 1 = True
    public DnsEnums.Z Z { get; set; } = DnsEnums.Z.Reserved; // 2 Bits / Reserved for future use. Must be zero in all queries and responses
    public DnsEnums.AnswerAuthenticated AnswerAuthenticated { get; set; } // 1 Bit
    public DnsEnums.NonAuthenticatedData NonAuthenticatedData { get; set; } // 1 Bit
    public DnsEnums.ResponseCode ResponseCode { get; set; } // 4 Bits
    public ushort QuestionsCount { get; set; } // 2 Bytes / Number Of Questions
    public ushort AnswersCount { get; set; } // 2 Bytes / Number Of Answers
    public ushort AuthoritiesCount { get; set; } // 2 Bytes / Number Of Authorities
    public ushort AdditionalsCount { get; set; } // 2 Bytes / Number Of Additional Records

    public override string ToString()
    {
        string result = "DNS Header:\n";
        result += $"{nameof(IsSuccess)}: {IsSuccess}\n";
        result += $"{nameof(ID)}: {ID}\n";
        result += $"{nameof(QR)}: {QR}\n";
        result += $"{nameof(OperationalCode)}: {OperationalCode}\n";
        result += $"{nameof(AA)}: {AA}\n";
        result += $"{nameof(TC)}: {TC}\n";
        result += $"{nameof(RD)}: {RD}\n";
        result += $"{nameof(RA)}: {RA}\n";
        result += $"{nameof(AnswerAuthenticated)}: {AnswerAuthenticated}\n";
        result += $"{nameof(NonAuthenticatedData)}: {NonAuthenticatedData}\n";
        result += $"{nameof(ResponseCode)}: {ResponseCode}\n";
        result += $"{nameof(QuestionsCount)}: {QuestionsCount}\n";
        result += $"{nameof(AnswersCount)}: {AnswersCount}\n";
        result += $"{nameof(AuthoritiesCount)}: {AuthoritiesCount}\n";
        result += $"{nameof(AdditionalsCount)}: {AdditionalsCount}";
        return result;
    }

    internal static ushort GenerateId()
    {
        try
        {
            bool idBool = ByteArrayTool.TryConvertBytesToUInt16(Guid.NewGuid().ToByteArray(), out ushort id);
            return idBool ? id : Convert.ToUInt16(12345);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS Header GenerateId: " + ex.Message);
            return 12345;
        }
    }

    public static Header Read(byte[] buffer, ref int pos)
    {
        try
        {
            if (buffer.Length <= pos + MsmhAgnosticServer.DNS_HEADER_LENGTH) return new Header();

            bool idBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort id);
            pos += 2;

            bool convertToBinaryBool = ByteArrayTool.TryConvertToBinary(buffer[pos..(pos + 2)], out string bitsStr);
            bool splitBinaryBool = ByteArrayTool.TrySplitBinary(bitsStr, out bool[] bits);

            pos += 2;
            bool qrBool = CommonTools.TryConvertToEnum(new[] { bits[0] }, out DnsEnums.QR qr);
            bool opCodeBool = CommonTools.TryConvertToEnum(new[] { bits[1], bits[2], bits[3], bits[4] }, out DnsEnums.OperationalCode opCode);
            bool aaBool = CommonTools.TryConvertToEnum(new[] { bits[5] }, out DnsEnums.AA aa);
            bool tcBool = CommonTools.TryConvertToEnum(new[] { bits[6] }, out DnsEnums.TC tc);
            bool rdBool = CommonTools.TryConvertToEnum(new[] { bits[7] }, out DnsEnums.RD rd);
            bool raBool = CommonTools.TryConvertToEnum(new[] { bits[8] }, out DnsEnums.RA ra);
            // 9-10 Reserved
            bool answerAuthenticatedBool = CommonTools.TryConvertToEnum(new[] { bits[11] }, out DnsEnums.AnswerAuthenticated answerAuthenticated);
            bool nonAuthenticatedDataBool = CommonTools.TryConvertToEnum(new[] { bits[12] }, out DnsEnums.NonAuthenticatedData nonAuthenticatedData);
            bool rCodeBool = CommonTools.TryConvertToEnum(new[] { bits[^4], bits[^3], bits[^2], bits[^1] }, out DnsEnums.ResponseCode rCode);

            bool questionsCountBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort questionsCount);
            pos += 2;
            bool answersCountBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort answersCount);
            pos += 2;
            bool authoritiesCountBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort authoritiesCount);
            pos += 2;
            bool additionalsCountBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort additionalsCount);
            pos += 2;
            bool isSuccess = idBool && convertToBinaryBool && splitBinaryBool &&
                             qrBool && opCodeBool && aaBool && tcBool && rdBool && raBool && rCodeBool &&
                             answerAuthenticatedBool && nonAuthenticatedDataBool &&
                             questionsCountBool && answersCountBool && authoritiesCountBool && additionalsCountBool;

            if (!isSuccess) return new Header();

            Header header = new()
            {
                IsSuccess = isSuccess,
                ID = id,
                QR = qr,
                OperationalCode = opCode,
                AA = aa,
                TC = tc,
                RD = rd,
                RA = ra,
                Z = DnsEnums.Z.Reserved,
                AnswerAuthenticated = answerAuthenticated,
                NonAuthenticatedData = nonAuthenticatedData,
                ResponseCode = rCode,
                QuestionsCount = questionsCount,
                AnswersCount = answersCount,
                AuthoritiesCount = authoritiesCount,
                AdditionalsCount = additionalsCount
            };

            return header;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS Read Header: " + ex.Message);
            return new Header();
        }
    }

    public static bool TryWrite(DnsMessage dnsMessage, ref int pos, out byte[] buffer)
    {
        try
        {
            Header header = dnsMessage.Header;
            bool idBool = ByteArrayTool.TryConvertUInt16ToBytes(header.ID, out byte[] id);

            bool[] bits = new bool[16]; // 16 Bits (2 Bytes)
            bits[0] = Convert.ToBoolean(header.QR); // 0
            bool opCodeBool = ByteArrayTool.TrySplitBinary((int)header.OperationalCode, out bool[] opCode);
            if (opCodeBool) opCode.CopyTo(bits, 1); // 1-4
            bits[5] = Convert.ToBoolean(header.AA); // 5
            bits[6] = Convert.ToBoolean(header.TC); // 6
            bits[7] = Convert.ToBoolean(header.RD); // 7
            bits[8] = Convert.ToBoolean(header.RA); // 8
            bool zBool = ByteArrayTool.TrySplitBinary((int)header.Z, out bool[] z);
            if (zBool) z.CopyTo(bits, 9); // 9-10, ZZ, Reserved
            bits[11] = Convert.ToBoolean(header.AnswerAuthenticated); // 11
            bits[12] = Convert.ToBoolean(header.NonAuthenticatedData); // 12
            bool rCodeBool = ByteArrayTool.TrySplitBinary((int)header.ResponseCode, out bool[] rCode);
            if (rCodeBool) rCode.CopyTo(bits, 13); // 13-16
            bool bitsBool = ByteArrayTool.TryConvertSplittedBinaryToBytes(bits, out byte[] bitsBytes);

            bool questionsCountBool = ByteArrayTool.TryConvertUInt16ToBytes(header.QuestionsCount, out byte[] questionsCount);
            bool answersCountBool = ByteArrayTool.TryConvertUInt16ToBytes(header.AnswersCount, out byte[] answersCount);
            bool authoritiesCountBool = ByteArrayTool.TryConvertUInt16ToBytes(header.AuthoritiesCount, out byte[] authoritiesCount);
            bool additionalRecordsCountBool = ByteArrayTool.TryConvertUInt16ToBytes(header.AdditionalsCount, out byte[] additionalRecordsCount);
            
            bool isSuccess = idBool &&
                             opCodeBool && zBool && rCodeBool && bitsBool &&
                             questionsCountBool && answersCountBool && authoritiesCountBool && additionalRecordsCountBool;
            
            if (!isSuccess)
            {
                buffer = Array.Empty<byte>();
                return false;
            }

            List<byte> bufferList = new();
            bufferList.AddRange(id);
            bufferList.AddRange(bitsBytes);
            bufferList.AddRange(questionsCount);
            bufferList.AddRange(answersCount);
            bufferList.AddRange(authoritiesCount);
            bufferList.AddRange(additionalRecordsCount);
            buffer = bufferList.ToArray();

            bool totalSuccess = buffer.Length == MsmhAgnosticServer.DNS_HEADER_LENGTH;
            if (!totalSuccess)
            {
                buffer = Array.Empty<byte>();
                return false;
            }

            pos += MsmhAgnosticServer.DNS_HEADER_LENGTH;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS TryWrite Header: " + ex.Message);
            buffer = Array.Empty<byte>();
            return false;
        }
    }
}
