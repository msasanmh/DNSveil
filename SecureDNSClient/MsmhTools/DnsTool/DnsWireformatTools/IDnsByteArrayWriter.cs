using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools.DnsTool.DnsWireformatTools
{
    /// <summary>
    /// Represents a class which can write its contents to an enumerable of bytes.
    /// </summary>
    public interface IDnsByteArrayWriter
    {
        /// <summary>
        /// Write to the the specified byte array.
        /// </summary>
        /// <param name="bytes">The byte array to read from.</param>
        /// <param name="offset">The offset to start at.</param>
        void WriteBytes(Memory<byte> bytes, ref int offset);
    }
}
