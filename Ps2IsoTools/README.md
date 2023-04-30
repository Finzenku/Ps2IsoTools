# About

Ps2IsoTools is an ISO editor designed to work with the UDF file system, specifically for use with PS2 games. Ps2IsoTools is heavily derived from [DiscUtils](https://github.com/DiscUtils/DiscUtils) and includes many of its ISO, UDF, and other classes.

Meta data about files such as Creation Date and Last Modified Date are lost when building or rebuilding ISOs.

# How to Use

### Reading a UDF format ISO with UdfReader
``` csharp
using (var reader = new UdfReader("path-to-local-iso.iso"))
{
	// Get list of all files
	List<string> fullNames = reader.GetAllFileFullNames();

	// FileIdentifiers are used to reference files on the ISO
	FileIdentifier? fileId = reader.GetFileByName("file-name");

	if (fileId is not null)
	{
		// Read data from file
		using (BinaryReader br = new(reader.GetFileStream(fileId)))
		{
			Console.WriteLine(br.ReadString());
		}

		// Copy file from the ISO to your local drive
		reader.CopyFile(fileId, "path-to-copy-to");
	}
}
```

### Building a UDF format ISO with UdfBuilder
``` csharp
var builder = new UdfBuilder();
builder.VolumeIdentifier = "volume-identifier";

// Add file via byte array
builder.AddFile("text.txt", Encoding.ASCII.GetBytes("Text file text"));

// Add directory
builder.AddDirectory("directory");

// Add file from local drive
builder.AddFile(@"directory\file.bin", "path-to-local-file");

// Add file from Stream
// Has optional bool parameter to make a copy of the Stream in RAM (default: false)
//  so that it is safe to close before calling Build()
using (FileStream fs = File.Open("path-to-local-file", FileMode.Open))
{
	builder.AddFile(@"directory\streams\file.dat", fs, true);
}

builder.Build("path-to-output-iso.iso");
```

### Editing a UDF format ISO with UdfEditor
``` csharp
// Has string optional parameter to copy the ISO immediataely so that the original is preserved
using (var editor = new UdfEditor("path-to-local-iso.iso", "optional-path-to-copy-to.iso"))
{
	var fileId = editor.GetFileByName("file-name");
	if (fileId is not null)
	{
		// Write directly to a specific file on the ISO
		// Does not require Rebuild()
		using (BinaryWriter bw = new(editor.GetFileStream(fileId)))
		{
			var data = new byte[] { 0xFF, 0xFF };
			bw.Write(data);
		}

		// Replace a file entirely
		// Has optional bool parameter to make a copy of the Stream in RAM (default: false)
		// ISO must be rebuilt with Rebuild() to save changes
		// Does not edit file name, only contents
		using (FileStream fs = File.Open("path-to-local-file", FileMode.Open))
		{
			editor.ReplaceFileStream(fileId, fs, true);
		}

		// Remove a file from the ISO
		// Requires Rebuild()
		editor.RemoveFile(fileId);

		// Add a file, same as UdfBuilder
		// Requires Rebuild()
		editor.AddFile("Text.txt", Encoding.ASCII.GetBytes("Text file text"));

		// Add a directory, same as UdfBuilder
		// Requires Rebuild()
		editor.AddDirectory(@"NewDirectory\DATA");
	}

	// Rebuild the ISO (Creates entirely new meta data using UdfBuilder)
	// Optional string parameter to output a new ISO file
	// Overwriting the current ISO will temporarily cause a spike in RAM usage ~2x total ISO size
	//  as all of the files will be stored in MemoryStreams during the process
	editor.Rebuild("optional-path-to-build.iso");
}
```
