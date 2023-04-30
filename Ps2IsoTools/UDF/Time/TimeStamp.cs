using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Time
{
    public class TimeStamp : IByteArraySerializable
    {
        public ushort TypeAndTimezone;
        public int Timezone => (TypeAndTimezone >> 12) & 0x0F;
        public int MinutesWest 
        { 
            get 
            {
                int mins = TypeAndTimezone & 0xFFF;
                if ((mins & 0x800) != 0)
                    return (-1 & ~0xFFF) | mins;
                return mins;
            } 
        }
        public short Year;
        public byte Month;
        public byte Day;
        public byte Hour;
        public byte Minute;
        public byte Second;
        public byte Centiseconds;
        public byte HundredsofMicroseconds;
        public byte Microseconds;
        public DateTime DateTime => new DateTime(Year, Month, Day, Hour, Minute, Second, 10 * Centiseconds + HundredsofMicroseconds / 10, DateTimeKind.Utc) - TimeSpan.FromMinutes(MinutesWest);

        public int Size => 12;

        public static TimeStamp FromDateTime(DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();
            return new TimeStamp
            {
                TypeAndTimezone = 0x1000,
                Year = (short)dateTime.Year,
                Month = (byte)dateTime.Month,
                Day = (byte)dateTime.Day,
                Hour = (byte)dateTime.Hour,
                Minute = (byte)dateTime.Minute,
                Second = (byte)dateTime.Second,
                Centiseconds = 0,
                HundredsofMicroseconds = 0,
                Microseconds = 0
            };
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            TypeAndTimezone = EndianUtilities.ToUInt16LittleEndian(buffer, offset);
            Year = UdfUtilities.ForceRange(1, 9999, EndianUtilities.ToInt16LittleEndian(buffer, offset + 2));
            Month = UdfUtilities.ForceRange(1, 12, buffer[offset + 4]);
            Day =  UdfUtilities.ForceRange(1, 31, buffer[offset + 5]);
            Hour = UdfUtilities.ForceRange(0, 23, buffer[offset + 6]);
            Minute = UdfUtilities.ForceRange(0, 59, buffer[offset + 7]);
            Second = UdfUtilities.ForceRange(0, 59, buffer[offset + 8]);
            Centiseconds = UdfUtilities.ForceRange(0, 99, buffer[offset + 9]);
            HundredsofMicroseconds = UdfUtilities.ForceRange(0, 99, buffer[offset + 10]);
            Microseconds = UdfUtilities.ForceRange(0, 99, buffer[offset + 11]);

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            EndianUtilities.WriteBytesLittleEndian(TypeAndTimezone, buffer, offset);
            EndianUtilities.WriteBytesLittleEndian(Year, buffer, offset + 2);
            buffer[offset + 4] = Month;
            buffer[offset + 5] = Day;
            buffer[offset + 6] = Hour;
            buffer[offset + 7] = Minute;
            buffer[offset + 8] = Second;
            buffer[offset + 9] = Centiseconds;
            buffer[offset + 10] = HundredsofMicroseconds;
            buffer[offset + 11] = Microseconds;
        }

        

        public override string ToString()
        {
            return $"{TypeAndTimezone:X4} ({Timezone}, {MinutesWest}) {Year} {Month} {Day} {Hour} {Minute} {Second} {Centiseconds} {HundredsofMicroseconds} {Microseconds}";
        }

    }
}
