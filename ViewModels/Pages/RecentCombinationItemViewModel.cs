using CommunityToolkit.Mvvm.Input;
using System.IO;
using NyxAssetsEditor.ViewModels.Core;

namespace NyxAssetsEditor.ViewModels.Pages
{
	public partial class RecentCombinationItemViewModel : ViewModelBase
	{
		private readonly HomeViewModel _parent;

		public string SpritePath { get; }
		public string ThingsPath { get; }

		// Sprite settings
		public bool SpriteGuessSettingsFromSignature { get; }
		public bool SpritePreferOtfiSettings { get; }
		public bool SpriteUseTransparentPixels { get; }
		public bool SpriteUseExtendedSpriteIds { get; }

		// Things settings
		public bool ThingsGuessSettingsFromSignature { get; }
		public bool ThingsPreferOtfiSettings { get; }
		public bool ThingsUseExtendedThingIds { get; }
		public bool ThingsUseFrameAnimations { get; }
		public bool ThingsUseFrameGroups { get; }

		public string DisplayName { get; }
		public string DetailsText { get; }
		public string ToolTipText { get; }
		public string ProjectName { get; private set; } = "";
		public bool HasProjectName => !string.IsNullOrEmpty(ProjectName);
		public bool HasBoth => !string.IsNullOrEmpty(SpritePath) && !string.IsNullOrEmpty(ThingsPath);
		public bool HasSpriteOnly => !string.IsNullOrEmpty(SpritePath) && string.IsNullOrEmpty(ThingsPath);
		public bool HasThingsOnly => string.IsNullOrEmpty(SpritePath) && !string.IsNullOrEmpty(ThingsPath);

		public RecentCombinationItemViewModel(
			string spritePath,
			string thingsPath,
			HomeViewModel parent,
			bool spriteGuess = true,
			bool spritePreferOtfi = false,
			bool spriteTransparent = true,
			bool spriteExtended = true,
			bool thingsGuess = true,
			bool thingsPreferOtfi = false,
			bool thingsExtended = true,
			bool thingsAnimations = true,
			bool thingsGroups = true)
		{
			SpritePath = spritePath;
			ThingsPath = thingsPath;
			_parent = parent;

			SpriteGuessSettingsFromSignature = spriteGuess;
			SpritePreferOtfiSettings = spritePreferOtfi;
			SpriteUseTransparentPixels = spriteTransparent;
			SpriteUseExtendedSpriteIds = spriteExtended;
			ThingsGuessSettingsFromSignature = thingsGuess;
			ThingsPreferOtfiSettings = thingsPreferOtfi;
			ThingsUseExtendedThingIds = thingsExtended;
			ThingsUseFrameAnimations = thingsAnimations;
			ThingsUseFrameGroups = thingsGroups;

			string sprName = string.IsNullOrEmpty(spritePath) ? "" : Path.GetFileName(spritePath);
			string datName = string.IsNullOrEmpty(thingsPath) ? "" : Path.GetFileName(thingsPath);

			string dir = "";
			if (!string.IsNullOrEmpty(sprName) && !string.IsNullOrEmpty(datName))
			{
				DisplayName = $"{datName} + {sprName}";
				dir = Path.GetDirectoryName(thingsPath) ?? "";
				ToolTipText = $"DAT: {thingsPath}\nSPR: {spritePath}";
			}
			else if (!string.IsNullOrEmpty(datName))
			{
				DisplayName = datName;
				dir = Path.GetDirectoryName(thingsPath) ?? "";
				ToolTipText = $"DAT: {thingsPath}";
			}
			else if (!string.IsNullOrEmpty(sprName))
			{
				DisplayName = sprName;
				dir = Path.GetDirectoryName(spritePath) ?? "";
				ToolTipText = $"SPR: {spritePath}";
			}
			else
			{
				DisplayName = "Unknown Archive";
				ToolTipText = "";
			}

			DetailsText = CompactPath(dir);
			ProjectName = InferProjectName(dir);
		}

		private string InferProjectName(string dirPath)
		{
			if (string.IsNullOrEmpty(dirPath))
				return "";

			char sep = Path.DirectorySeparatorChar;
			var parts = dirPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
			
			int projectIdx = -1;
			for (int i = 0; i < parts.Length; i++)
			{
				var part = parts[i].ToLowerInvariant();
				if (part == "data" || part == "things" || part == "assets" || part == "datprotocols" || 
				    (part.StartsWith("v") && part.Length > 1 && char.IsDigit(part[1])) ||
				    int.TryParse(part, out _))
				{
					if (i > 0)
					{
						var prev = parts[i - 1].ToLowerInvariant();
						if (prev != "desktop" && prev != "documents" && prev != "downloads" && prev != "users" && prev != "")
						{
							projectIdx = i - 1;
							break;
						}
					}
				}
			}

			if (projectIdx == -1)
			{
				if (parts.Length > 1)
				{
					var prev = parts[parts.Length - 2].ToLowerInvariant();
					if (prev != "desktop" && prev != "documents" && prev != "downloads" && prev != "users" && prev != "")
					{
						projectIdx = parts.Length - 2;
					}
					else
					{
						projectIdx = parts.Length - 1;
					}
				}
				else if (parts.Length == 1)
				{
					projectIdx = 0;
				}
			}

			if (projectIdx >= 0 && projectIdx < parts.Length)
			{
				var name = parts[projectIdx];
				if (name != "desktop" && name != "documents" && name != "downloads" && name != "users" && name != "")
				{
					return name;
				}
			}

			return "";
		}

		private static string CompactPath(string path, int maxLength = 35)
		{
			if (string.IsNullOrEmpty(path))
				return "";

			if (path.Length <= maxLength)
				return path;

			var separator = Path.DirectorySeparatorChar;
			var parts = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, System.StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
				return path;

			string result = parts[parts.Length - 1];
			for (int i = parts.Length - 2; i >= 0; i--)
			{
				string candidate = parts[i] + separator + result;
				if (($"...{separator}{candidate}").Length > maxLength)
				{
					break;
				}
				result = candidate;
			}

			return $"...{separator}{result}";
		}

		[RelayCommand]
		private void Load()
		{
			var missing = new System.Collections.Generic.List<string>();
			if (!string.IsNullOrEmpty(SpritePath) && !File.Exists(SpritePath))
				missing.Add(SpritePath);
			if (!string.IsNullOrEmpty(ThingsPath) && !File.Exists(ThingsPath))
				missing.Add(ThingsPath);

			if (missing.Count > 0)
			{
				_parent.NotifyMissingRecentCombination(this, missing);
				return;
			}

			_parent.LoadCombination(
				SpritePath,
				ThingsPath,
				SpriteGuessSettingsFromSignature,
				SpritePreferOtfiSettings,
				SpriteUseTransparentPixels,
				SpriteUseExtendedSpriteIds,
				ThingsGuessSettingsFromSignature,
				ThingsPreferOtfiSettings,
				ThingsUseExtendedThingIds,
				ThingsUseFrameAnimations,
				ThingsUseFrameGroups
			);
		}

		[RelayCommand]
		private void Remove()
		{
			_parent.RemoveCombination(this);
		}
	}
}
