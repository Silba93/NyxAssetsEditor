using System.Collections.Generic;
using NyxAssets.Things;

namespace NyxAssetsEditor.Services.Archive
{
	public class ThingUndoAction
	{
		public uint ItemCountBefore { get; set; }
		public uint ItemCountAfter { get; set; }

		public uint OutfitCountBefore { get; set; }
		public uint OutfitCountAfter { get; set; }

		public uint EffectCountBefore { get; set; }
		public uint EffectCountAfter { get; set; }

		public uint MissileCountBefore { get; set; }
		public uint MissileCountAfter { get; set; }

		public Dictionary<ThingKind, Dictionary<uint, ThingType>> ThingsBefore { get; } = new()
		{
			{ ThingKind.Item, new() },
			{ ThingKind.Outfit, new() },
			{ ThingKind.Effect, new() },
			{ ThingKind.Missile, new() }
		};

		public Dictionary<ThingKind, Dictionary<uint, ThingType>> ThingsAfter { get; } = new()
		{
			{ ThingKind.Item, new() },
			{ ThingKind.Outfit, new() },
			{ ThingKind.Effect, new() },
			{ ThingKind.Missile, new() }
		};

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
