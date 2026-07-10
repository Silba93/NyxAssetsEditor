using System;
using System.Collections.Generic;
using System.IO;

namespace NyxAssetsEditor.Services.Archive;

public sealed record OtfiSettings(
	bool? Extended,
	bool? Transparency,
	bool? FrameDurations,
	bool? FrameGroups);

public static class OtfiSettingsReader
{
	public static OtfiSettings? ReadForArchive(string archivePath, out string? warning)
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
					? "No .otfi file was found beside the archive. Using the selected loading settings."
					: $"No '{Path.GetFileName(otfiPath)}' was found and the directory contains multiple .otfi files. Using the selected loading settings.";
				return null;
			}
			otfiPath = files[0];
		}

		try
		{
			var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var hasDatSprSection = false;
			foreach (var rawLine in File.ReadLines(otfiPath))
			{
				var line = rawLine.Trim();
				if (line.Equals("DatSpr", StringComparison.OrdinalIgnoreCase))
				{
					hasDatSprSection = true;
					continue;
				}

				var separator = line.IndexOf(':');
				if (separator > 0)
					values[line[..separator].Trim()] = line[(separator + 1)..].Trim();
			}

			if (!hasDatSprSection)
				throw new InvalidDataException("the DatSpr section is missing");

			var invalid = new List<string>();
			bool? ReadBool(string key)
			{
				if (!values.TryGetValue(key, out var raw)) return null;
				if (bool.TryParse(raw, out var value)) return value;
				invalid.Add(key);
				return null;
			}

			var settings = new OtfiSettings(
				ReadBool("extended"),
				ReadBool("transparency"),
				ReadBool("frame-durations"),
				ReadBool("frame-groups"));

			if (invalid.Count > 0)
				warning = $"'{Path.GetFileName(otfiPath)}' has invalid values for {string.Join(", ", invalid)}. Using the selected checkboxes for those settings.";
			return settings;
		}
		catch (Exception ex)
		{
			warning = $"Could not read '{Path.GetFileName(otfiPath)}': {ex.Message}. Using the selected loading settings.";
			return null;
		}
	}
}
