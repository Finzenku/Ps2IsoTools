﻿//
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
using Ps2IsoTools.UDF.OS;
using System.Globalization;

namespace Ps2IsoTools.UDF.EntityIdentifiers
{
    internal class ImplementationEntityIdentifier : EntityIdentifier
    {
        private bool osImplementation = false;
        public ImplementationEntityIdentifier() { }
        public ImplementationEntityIdentifier(string identifier, OSClass osClass = OSClass.None, OSIdentifier osId = OSIdentifier.None)
        {
            osImplementation = true;
            Identifier = identifier;
            EndianUtilities.WriteBytesBigEndian((short)osId, Suffix, 0);
            Suffix[0] = (byte)osClass;
        }

        public ImplementationEntityIdentifier(string identifier, ushort udfVer)
        {
            Identifier = identifier;
            EndianUtilities.WriteBytesBigEndian(udfVer, Suffix, 0);
        }

        public override string ToString()
        {
            if (Suffix[0] == 0 && Suffix[1] == 0)
                return Identifier;

            if (osImplementation)
            {
                OSClass osClass = (OSClass)Suffix[0];
                OSIdentifier osId = (OSIdentifier)EndianUtilities.ToUInt16BigEndian(Suffix, 0);
                return string.Format(CultureInfo.InvariantCulture, "{0} [OS {1} {2}]", Identifier, osClass, osId);
            }

            return $"{Identifier} {Suffix[1]:X}.{Suffix[0]:X2}";
        }
    }
}
