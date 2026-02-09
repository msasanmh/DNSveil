using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class Answers
{
    public List<IResourceRecord> AnswerRecords = new();

    public override string ToString()
    {
        if (!AnswerRecords.Any()) return string.Empty;
        string result = "DNS Answer:\n";
        foreach (IResourceRecord r in AnswerRecords) result += r.ToString() + "\n";
        return result;
    }

    public static Answers Read(byte[] buffer, ref int pos, DnsMessage dnsMessage)
    {
        Answers answers = new()
        {
            AnswerRecords = ResourceRecord.Read(buffer, ref pos, dnsMessage.Header.AnswersCount)
        };
        return answers;
    }

    public static bool TryWrite(DnsMessage dnsMessage, ref int pos, out byte[] buffer)
    {
        return ResourceRecord.TryWrite(dnsMessage, dnsMessage.Answers.AnswerRecords, ref pos, out buffer);
    }
}

public class Authorities
{
    public List<IResourceRecord> AuthorityRecords = new();

    public override string ToString()
    {
        if (!AuthorityRecords.Any()) return string.Empty;
        string result = "DNS Authority:\n";
        foreach (IResourceRecord r in AuthorityRecords) result += r.ToString() + "\n";
        return result;
    }

    public static Authorities Read(byte[] buffer, ref int pos, DnsMessage dnsMessage)
    {
        Authorities authorities = new()
        {
            AuthorityRecords = ResourceRecord.Read(buffer, ref pos, dnsMessage.Header.AuthoritiesCount)
        };
        return authorities;
    }

    public static bool TryWrite(DnsMessage dnsMessage, ref int pos, out byte[] buffer)
    {
        return ResourceRecord.TryWrite(dnsMessage, dnsMessage.Authorities.AuthorityRecords, ref pos, out buffer);
    }
}

public class Additionals
{
    public List<IResourceRecord> AdditionalRecords = new();

    public override string ToString()
    {
        if (!AdditionalRecords.Any()) return string.Empty;
        string result = "DNS Additional:\n";
        foreach (IResourceRecord r in AdditionalRecords) result += r.ToString() + "\n";
        return result;
    }

    public static Additionals Read(byte[] buffer, ref int pos, DnsMessage dnsMessage)
    {
        Additionals additionals = new()
        {
            AdditionalRecords = ResourceRecord.Read(buffer, ref pos, dnsMessage.Header.AdditionalsCount)
        };
        return additionals;
    }

    public static bool TryWrite(DnsMessage dnsMessage, ref int pos, out byte[] buffer)
    {
        return ResourceRecord.TryWrite(dnsMessage, dnsMessage.Additionals.AdditionalRecords, ref pos, out buffer);
    }
}

public interface IResourceRecord
{
    string Name { get; }
    DnsEnums.RRType TYPE { get; }
    DnsEnums.CLASS CLASS { get; }
    uint TimeToLive { get; }
    DateTime TTLDateTime { get; }
    string ToString();
}

public class ResourceRecord : IResourceRecord
{
    public string Name { get; set; } = string.Empty; // Vary Bytes / Compressed Host, Contains Pointers
    public DnsEnums.RRType TYPE { get; set; } // 2 Bytes / DNS Record, A Record = 1
    public DnsEnums.CLASS CLASS { get; set; } // 2 Bytes / DNS Class, IN = 1
    public uint TimeToLive { get; set; } // 4 Bytes / TTL In Sec
    public DateTime TTLDateTime { get; set; }
    internal byte[] RecordBuffer { get; set; } = Array.Empty<byte>();

    public override string ToString()
    {
        string result = $"{nameof(Name)}: {Name}\n";
        result += $"{nameof(TYPE)}: {TYPE}\n";
        result += $"{nameof(CLASS)}: {CLASS}\n";
        result += $"{nameof(TimeToLive)}: {TimeToLive}";
        return result;
    }

    public static List<IResourceRecord> Read(byte[] buffer, ref int pos, int resourceCount)
    {
        List<IResourceRecord> resourceRecords = new();
        
        try
        {
            int currentPos = pos;
            for (int n = 0; n < resourceCount; n++)
            {
                int recordStartPos = pos;
                ResourceRecord resourceRecord = new();
                
                if (buffer.Length <= pos + 12) return resourceRecords;

                string name = ReadRecordName(buffer, pos, out int length).ToString().Trim(); // Name Can Be Empty Here
                resourceRecord.Name = name;
                currentPos += length;
                pos += length;

                // TYPE
                if (currentPos + 2 > buffer.Length) return resourceRecords;
                bool rrTypeBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[currentPos..(currentPos + 2)], out ushort rrType);
                if (!rrTypeBool) return resourceRecords;
                resourceRecord.TYPE = DnsEnums.ParseRRType(rrType);
                if (resourceRecord.TYPE.Equals(DnsEnums.RRType.Unknown)) return resourceRecords;
                currentPos += 2;
                pos += 2;

                // CLASS
                if (currentPos + 2 > buffer.Length) return resourceRecords;
                bool rClassBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[currentPos..(currentPos + 2)], out ushort rClass);
                if (!rClassBool) return resourceRecords;
                resourceRecord.CLASS = DnsEnums.ParseClass(rClass);
                if (resourceRecord.CLASS.Equals(DnsEnums.CLASS.Unknown)) return resourceRecords;
                currentPos += 2;
                pos += 2;

                // TTL
                if (currentPos + 4 > buffer.Length) return resourceRecords;
                bool ttlBool = ByteArrayTool.TryConvertBytesToUInt32(buffer[currentPos..(currentPos + 4)], out uint ttl);
                if (!ttlBool) return resourceRecords;
                resourceRecord.TimeToLive = ttl;
                resourceRecord.TTLDateTime = DateTime.UtcNow;
                currentPos += 4;
                pos += 4;

                // RDLENGTH - The Length Of RDDATA
                if (currentPos + 2 > buffer.Length) return resourceRecords;
                bool rdLengthBool = ByteArrayTool.TryConvertBytesToUInt16(buffer[currentPos..(currentPos + 2)], out ushort rdLength);
                if (!rdLengthBool) return resourceRecords;
                currentPos += 2;
                pos += 2;

                // RDDATA
                resourceRecord = ReadRDDATA(resourceRecord, buffer, pos, rdLength);
                int rdLengthInt = Convert.ToInt32(rdLength);
                currentPos += rdLengthInt;
                pos += rdLengthInt;
                
                if (pos <= buffer.Length && rdLengthInt != 0)
                {
                    resourceRecord.RecordBuffer = buffer[recordStartPos..pos];
                    resourceRecords.Add(resourceRecord);
                }
                else
                {
                    Debug.WriteLine("DNS ResourceRecord: Empty Record Received");
                    Debug.WriteLine($"Buffer Length: {buffer.Length} Position: {pos}");
                    Debug.WriteLine($"RDData Length: {rdLengthInt} TTL: {resourceRecord.TimeToLive}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS ResourceRecord Read: " + ex.Message);
        }

        return resourceRecords;
    }

    public static bool TryWrite(DnsMessage dnsMessage, List<IResourceRecord> resourceRecords, ref int pos, out byte[] buffer)
    {
        buffer = Array.Empty<byte>();
        if (!resourceRecords.Any()) return true;
        List<byte> rrsBufferList = new();

        try
        {
            for (int n = 0; n < resourceRecords.Count; n++)
            {
                List<byte> rrBufferList = new();
                IResourceRecord resourceRecord = resourceRecords[n];

                byte[] name = WriteRecordName(dnsMessage, resourceRecord.Name, pos);
                
                bool rTypeBool = ByteArrayTool.TryConvertUInt16ToBytes((ushort)resourceRecord.TYPE, out byte[] rType); // 2 Bytes
                bool rClassBool = ByteArrayTool.TryConvertUInt16ToBytes((ushort)resourceRecord.CLASS, out byte[] rClass); // 2 Bytes
                bool ttlBool = ByteArrayTool.TryConvertUInt32ToBytes(resourceRecord.TimeToLive, out byte[] ttl); // 4 Bytes
                
                if (!rTypeBool || !rClassBool || !ttlBool) return false;
                
                rrBufferList.AddRange(name);
                rrBufferList.AddRange(rType);
                rrBufferList.AddRange(rClass);
                rrBufferList.AddRange(ttl);

                pos += name.Length; // QNAME(Vari)
                pos += 8; // TYPE(2), CLASS(2), TTL(4)

                // RDLENGTH & RDDATA
                bool writeRdDataBool = TryWriteRDDATA(resourceRecord, rrBufferList, dnsMessage, ref pos);
                if (!writeRdDataBool) return false;

                rrsBufferList.AddRange(rrBufferList);
            }

            buffer = rrsBufferList.ToArray();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS ResourceRecord TryWrite: " + ex.Message);
            buffer = Array.Empty<byte>();
            return false;
        }
    }

    private enum CompressionPointer : ushort
    {
        NormalPointer = 11, // 2 Bytes
        ExtendedPointer_8Bit_Offset = 01, // 3 Bytes
        ExtendedPointer_16Bit_Offset = 10, // 4 Bytes
        Label = 00,
        Unknown = 99
    }

    private static bool TryGetPointer(byte pointerByte, out CompressionPointer pointer)
    {
        bool binaryStrBool = ByteArrayTool.TryConvertToBinary(pointerByte, out string binaryStr);
        bool bitsBool = ByteArrayTool.TrySplitBinary(binaryStr, out bool[] bits);
        bool pointBool = CommonTools.TryConvertToEnum(new[] { bits[0], bits[1] }, out pointer);
        return binaryStrBool && bitsBool && pointBool;
    }

    private static int GetLabelPositionByPoint(CompressionPointer pointer, byte[] pointerBuffer, out int length)
    {
        // 0x3F To Convert Two First Bits to 00
        if (pointer == CompressionPointer.NormalPointer)
        {
            length = 2;
            int refPos = ((pointerBuffer[0] & 0x3F) << 8) | pointerBuffer[1];
            return refPos;
        }
        else if (pointer == CompressionPointer.ExtendedPointer_8Bit_Offset)
        {
            length = 3;
            int refPos = ((pointerBuffer[0] & 0x3F) << 8) | pointerBuffer[1];
            int offset = pointerBuffer[2];
            return refPos + offset;
        }
        else if (pointer == CompressionPointer.ExtendedPointer_16Bit_Offset)
        {
            length = 4;
            int refPos = ((pointerBuffer[0] & 0x3F) << 8) | pointerBuffer[1];
            int offset = (pointerBuffer[2] << 8) + pointerBuffer[3];
            return refPos + offset;
        }
        else
        {
            length = 0;
            return 0;
        }
    }

    private static byte[] WritePointer(int labelPos)
    {
        try
        {
            // 00111111 = 63, 11111111 = 255, 0011111111111111 = 16383
            int in14Bits = 16383; // Max: 16383
            if (labelPos <= in14Bits) // 16383
            {
                // NormalPointer 11
                //Debug.WriteLine("======= WritePointer: NormalPointer");
                byte[] bytes = new byte[2];
                bytes[0] = (byte)(labelPos >> 8);
                bytes[1] = (byte)(labelPos & 255);
                bytes[0] &= 63; // 00
                bytes[0] |= 192; // 11
                return bytes;
            }
            else if (labelPos <= in14Bits + 255) // 16383 + 255
            {
                // ExtendedPointer_8Bit_Offset 01
                Debug.WriteLine("======= WritePointer: ExtendedPointer_8Bit_Offset " + labelPos);
                byte[] bytes = new byte[3];
                int offset = labelPos - in14Bits;
                bytes[0] = (byte)(in14Bits >> 8);
                bytes[1] = (byte)(in14Bits & 255);
                bytes[2] = (byte)offset;
                bytes[0] &= 63; // 00
                bytes[0] |= 64; // 01
                return bytes;
            }
            else if (labelPos <= in14Bits + 65535) // 16383 + 65535
            {
                // ExtendedPointer_16Bit_Offset 10
                Debug.WriteLine("======= WritePointer: ExtendedPointer_16Bit_Offset " + labelPos);
                byte[] bytes = new byte[4];
                int offset = labelPos - in14Bits;
                bytes[0] = (byte)(in14Bits >> 8);
                bytes[1] = (byte)(in14Bits & 255);
                bytes[2] = (byte)(offset >> 8);
                bytes[3] = (byte)offset;
                bytes[0] &= 63; // 00
                bytes[0] |= 128; // 10
                return bytes;
            }
            else return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS ResourceRecord WritePointer: " + ex.Message);
            return Array.Empty<byte>();
        }
    }

    public static StringBuilder ReadRecordName(byte[] buffer, int startPostion, out int length, bool isEmail = false)
    {
        int overflow = 0;
        return ReadRecordNameInternal(ref overflow, buffer, startPostion, out length, isEmail);
    }

    public static StringBuilder ReadRecordNameInternal(ref int innerOverflow, byte[] buffer, int startPostion, out int length, bool isEmail = false)
    {
        // https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.4
        // A sequence of labels ending in a zero octet
        // A pointer
        // A sequence of labels ending with a pointer
        length = 0;
        int pos = startPostion;
        StringBuilder sb = new();
        innerOverflow++;

        try
        {
            int stackOverflowMax = 10;
            int stackOverflow = 0;
            bool isPointer = false;
            while (true)
            {
                stackOverflow++;
                // Detect Infinite Pointer To Pointer
                if (stackOverflow > stackOverflowMax || innerOverflow > stackOverflowMax)
                {
                    Debug.WriteLine("DnsServer ResourceRecord: Prevented StackOverflow Exception. (Bad Response).");
                    // Usually It's A Web Page Rather Than A DNS Server Or A Bad Response.
                    break;
                }

                if (pos > buffer.Length) break;

                bool pointBool = TryGetPointer(buffer[pos], out CompressionPointer pointer);
                if (!pointBool)
                {
                    Debug.WriteLine("DNS TryGetPoint Failed");
                    break;
                }
                
                if (pointer == CompressionPointer.NormalPointer)
                {
                    if (pos + 2 > buffer.Length) break;
                    int refPos = GetLabelPositionByPoint(pointer, buffer[pos..(pos + 2)], out int len);
                    string recordName = ReadRecordNameInternal(ref innerOverflow, buffer, refPos, out _).ToString();
                    if (string.IsNullOrEmpty(recordName)) break;
                    sb.Append(recordName);
                    pos += len;
                    isPointer = true;
                }
                else if (pointer == CompressionPointer.ExtendedPointer_8Bit_Offset)
                {
                    if (pos + 3 > buffer.Length) break;
                    int refPos = GetLabelPositionByPoint(pointer, buffer[pos..(pos + 3)], out int len);
                    string recordName = ReadRecordNameInternal(ref innerOverflow, buffer, refPos, out _).ToString();
                    if (string.IsNullOrEmpty(recordName)) break;
                    sb.Append(recordName);
                    pos += len;
                    isPointer = true;
                    Debug.WriteLine("=============================== " + pointer + " " + sb.ToString());
                }
                else if (pointer == CompressionPointer.ExtendedPointer_16Bit_Offset)
                {
                    if (pos + 4 > buffer.Length) break;
                    int refPos = GetLabelPositionByPoint(pointer, buffer[pos..(pos + 4)], out int len);
                    string recordName = ReadRecordNameInternal(ref innerOverflow, buffer, refPos, out _).ToString();
                    if (string.IsNullOrEmpty(recordName)) break;
                    sb.Append(recordName);
                    pos += len;
                    isPointer = true;
                    Debug.WriteLine("=============================== " + pointer);
                }
                else if (pointer == CompressionPointer.Label)
                {
                    int len = buffer[pos];
                    pos++;
                    if (pos + len >= buffer.Length) break;
                    string label = Encoding.UTF8.GetString(buffer[pos..(pos + len)]) + ".";
                    sb.Append(label);
                    pos += len;
                }
                else
                {
                    Debug.WriteLine("DNS Unknown CompressionPointer: " + pointer);
                    break;
                }

                if (pos >= buffer.Length) break; // End Of Buffer

                if (buffer[pos] == 0 && !isPointer)
                {
                    pos++; // Label Terminator 0x00
                    break;
                }

                if (isPointer) break;
            }
            
            //Debug.WriteLine($"======== Found {sb} In {innerOverflow + 1} Loops =======");
            if (sb.Length > 0 && sb[^1] == '.') sb.Remove(sb.Length - 1, 1);

            StringBuilder sbResult = new();
            bool email = false;
            for (int n = 0; n < sb.Length; n++)
            {
                char c = sb[n];
                if (isEmail && !email && c.Equals('.'))
                {
                    sbResult.Append('@');
                    email = true;
                }
                else sbResult.Append(c);
            }

            string nameResult = sbResult.ToString().Trim();
            if (string.IsNullOrEmpty(nameResult))
            {
                length = 1;
            }
            else
            {
                Regex regex = new("^[0-9a-zA-Z-.@:]+$"); // Domain / IPv4 / IPv6 / Email (IP Is An Invalid Question)
                if (stackOverflow <= stackOverflowMax && innerOverflow <= stackOverflowMax && regex.IsMatch(nameResult))
                {
                    length = pos - startPostion;
                }
                else
                {
                    // Unexpected EOF
                    length = MsmhAgnosticServer.MaxDataSize - buffer.Length;
                    sbResult.Clear();
                }
            }
            
            return sbResult;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS ReadRecordName: " + ex.Message);
            return sb;
        }
    }

    public static byte[] WriteRecordName(DnsMessage dnsMessage, string domainName, int domainPos = -1)
    {
        // https://datatracker.ietf.org/doc/html/rfc1035#section-4.1.4
        // A sequence of labels ending in a zero octet
        // A pointer
        // A sequence of labels ending with a pointer
        // Limitation: https://datatracker.ietf.org/doc/html/rfc1035#section-2.3.4
        List<byte> bufferList = new();

        try
        {
            if (string.IsNullOrEmpty(domainName))
            {
                bufferList.Add(0x00); // Terminator
                return bufferList.ToArray();
            }

            // Convert @ To Dot
            string domainNameConverted = string.Empty;
            for (int n = 0; n < domainName.Length; n++)
            {
                char c = domainName[n];
                if (c.Equals('@')) c = '.';
                domainNameConverted += c;
            }

            bool isPointer = false;
            int labelSequencePos = domainPos;
            string[] labels = domainNameConverted.Split('.');
            for (int n = 0; n < labels.Length; n++)
            {
                string label = labels[n];

                if (string.IsNullOrEmpty(label))
                {
                    bufferList.Add(0x00);
                    continue;
                }

                string labelSequence = string.Join('.', labels[n..]);
                bool exist = isExist(labelSequence, out Tuple<string, int>? lp);
                if (exist && lp != null)
                {
                    // Create Pointer
                    byte[] pointerBytes = WritePointer(lp.Item2);
                    bufferList.AddRange(pointerBytes);
                    isPointer = true;
                    //Debug.WriteLine($"======= Used Pointer For: {lp.Item1} Pos: {lp.Item2}");
                    break;
                }
                else
                {
                    // Create Label
                    bufferList.Add(Convert.ToByte(label.Length));
                    bufferList.AddRange(Encoding.UTF8.GetBytes(label));
                    //Debug.WriteLine($"------- Label Created: {label}");

                    // Add Current Domain Name Labels To Compress DnsMessage
                    if (domainPos != -1 && !string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(labelSequence))
                    {
                        if (n > 0)
                        {
                            string preLabel = labels[n - 1];
                            labelSequencePos = labelSequencePos + Encoding.UTF8.GetBytes(preLabel).Length + 1; // 1 Byte Of Label Length
                        }

                        dnsMessage.LabelPositions.Add(new Tuple<string, int>(labelSequence, labelSequencePos));
                        //Debug.WriteLine($"======= Label Sequence Saved: {labelSequence} Pos: {labelSequencePos}");
                    }
                }
            }

            if (!isPointer) bufferList.Add(0x00); // Domain Terminator If Doesn't End With A Pointer

            bool isExist(string newLabel, out Tuple<string, int>? lpOut)
            {
                lpOut = null;
                foreach (Tuple<string, int> lp in dnsMessage.LabelPositions.ToList())
                    if (lp != null && newLabel.Equals(lp.Item1))
                    {
                        lpOut = lp;
                        return true;
                    }
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS ResourceRecord WriteRecordLabel: " + ex.Message);
        }
        
        return bufferList.ToArray();
    }

    protected static ResourceRecord ReadRDDATA(ResourceRecord resourceRecord, byte[] buffer, int pos, ushort rLength)
    {
        return resourceRecord.TYPE switch
        {
            DnsEnums.RRType.A => ARecord.Parse(resourceRecord, buffer, pos),
            DnsEnums.RRType.AAAA => AaaaRecord.Parse(resourceRecord, buffer, pos),
            DnsEnums.RRType.CNAME => CNameRecord.Parse(resourceRecord, buffer, pos),
            DnsEnums.RRType.MX => MxRecord.Parse(resourceRecord, buffer, pos),
            DnsEnums.RRType.NS => NsRecord.Parse(resourceRecord, buffer, pos),
            DnsEnums.RRType.SOA => SoaRecord.Parse(resourceRecord, buffer, pos),
            DnsEnums.RRType.TEXT => TextRecord.Parse(resourceRecord, buffer, pos, rLength),
            _ => UnknownRecord.Parse(resourceRecord, buffer, pos, rLength)
        };
    }

    protected static bool TryWriteRDDATA(IResourceRecord resourceRecord, List<byte> bufferList, DnsMessage dnsMessage, ref int pos)
    {
        return resourceRecord.TYPE switch
        {
            DnsEnums.RRType.A => ARecord.TryWrite(resourceRecord, bufferList, ref pos),
            DnsEnums.RRType.AAAA => AaaaRecord.TryWrite(resourceRecord, bufferList, ref pos),
            DnsEnums.RRType.CNAME => CNameRecord.TryWrite(resourceRecord, bufferList, dnsMessage, ref pos),
            DnsEnums.RRType.MX => MxRecord.TryWrite(resourceRecord, bufferList, dnsMessage, ref pos),
            DnsEnums.RRType.NS => NsRecord.TryWrite(resourceRecord, bufferList, dnsMessage, ref pos),
            DnsEnums.RRType.SOA => SoaRecord.TryWrite(resourceRecord, bufferList, dnsMessage, ref pos),
            DnsEnums.RRType.TEXT => TextRecord.TryWrite(resourceRecord, bufferList, ref pos),
            _ => UnknownRecord.TryWrite(resourceRecord, bufferList, dnsMessage, ref pos)
        };
    }
}
