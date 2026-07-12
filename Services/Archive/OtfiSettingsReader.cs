using System;
using System.IO;
using NyxAssets.Client;

namespace NyxAssetsEditor.Services.Archive;

public static class OtfiSettingsReader
{
	public static OtfiFile? ReadForArchive(string archivePath, out string? warning)
	{
		warning = null;
		var directory = Path.GetDirectoryName(Path.GetFullPath(archivePath))!;
		var otfiPath = Path.ChangeExtension(archivePath, ".otfi");

		if (!File.Exists(otfiPath))
		{
			var files = Directory.GetFiles(directory, "*.otfi");
			if (files.Length != 1)
			{
				warning = files.Length == 0
					? "No .otfi file was found beside the archive."
					: $"No '{Path.GetFileName(otfiPath)}' was found and the directory contains multiple .otfi files.";
				return null;
			}
			otfiPath = files[0];
		}

		try
		{
			return OtfiFile.Load(otfiPath);
		}
		catch (Exception ex)
		{
			warning = $"Could not read '{Path.GetFileName(otfiPath)}': {ex.Message}.";
			return null;
		}
	}
}
