namespace Ps2IsoTools.UDF.Descriptors
{
    internal enum ShortAllocationFlags
    {
        RecordedAndAllocated = 0,
        AllocatedNotRecorded = 1,
        NotRecordedNotAllocated = 2,
        NextExtentOfAllocationDescriptors = 3
    }
}
