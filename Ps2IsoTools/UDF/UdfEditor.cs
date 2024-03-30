using Ps2IsoTools.UDF.Files;

namespace Ps2IsoTools.UDF
{
    public class UdfEditor : IDisposable
    {
        private string readerPath;
        private UdfReader reader;
        private UdfBuilder builder;
        private Dictionary<FileIdentifier, RebuildFile> rebuildFiles;

        public UdfEditor(string inputFile, string copyTo = "")
        {
            if (!string.IsNullOrEmpty(copyTo))
            {
                System.IO.File.Copy(inputFile, copyTo, true);
                if (System.IO.File.Exists(copyTo))
                    inputFile = copyTo;
            }
            readerPath = inputFile;
            InitializeReader(readerPath);
        }

        private void InitializeReader(string fileToRead)
        {
            reader?.Dispose();
            reader = new UdfReader(fileToRead);
            builder = new();
            rebuildFiles = new();

            foreach (string fileName in reader.GetAllFileFullNames())
            {
                FileIdentifier fi = reader.GetFileByName(fileName)!;
                rebuildFiles.Add(fi, new(fileName, reader.GetFileStream(fi)));
            }
        }

        public void Rebuild(string outputFile = "")
        {
            if (outputFile == "" || string.Compare(outputFile, readerPath, StringComparison.OrdinalIgnoreCase) == 0)
            {
                foreach (RebuildFile rf in rebuildFiles.Values)
                    rf.ConvertToMemoryStream();
                reader.Dispose();
                outputFile = readerPath;
            }
            builder.VolumeIdentifier = reader.VolumeLabel;

            foreach(RebuildFile rf in rebuildFiles.Values)
            {
                builder.AddFile(rf.FullName, rf.FileStream);
            }

            builder.Build(outputFile);

            InitializeReader(outputFile);
        }

        public FileIdentifier? GetFileByName(string fileName) => reader.GetFileByName(fileName);
        public Stream GetFileStream(FileIdentifier file) => reader.GetWritableFileStream(file);

        public void ReplaceFileStream(FileIdentifier fileIdentifier, Stream newSource, bool makeCopy = false)
        {
            MemoryStream copy = new();
            if (makeCopy)
            {
                newSource.CopyTo(copy);
                copy.Position = 0;
            }
            rebuildFiles[fileIdentifier].FileStream = makeCopy ? copy : newSource;
        }

        public bool AddFile(string fileFullName, string filePath) => builder.AddFile(fileFullName, filePath) is not null;
        public bool AddFile(string fileFullName, byte[] fileData) => builder.AddFile(fileFullName, fileData) is not null;
        public bool AddFile(string fileFullName, Stream fileStream) => builder.AddFile(fileFullName, fileStream) is not null;
        public bool AddDirectory(string directory) => builder.AddDirectory(directory) is not null;

        public bool RemoveFile(FileIdentifier file) => rebuildFiles.Remove(file);

        public void Dispose()
        {
            reader.Dispose();
        }

        private class RebuildFile
        {
            public string FullName { get; set; }
            public Stream FileStream { get; set; }

            public RebuildFile(string fullName, Stream fileStream)
            {
                FullName = fullName;
                FileStream = fileStream;
            }

            public void ConvertToMemoryStream()
            {
                MemoryStream myStream = new();
                FileStream.Position = 0;
                FileStream.CopyTo(myStream);
                FileStream.Dispose();
                FileStream = myStream;
            }
        }
    }

    
}
