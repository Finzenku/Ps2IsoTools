//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using Ps2IsoTools.DiscUtils.Utils;
using System.Globalization;
using System.Text;

namespace Ps2IsoTools.UDF.EntityIdentifiers
{
    internal class DomainEntityIdentifier : EntityIdentifier
    {
        public DomainEntityIdentifier() { }
        public DomainEntityIdentifier(string identifier, ushort udfVersion, DomainFlags flags)
        {
            if (Encoding.ASCII.GetByteCount(identifier) > 23)
                throw new ArgumentException("String identifier is longer than 23 bytes");
            Flags = 0;
            Identifier = identifier;
            EndianUtilities.WriteBytesLittleEndian(udfVersion, Suffix, 0);
            Suffix[2] = (byte)flags;
        }

        public override string ToString()
        {
            string major = ((uint)Suffix[1]).ToString("X", CultureInfo.InvariantCulture);
            string minor = ((uint)Suffix[0]).ToString("X", CultureInfo.InvariantCulture);
            DomainFlags flags = (DomainFlags)Suffix[2];
            return string.Format(CultureInfo.InvariantCulture, "{0} [UDF {1}.{2} : Flags {3}]", Identifier, major, minor,
                flags);
        }
    }
}