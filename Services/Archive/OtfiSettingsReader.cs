using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NyxAssetsEditor.Services.Archive;

public sealed class OtfiSettings
{
	public bool? Extended { get; init; }
	public bool? Transparency { get; init; }
	public bool? FrameDurations { get; init; }
	public bool? FrameGroups { get; init; }
}

public static class OtfiSettingsReader
{
	public static OtfiSettings? ReadForArchive(string archivePath, out string? warning)
	{
		warning = null;
		var directory = Path.GetDirectoryName(Path.GetFullPath(archivePath));
		if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
		{
			warning = "OTFI settings could not be read because the archive directory does not exist. Using the selected loading settings.";
			return null;
		}

		var files = Directory.EnumerateFiles(directory, "*.otfi").ToList();
		if (files.Count == 0)
		{
			warning = "No .otfi file was found beside the archive. Using the selected loading settings.";
			return null;
		}

		var archiveName = Path.GetFileName(archivePath);
		var parsed = new List<(string Path, Dictionary<string, string> Values)>();
		foreach (var file in files)
		{
			try { parsed.Add((file, Parse(file))); }
			catch (Exception ex)
			{
				if (files.Count == 1)
				{
					warning = $"Could not read '{Path.GetFileName(file)}': {ex.Message} Using the selected loading settings.";
					return null;
				}
			}
		}

		string referenceKey = Path.GetExtension(archivePath).Equals(".spr", StringComparison.OrdinalIgnoreCase)
			? "sprites-file" : "metadata-file";
		var matches = parsed.Where(p => p.Values.TryGetValue(referenceKey, out var value)
			&& string.Equals(Path.GetFileName(value), archiveName, StringComparison.OrdinalIgnoreCase)).ToList();
		var selected = matches.Count == 1 ? matches[0] : parsed.Count == 1 ? parsed[0] : default;
		if (selected.Values == null)
		{
			warning = matches.Count > 1
				? $"Multiple .otfi files reference '{archiveName}'. Using the selected loading settings."
				: $"No unambiguous .otfi file references '{archiveName}'. Using the selected loading settings.";
			return null;
		}

		var invalid = new List<string>();
		bool? ReadBool(string key)
		{
			if (!selected.Values.TryGetValue(key, out var raw)) return null;
			if (bool.TryParse(raw, out var value)) return value;
			invalid.Add(key);
			return null;
		}

		var settings = new OtfiSettings
		{
			Extended = ReadBool("extended"),
			Transparency = ReadBool("transparency"),
			FrameDurations = ReadBool("frame-durations"),
			FrameGroups = ReadBool("frame-groups")
		};
		if (invalid.Count > 0)
			warning = $"'{Path.GetFileName(selected.Path)}' has invalid boolean value(s) for {string.Join(", ", invalid)}. Those settings will use the selected checkboxes.";
		return settings;
	}

	private static Dictionary<string, string> Parse(string path)
	{
		var lines = File.ReadAllLines(path);
		if (!lines.Any(line => string.Equals(line.Trim(), "DatSpr", StringComparison.OrdinalIgnoreCase)))
			throw new InvalidDataException("the DatSpr section is missing.");

		var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (var rawLine in lines)
		{
			var line = rawLine.Trim();
			if (line.Length == 0 || line.StartsWith('#')) continue;
			var separator = line.IndexOf(':');
			if (separator <= 0) continue;
			values[line[..separator].Trim()] = line[(separator + 1)..].Trim().Trim('"', '\'');
		}
		return values;
	}
}
