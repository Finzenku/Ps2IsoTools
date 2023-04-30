using Ps2IsoTools.DiscUtils.Streams.Buffers;

namespace Ps2IsoTools.UDF.Files
{
    /// <summary>
    /// Interface implemented by a class representing a file.
    /// </summary>
    /// <remarks>
    /// File system implementations should have a class that implements this
    /// interface.  If the file system implementation is read-only, it is
    /// acceptable to throw <c>NotImplementedException</c> from setters.
    /// </remarks>
    internal interface IVfsFile
    {
        /// <summary>
        /// Gets or sets the last creation time in UTC.
        /// </summary>
        DateTime CreationTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the file's attributes.
        /// </summary>
        FileAttributes FileAttributes { get; set; }

        /// <summary>
        /// Gets a buffer to access the file's contents.
        /// </summary>
        IBuffer FileContent { get; }

        /// <summary>
        /// Gets the length of the file.
        /// </summary>
        long FileLength { get; }

        /// <summary>
        /// Gets or sets the last access time in UTC.
        /// </summary>
        DateTime LastAccessTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the last write time in UTC.
        /// </summary>
        DateTime LastWriteTimeUtc { get; set; }
    }
}
