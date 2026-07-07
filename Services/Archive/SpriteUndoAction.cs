using System.Collections.Generic;

namespace NyxAssetsEditor.Services.Archive
{
	public class SpriteUndoAction
	{
		public uint SpriteCountBefore { get; set; }
		public uint SpriteCountAfter { get; set; }

		public Dictionary<uint, byte[]> PixelsBefore { get; } = new();
		public Dictionary<uint, byte[]> PixelsAfter { get; } = new();

		public HashSet<uint> AddedBefore { get; set; } = new();
		public HashSet<uint> AddedAfter { get; set; } = new();

		public HashSet<uint> RemovedBefore { get; set; } = new();
		public HashSet<uint> RemovedAfter { get; set; } = new();

		public HashSet<uint> ModifiedBefore { get; set; } = new();
		public HashSet<uint> ModifiedAfter { get; set; } = new();

		public bool HasSavedChangesBefore { get; set; }
		public bool HasSavedChangesAfter { get; set; }
	}
}
