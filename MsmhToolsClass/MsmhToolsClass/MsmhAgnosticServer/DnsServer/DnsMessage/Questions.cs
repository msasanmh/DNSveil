using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class Question
{
    public string QNAME { get; set; } = string.Empty; // URL Encoded Host, Terminates With 0x00 (e.g. 0x07 example 0x03 com 0x00)
    public int QNamePosition { get; protected set; } = MsmhAgnosticServer.DNS_HEADER_LENGTH;
    public DnsEnums.RRType QTYPE { get; set; } // 2 Bytes / DNS Record, A Record = 1
    public DnsEnums.CLASS QCLASS { get; set; } // 2 Bytes / DNS Class, IN = 1

    public Question() { }

    public Question(string qNAME, int qNamePosition, DnsEnums.RRType qTYPE, DnsEnums.CLASS qCLASS)
    {
        QNAME = qNAME;
        QNamePosition = qNamePosition;
        QTYPE = qTYPE;
        QCLASS = qCLASS;
    }
}

public class Questions
{
    public bool IsSuccess { get; private set; } = false;
    public List<Question> QuestionRecords { get; set; } = new();

    public override string ToString()
    {
        string result = "DNS Questions:\n";
        result += $"{nameof(IsSuccess)}: {IsSuccess}\n";
        for (int n = 0; n < QuestionRecords.Count; n++)
        {
            result += $"Question Number {n + 1}:\n";
            Question question = QuestionRecords[n];
            result += $"{nameof(question.QNAME)}: {question.QNAME}\n";
            result += $"{nameof(question.QTYPE)}: {question.QTYPE}\n";
            result += $"{nameof(question.QCLASS)}: {question.QCLASS}";
        }
        return result;
    }

    public static Questions Read(byte[] buffer, ref int pos, DnsMessage dnsMessage)
    {
        try
        {
            Questions questions = new();

            for (int n = 0; n < dnsMessage.Header.QuestionsCount; n++)
            {
                if (pos > buffer.Length) break;
                if (buffer.Length < pos + 6) break;
                
                // QNAME
                string domain = ResourceRecord.ReadRecordName(buffer, pos, out int qLength).ToString();
                if (string.IsNullOrEmpty(domain)) return questions;
                int qNamePosition = pos;
                pos += qLength;
                
                // QTYPE
                if (pos + 2 > buffer.Length) return questions;
                bool qTypeBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort qType);
                pos += 2;

                // QCLASS
                if (pos + 2 > buffer.Length) return questions;
                bool qClassBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort qClass);
                pos += 2;

                if (!qTypeBool || !qClassBool) return questions;

                DnsEnums.RRType typeEnum = (DnsEnums.RRType)Enum.Parse(typeof(DnsEnums.RRType), qType.ToString());
                DnsEnums.CLASS classEnum = (DnsEnums.CLASS)Enum.Parse(typeof(DnsEnums.CLASS), qClass.ToString());

                Question question = new(domain, qNamePosition, typeEnum, classEnum);
                questions.QuestionRecords.Add(question);
                questions.IsSuccess = true;
            }

            return questions;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS Read Questions: " + ex.Message);
            return new Questions();
        }
    }

    public static bool TryWrite(DnsMessage dnsMessage, ref int pos, out byte[] buffer)
    {
        try
        {
            Questions questions = dnsMessage.Questions;
            List<byte> bufferList = new();
            
            for (int n = 0; n < questions.QuestionRecords.Count; n++)
            {
                Question question = questions.QuestionRecords[n];
                
                if (string.IsNullOrEmpty(question.QNAME.Trim()) || !NetworkTool.IsDomainNameValid(question.QNAME))
                {
                    buffer = Array.Empty<byte>();
                    return false;
                }
                
                bool qTypeBool = ByteArrayTool.TryConvertUInt16ToBytes((ushort)question.QTYPE, out byte[] qType);
                bool qClassBool = ByteArrayTool.TryConvertUInt16ToBytes((ushort)question.QCLASS, out byte[] qClass);

                bool isSuccess = qTypeBool && qClassBool;

                if (!isSuccess)
                {
                    buffer = Array.Empty<byte>();
                    return false;
                }

                // QNAME
                byte[] qName = ResourceRecord.WriteRecordName(dnsMessage, question.QNAME, question.QNamePosition);

                bufferList.AddRange(qName);
                bufferList.AddRange(qType); // QTYPE
                bufferList.AddRange(qClass); // QCLASS
            }
            
            buffer = bufferList.ToArray();
            pos += buffer.Length;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS TryWrite Questions: " + ex.Message);
            buffer = Array.Empty<byte>();
            return false;
        }
    }
}
