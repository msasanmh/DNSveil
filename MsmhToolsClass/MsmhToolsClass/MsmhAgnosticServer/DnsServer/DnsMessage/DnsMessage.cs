using System.Diagnostics;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DnsMessage
{
    public bool IsSuccess { get; internal set; } = false;
    public byte[] DnsMessageBuffer { get; internal set; } = Array.Empty<byte>();
    public DnsEnums.DnsProtocol DnsProtocol { get; set; }
    public ushort TcpMessageLength { get; internal set; }
    public Header Header { get; set; } = new();
    public Questions Questions {  get; set; } = new();
    public Answers Answers { get; set; } = new();
    public Authorities Authorities { get; set; } = new();
    public Additionals Additionals { get; set; } = new();
    internal List<Tuple<string, int>> LabelPositions { get; set; } = new();
    private static readonly string DnsMessageContentType = "application/dns-message";

    public DnsMessage() { }

    /// <summary>
    /// Clone With An Empty And New LabelPositions
    /// </summary>
    public DnsMessage(DnsMessage dm)
    {
        IsSuccess = dm.IsSuccess;
        DnsMessageBuffer = dm.DnsMessageBuffer;
        DnsProtocol = dm.DnsProtocol;
        TcpMessageLength = dm.TcpMessageLength;
        Header = dm.Header;
        Questions = dm.Questions;
        Answers = dm.Answers;
        Authorities = dm.Authorities;
        Additionals = dm.Additionals;
        LabelPositions = new();
    }

    public override string ToString()
    {
        string result = "DNS Message:\n";
        result += $"{nameof(IsSuccess)}: {IsSuccess}\n";
        result += $"{nameof(DnsProtocol)}: {DnsProtocol}\n";
        result += $"{nameof(TcpMessageLength)}: {TcpMessageLength}";
        result += "\n\n";
        result += Header.ToString();
        result += "\n\n";
        if (Header.QuestionsCount > 0)
        {
            result += Questions.ToString();
            result += "\n\n";
        }
        if (Header.AnswersCount > 0)
        {
            result += Answers.ToString();
            result += "\n\n";
        }
        if (Header.AuthoritiesCount > 0)
        {
            result += Authorities.ToString();
            result += "\n\n";
        }
        if (Header.AdditionalsCount > 0)
        {
            result += Additionals.ToString();
        }
        return result;
    }

    public static DnsMessage Read(byte[] buffer, DnsEnums.DnsProtocol dnsProtocol)
    {
        DnsMessage dnsMessage = new();
        if (buffer.Length <= 3) return dnsMessage;

        try
        {
            dnsMessage.DnsMessageBuffer = buffer;
            dnsMessage.DnsProtocol = dnsProtocol;

            int pos = 0;
            // https://datatracker.ietf.org/doc/html/rfc1035#section-4.2.2
            if (dnsProtocol == DnsEnums.DnsProtocol.TCP)
            {
                bool tcpMessageLengthBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[0..2], out ushort tcpMessageLength);
                if (!tcpMessageLengthBool) return dnsMessage;
                dnsMessage.TcpMessageLength = tcpMessageLength;
                buffer = buffer[2..]; // The Start Of Message Does Not Include TCP Message Length
                dnsMessage.DnsMessageBuffer = buffer;
            }

            dnsMessage.Header = Header.Read(buffer, ref pos);
            if (!dnsMessage.Header.IsSuccess) return dnsMessage;

            if (dnsMessage.Header.QuestionsCount > 0)
            {
                dnsMessage.Questions = Questions.Read(buffer, ref pos, dnsMessage);
                if (!dnsMessage.Questions.IsSuccess) return dnsMessage;

                if (dnsMessage.Header.AnswersCount > 0)
                    dnsMessage.Answers = Answers.Read(buffer, ref pos, dnsMessage);

                if (dnsMessage.Header.AuthoritiesCount > 0)
                    dnsMessage.Authorities = Authorities.Read(buffer, ref pos, dnsMessage);

                if (dnsMessage.Header.AdditionalsCount > 0)
                    dnsMessage.Additionals = Additionals.Read(buffer, ref pos, dnsMessage);

                dnsMessage.IsSuccess = dnsMessage.Header.IsSuccess && dnsMessage.Questions.IsSuccess;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsMessage Read: " + ex.Message);
        }
        
        return dnsMessage;
    }

    public static bool TryWrite(DnsMessage dnsMessage, out byte[] buffer)
    {
        dnsMessage = new DnsMessage(dnsMessage); // It's Important To Clear LabelPositions On New Write

        buffer = Array.Empty<byte>();

        try
        {
            int pos = 0;
            bool headerBool = Header.TryWrite(dnsMessage, ref pos, out byte[] header);
            if (!headerBool) return false;
            
            bool questionBool = Questions.TryWrite(dnsMessage, ref pos, out byte[] questions);
            if (!questionBool) return false;
            
            bool answersBool = Answers.TryWrite(dnsMessage, ref pos, out byte[] answers);
            if (!answersBool) return false;

            bool authoritiesBool = Authorities.TryWrite(dnsMessage, ref pos, out byte[] authorities);
            if (!authoritiesBool) return false;

            bool additionalsBool = Additionals.TryWrite(dnsMessage, ref pos, out byte[] additionals);
            if (!additionalsBool) return false;

            List<byte> bufferList = new();
            if (dnsMessage.DnsProtocol == DnsEnums.DnsProtocol.TCP)
            {
                dnsMessage.TcpMessageLength = Convert.ToUInt16(header.Length + questions.Length + answers.Length + authorities.Length + additionals.Length);
                bool tcpMessageLengthBool = ByteArrayTool.TryConvertUInt16ToBytes(dnsMessage.TcpMessageLength, out byte[] tcpMessageLength);
                if (!tcpMessageLengthBool) return false;
                bufferList.AddRange(tcpMessageLength);
            }

            bufferList.AddRange(header);
            bufferList.AddRange(questions);
            bufferList.AddRange(answers);
            bufferList.AddRange(authorities);
            bufferList.AddRange(additionals);
            buffer = bufferList.ToArray();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS TryWrite DnsMessage: " + ex.Message);
            return false;
        }
    }

    public static DnsMessage CreateQuery(DnsEnums.DnsProtocol dnsProtocol, string domain, DnsEnums.RRType rrType, DnsEnums.CLASS qClass)
    {
        DnsMessage dm = new()
        {
            DnsProtocol = dnsProtocol,
            Header = new()
            {
                ID = Header.GenerateId(),
                QR = DnsEnums.QR.Query,
                OperationalCode = DnsEnums.OperationalCode.QUERY,
                AA = DnsEnums.AA.NonAuthoritive,
                TC = DnsEnums.TC.NotTruncated,
                RD = DnsEnums.RD.RecursionIsDesired,
                RA = DnsEnums.RA.RecursionIsNotAvailable,
                AnswerAuthenticated = DnsEnums.AnswerAuthenticated.False,
                NonAuthenticatedData = DnsEnums.NonAuthenticatedData.False,
                ResponseCode = DnsEnums.ResponseCode.NoError,
                QuestionsCount = 1,
                AnswersCount = 0,
                AuthoritiesCount = 0,
                AdditionalsCount = 0
            },
            Questions = new()
            {
                QuestionRecords = new()
                {
                    new Question()
                    {
                        QNAME = domain,
                        QTYPE = rrType,
                        QCLASS = qClass
                    }
                }
            }
        };

        return dm;
    }

    public static DnsMessage CreateResponse(DnsMessage dnsMessage, ushort answersCount, ushort authoritiesCount, ushort additionalsCount)
    {
        dnsMessage.Header.QR = DnsEnums.QR.Response;
        dnsMessage.Header.ResponseCode = DnsEnums.ResponseCode.NoError;
        dnsMessage.Header.AnswersCount = answersCount;
        dnsMessage.Header.AuthoritiesCount = authoritiesCount;
        dnsMessage.Header.AdditionalsCount = additionalsCount;
        return dnsMessage;
    }

    public static DnsMessage CreateFailedResponse(DnsMessage dnsMessage)
    {
        dnsMessage.Header.QR = DnsEnums.QR.Response;
        dnsMessage.Header.ResponseCode = DnsEnums.ResponseCode.ServerFailure;
        dnsMessage.Header.AnswersCount = 0;
        dnsMessage.Header.AuthoritiesCount = 0;
        dnsMessage.Header.AdditionalsCount = 0;
        return dnsMessage;
    }

    public static bool TryWriteDoHResponse(byte[] aBuffer, out byte[] result)
    {
        // https://datatracker.ietf.org/doc/html/rfc8484#section-4.2.2
        try
        {
            List<byte> bufferList = new();
            string statusLine = $"HTTP/1.1 200 OK\r\n";
            bufferList.AddRange(Encoding.UTF8.GetBytes(statusLine));

            string contentTypeLine = "Content-Type: " + DnsMessageContentType + "\r\n";
            bufferList.AddRange(Encoding.UTF8.GetBytes(contentTypeLine));

            string contentLenLine = "Content-Length: " + aBuffer.Length + "\r\n";
            bufferList.AddRange(Encoding.UTF8.GetBytes(contentLenLine));

            bufferList.AddRange(Encoding.UTF8.GetBytes("\r\n"));

            // Merge Headers and Body
            bufferList.AddRange(aBuffer);

            result = bufferList.ToArray();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsMessage TryWriteDoHPostResponse: " + ex.Message);
            result = Array.Empty<byte>();
            return false;
        }
    }

}
