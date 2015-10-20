using kerbalgit.GameObjects;
using System.Collections.Generic;

namespace kerbalgit.Diff {
	class VesselInfo {
		public readonly Vessel Vessel;
		public bool LostOrGainedParts;
		private List<Vessel> correspondingVessels;

		public VesselInfo(Vessel vessel) {
			this.Vessel = vessel;
			LostOrGainedParts = false;
			correspondingVessels = new List<Vessel>();
		}

		public void AddCorrespondingVessel(Vessel vessel) {
			if (!correspondingVessels.Contains(vessel)) {
				correspondingVessels.Add(vessel);
			}
		}

		public IReadOnlyCollection<Vessel> CorrespondingVessels {
			get {
				return correspondingVessels.AsReadOnly();
			}
		}
	}
}
