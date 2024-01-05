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

using System.Text;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;

namespace Ps2IsoTools.ISO.VolumeDescriptors
{
    internal class BaseVolumeDescriptor
    {
        public VolumeDescriptorType VolumeDescriptorType { get; private set; }
        public string Identifier { get; private set; } = "CD001";
        public byte VolumeDescriptorVersion = 1;

        public BaseVolumeDescriptor(VolumeDescriptorType type, byte version)
        {
            VolumeDescriptorType = type;
            Identifier = "CD001";
            VolumeDescriptorVersion = version;
        }

        public BaseVolumeDescriptor(string identifier, byte version = 1)
        {
            VolumeDescriptorType = VolumeDescriptorType.Boot;
            Identifier = identifier;
            VolumeDescriptorVersion = version;
        }

        public BaseVolumeDescriptor(byte[] buffer, int offset)
        {
            VolumeDescriptorType = (VolumeDescriptorType)buffer[offset];
            Identifier = Encoding.ASCII.GetString(buffer.Slice((offset + 1), (offset + 6)));
            VolumeDescriptorVersion = buffer[offset + 6];
        }
        internal virtual void WriteTo(byte[] buffer, int offset)
        {
            Array.Clear(buffer, offset, IsoUtilities.SectorSize);
            buffer[offset] = (byte)VolumeDescriptorType;
            IsoUtilities.WriteAChars(buffer, offset + 1, 5, Identifier);
            buffer[offset + 6] = VolumeDescriptorVersion;
        }
    }
}
