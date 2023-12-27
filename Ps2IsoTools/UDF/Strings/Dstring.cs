using System.Text;

namespace Ps2IsoTools.UDF.Strings
{
    public class Dstring
    {
        private CompressionID _compID = CompressionID.Empty;
        private string value = string.Empty;
        public CompressionID CompressionID
        {
            get => _compID;
            set
            {
                if (_compID != value)
                {
                    _compID = value;
                    stringLen = GetStringByteCount(Value, CompressionID);
                    if (DataLength < MinLength)
                        DataLength = MinLength;
                }
            }
        }
        public string Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    stringLen = GetStringByteCount(Value, CompressionID);
                    if (DataLength < MinLength)
                        DataLength = MinLength;
                }
            }
        }
        public int DataLength { get; set; }
        public int MinLength => stringLen + 1;
        private int stringLen;

        public Dstring(string value, CompressionID compressionID, int dataLength)
        {
            stringLen = GetStringByteCount(value, compressionID);

            if (stringLen > 255)
                throw new ArgumentException($"baseString be a maximum of 255 bytes long; currently {stringLen} bytes");
            if (stringLen + 1 > dataLength)
                throw new ArgumentException("Byte count is greater than dataLength");

            _compID = compressionID;

            if (stringLen > 0)
            {
                Value = value;
            }
            DataLength = dataLength;
        }

        public Dstring(string value, CompressionID compressionID = CompressionID.UTF8)
        {
            stringLen = GetStringByteCount(value, compressionID);

            if (stringLen > 255)
                throw new ArgumentException($"baseString be a maximum of 255 bytes long; currently {stringLen} bytes");

            _compID = compressionID;

            if (stringLen > 0)
            {
                Value = value;
                DataLength = MinLength;
            }
        }

        //Lets `Dstring` be asigned as if it were a `string` e.g. Dstring myDstring = "string";
        public static implicit operator Dstring(string value) => new Dstring(value);
        //Lets any `Dstring` be treated as a `string` based on its string `Value` e.g. if (myDstring == "string")
        public static implicit operator string(Dstring value) => value.Value;

        public static int CalcLength(string value, CompressionID compressionID, int minLength = 0) => Math.Max(1 + GetStringByteCount(value, compressionID), minLength);

        private static int GetStringByteCount(string value, CompressionID compressionID)
        {
            if (string.IsNullOrEmpty(value) || value == "\0")
                return 0;
            switch (compressionID)
            {
                case CompressionID.UTF8:
                    return Encoding.UTF8.GetByteCount(value);

                case CompressionID.UTF16:
                    return Encoding.BigEndianUnicode.GetByteCount(value);

                case CompressionID.Empty:
                default:
                    return 0;
            }
        }

        public byte[] GetBytes()
        {
            byte[] data;
            if (DataLength >= MinLength)
                data = new byte[DataLength];
            else
                data = new byte[MinLength];
            switch (CompressionID)
            {
                case CompressionID.Empty:
                    return data;

                case CompressionID.UTF8:
                    Array.Copy(Encoding.UTF8.GetBytes(Value), 0, data, 1, stringLen);
                    break;

                case CompressionID.UTF16:
                    Array.Copy(Encoding.BigEndianUnicode.GetBytes(Value), 0, data, 1, stringLen);
                    break;
            }
            data[0] = (byte)CompressionID;
            if (DataLength > MinLength)
                data[data.Length - 1] = (byte)(MinLength);
            return data;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + DataLength)
                throw new InvalidDataException("The size of the buffer was too small to write to");
            Array.Copy(GetBytes(), 0, buffer, offset, DataLength);
        }

        public static Dstring FromBytes(byte[] buffer, int offset, int count, bool padded = true)
        {
            if (padded)
                return new Dstring(ReadDCharacters(buffer, offset, buffer[offset + count - 1]), (CompressionID)buffer[offset], count);
            else
                return new Dstring(ReadDCharacters(buffer, offset, count), (CompressionID)buffer[offset]);
        }

        public static string ReadDCharacters(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return string.Empty;

            CompressionID alg = (CompressionID)buffer[offset];

            if (alg != CompressionID.UTF8 && alg != CompressionID.UTF16)
                throw new InvalidDataException("Corrupt compressed unicode string");

            StringBuilder result = new StringBuilder(count);

            int pos = 1;
            while (pos < count)
            {
                char ch = '\0';

                if (alg == CompressionID.UTF16)
                {
                    ch = (char)(buffer[offset + pos] << 8);
                    pos++;
                }

                if (pos < count)
                {
                    ch |= (char)buffer[offset + pos];
                    pos++;
                }

                result.Append(ch);
            }

            return result.ToString();
        }

        public override string ToString() => Value;
    }
}
