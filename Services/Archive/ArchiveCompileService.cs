using System;
using System.IO;
using NyxAssets.Sprites;
using NyxAssets.Things;
using NyxAssetsEditor.ViewModels.ArchiveLoaders;
using NyxAssetsEditor.ViewModels.Common;

namespace NyxAssetsEditor.Services.Archive
{
	public static class ArchiveCompileService
	{
		public static void CompilePair(
			FloatingSpriteLoaderViewModel spritePanel,
			FloatingThingsLoaderViewModel thingsPanel,
			string spriteOutputPath,
			string thingsOutputPath)
		{
			var spriteFormat = ArchiveFormatHelper.FromPath(spritePanel.FilePath);
			var thingsFormat = ArchiveFormatHelper.FromPath(thingsPanel.FilePath);

			if (!ArchiveFormatHelper.AreCompatible(spriteFormat, thingsFormat))
				throw new InvalidOperationException($"Cannot compile {spriteFormat} with {thingsFormat}. Use spr+dat or assets+things.");

			var catalog = thingsPanel.Catalog
				?? throw new InvalidOperationException("Things catalog is not loaded.");

			var options = thingsPanel.GetWriteOptions();

			if (spriteFormat == ArchiveFormat.Spr)
			{
				spritePanel.Loader.WriteSprTo(spriteOutputPath);
				using var datStream = File.Create(thingsOutputPath);
				catalog.WriteDatTo(datStream, options);
			}
			else
			{
				spritePanel.Loader.WriteAssetsTo(spriteOutputPath);
				catalog.ExportJson(thingsOutputPath, options);
			}
		}

		public static void BackupIfExists(string path)
		{
			if (!File.Exists(path))
				return;

			var backupPath = path + ".bak";
			using var source = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			using var destination = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
			source.CopyTo(destination);
		}
	}
}
