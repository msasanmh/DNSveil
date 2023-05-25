using System;
using System.Diagnostics;
using System.Text;

namespace MsmhTools.HTTPProxyServer
{
    /// <summary>
    /// Decode chunk-transfer-encoded streams or byte arrays, particularly from HTTP data.
    /// </summary>
    public class ChunkDecoder
    {
        /// <summary>
        /// Callback to invoke to process signatures found in the chunk length line.
        /// Called before ProcessChunk.
        /// Return 'true' to continue operation.  
        /// Return 'false' to terminate.
        /// </summary>
        public Func<KeyValuePair<string, string>, byte[], bool>? ProcessSignature = null;

        /// <summary>
        /// Callback to invoke to process a chunk.
        /// Called after ProcessSignature.
        /// Return 'true' to continue operation.
        /// Return 'false' to terminate.
        /// </summary>
        public Func<byte[], bool>? ProcessChunk = null;

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ChunkDecoder()
        {

        }

        /// <summary>
        /// Decode a chunk-transfer-encoded byte array to a non-chunked byte array.
        /// </summary>
        /// <param name="data">Original byte array with chunk-transfer-encoding.</param>
        /// <param name="outData">Data without encoding.</param>
        /// <returns>True if successful.</returns>
        public bool Decode(byte[] data, out byte[]? outData)
        {
            outData = null;
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));

            MemoryStream ms = new();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);

            if (Decode(ms, out long contentLength, out MemoryStream outStream))
            {
                if (contentLength > 0)
                {
                    outData = new byte[contentLength];
                    outData = outStream.ToArray();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Decode a chunk-transfer-encoded stream to a non-chunked stream.
        /// </summary>
        /// <param name="stream">Original stream with chunk-transfer-encoding.</param>
        /// <param name="contentLength">Content length of the data in the output stream.</param>
        /// <param name="outStream">Output stream containing data without encoding.</param>
        /// <returns>True if successful.</returns>
        public bool Decode(Stream stream, out long contentLength, out MemoryStream outStream)
        {
            contentLength = 0;
            outStream = new MemoryStream();

            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("Cannot read from supplied stream.");
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

            int bytesRead = 0;
            long segmentLength = 0;
            byte[] headerBuffer = new byte[1];
            string header = "";
            byte[]? dataBuffer = null;
            long bytesRemaining = 0;

            while (true)
            {
                // Read-Chunk-Length
                Log("Reading chunk length");
                headerBuffer = new byte[1];
                header = "";

                while (true)
                {
                    bytesRead = stream.Read(headerBuffer, 0, headerBuffer.Length);
                    if (bytesRead > 0)
                    {
                        Log("| Read " + bytesRead + ": " + Encoding.UTF8.GetString(headerBuffer).Trim());
                        header += Convert.ToChar(headerBuffer[0]);
                        if ((int)headerBuffer[0] == 10)
                        {
                            // end of header
                            Log("| End of header detected");
                            break;
                        }
                    }
                }

                // Check-for-Key-Value-Pairs
                header = header.Trim();
                string segmentLengthStr;
                Dictionary<string, string> signatures = new();

                if (header.Contains(';'))
                {
                    Log("Embedded key-value pairs detected");
                    string[] vals = header.Split(';');
                    segmentLengthStr = vals[0];

                    if (vals.Length > 1)
                    {
                        for (int i = 1; i < vals.Length; i++)
                        {
                            string[] kvp = vals[i].Split('=');
                            if (kvp.Length == 1)
                            {
                                Log("| " + kvp[0] + ": null");
                                signatures.Add(kvp[0], null);
                            }
                            if (kvp.Length == 2)
                            {
                                Log("| " + kvp[0] + ": " + kvp[1]);
                                signatures.Add(kvp[0], kvp[1]);
                            }
                        }
                    }
                }
                else
                {
                    segmentLengthStr = header;
                }

                Log("Reading data of length " + segmentLengthStr);

                // Check-for-End
                if (!string.IsNullOrEmpty(segmentLengthStr)) segmentLength = Convert.ToInt64(segmentLengthStr, 16);
                if (segmentLength < 1)  // Segment length of 0 indicates end of message
                {
                    // Read out the final \r\n
                    Log("End of message detected");
                    headerBuffer = new byte[2];
                    bytesRead = stream.Read(headerBuffer, 0, headerBuffer.Length);
                    break;  // end of stream
                }

                // Read-Data
                dataBuffer = new byte[segmentLength];
                bytesRemaining = segmentLength;

                while (bytesRemaining > 0)
                {
                    bytesRead = stream.Read(dataBuffer, 0, dataBuffer.Length);
                    if (bytesRead > 0)
                    {
                        Log("| Read " + bytesRead + ": " + Encoding.UTF8.GetString(dataBuffer));

                        if (ProcessSignature != null && signatures != null && signatures.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> currSignature in signatures)
                            {
                                if (!ProcessSignature(currSignature, dataBuffer))
                                {
                                    Log("*** Failed to process signature " + currSignature.Key + ", exiting");
                                    return false;
                                }
                            }
                        }

                        if (ProcessChunk != null)
                        {
                            if (!ProcessChunk(dataBuffer))
                            {
                                Log("*** Failed to process chunk");
                                return false;
                            }
                        }

                        outStream.Write(dataBuffer, 0, bytesRead);
                        bytesRemaining -= bytesRead;
                        contentLength += bytesRead;
                        Log("| Content length is now " + contentLength);
                    }
                }

                // Read-CRLF-After-Data
                // Read out the final \r\n
                byte[] newlineBuffer = new byte[1];
                while (true)
                {
                    bytesRead = stream.Read(newlineBuffer, 0, newlineBuffer.Length);
                    if (bytesRead > 0)
                    {
                        if ((int)newlineBuffer[0] == 10) break;
                    }
                }
            }

            outStream.Seek(0, SeekOrigin.Begin);
            return true;
        }

        // Private-Methods
        private void Log(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.WriteLine(msg);
            }
        }
    }
}
