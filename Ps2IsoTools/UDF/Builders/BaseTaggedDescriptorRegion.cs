using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.ISO.VolumeDescriptors.Regions;
using Ps2IsoTools.UDF.Descriptors;

namespace Ps2IsoTools.UDF.Builders
{
    internal class BaseTaggedDescriptorRegion : VolumeDescriptorDiskRegion
    {
        private readonly BaseTaggedDescriptor _descriptor;

        public BaseTaggedDescriptorRegion(BaseTaggedDescriptor descriptor, long start)
            : base(start)
        {
            _descriptor = descriptor;
        }

        protected override byte[] GetBlockData()
        {
            byte[] buffer = new byte[IsoUtilities.SectorSize];
            _descriptor.WriteTo(buffer, 0);
            return buffer;
        }
    }
}
