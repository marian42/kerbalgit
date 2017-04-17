using kerbalgit.Tree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kerbalgit.GameObjects {
	public class Vessel {
		public enum FlighState {
			Orbiting,
			Prelaunch,
			Landed,
			Splashed,
			Sub_Orbital,
			Flying,
			Escaping
		}

		public enum VesselType {
			Ship,
			Probe,
			Lander,
			Flag,
			Debris,
			Asteroid,
			EVA,
			SpaceObject,
			Rover,
			Base,
			Relay
		}

		public enum DiscoveryLevel {
			Owned = -1,
			None = 0,
			Presence = 1,
			Name = 4,
			StateVectors = 8,
			Appearance = 16,
			Unowned = 29,
		}

		public readonly Node Node;
		public readonly IEnumerable<Part> Parts;
		public readonly Orbit Orbit;

		public Vessel(Node node) {
			if (node.Name != "vessel") {
				throw new InvalidOperationException("Node is not a vessel.");
			}

			this.Node = node;
			this.Parts = getParts();
			this.Orbit = new Orbit(node.Get("orbit") as Node);
		}

		public string Id {
			get {
				var pid = Node.GetValue("pid");
				if (pid == "00000000000000000000000000000000") {
					return Name;
				}
				return pid;
			}
		}

		public string Name {
			get {
				return Node.GetValue("name").Substring(0, 1).ToUpper() + Node.GetValue("name").Substring(1);
			}
		}

		public override int GetHashCode() {
			return Id.GetHashCode();
		}

		public override bool Equals(object obj) {
			return obj is Vessel && (obj as Vessel).Id == Id;
		}

		public string StatusReport {
			get {
				var result = string.Empty;
				result += "Name: " + Name + "\n";
				result += "Flightstate: " + FlightStateValue.ToString() + "\n";
				result += "Orbit: " + Orbit.GetName() + "\n";

				return result;
			}
		}

		public FlighState FlightStateValue {
			get {
				try {
					return Enum.GetValues(typeof(Vessel.FlighState)).Cast<Vessel.FlighState>().First(flightState => flightState.ToString().ToLower() == Node.GetValue("sit").ToLower());
				}
				catch (InvalidOperationException) {
					throw new NotImplementedException("Unknown flight state: " + Node.GetValue("type"));
				}	

			}
		}

		public bool InFlight {
			get {
				return FlightStateValue == FlighState.Escaping || FlightStateValue == FlighState.Orbiting || FlightStateValue == FlighState.Sub_Orbital;
			}
		}

		public VesselType Type {
			get {
				try {
					return Enum.GetValues(typeof(Vessel.VesselType)).Cast<Vessel.VesselType>().First(vesselType => vesselType.ToString().ToLower() == Node.GetValue("type").ToLower());
				}
				catch (InvalidOperationException) {
					throw new NotImplementedException("Unknown vessel type: " + Node.GetValue("type"));
				}				
			}
		}

		public CelestialBody CelestialBody {
			get {
				return Orbit.CelestialBody;
			}
		}

		public string Location {
			get {
				switch (FlightStateValue) {
					case FlighState.Escaping:
						return "escaping from " + CelestialBody;
					case FlighState.Flying:
						return "flying over " + CelestialBody;
					case FlighState.Landed:
						return "landed on " + CelestialBody;
					case FlighState.Orbiting:
						return "orbiting around " + CelestialBody;
					case FlighState.Prelaunch:
						return "in preparation of launch";
					case FlighState.Splashed:
						return "splashed down on " + CelestialBody;
					case FlighState.Sub_Orbital:
						return "on a suborbital trajectory above " + CelestialBody;
					default: throw new NotImplementedException(FlightStateValue.ToString());
				}
			}
		}

		public int Stage {
			get {
				return (int)Node.GetInt("stg");
			}
		}

		public DiscoveryLevel DiscoveryLevelValue {
			get {
				var i = (int)Node.GetInt("discovery/state");
				return Enum.GetValues(typeof(Vessel.DiscoveryLevel)).Cast<Vessel.DiscoveryLevel>().First(level => (int)level == i);  
			}
		}

		public bool Owned {
			get {
				return DiscoveryLevelValue == DiscoveryLevel.Owned;
			}
		}

		public override string ToString() {
			return Name;
		}

		public IEnumerable<ScienceData> ScienceData {
			get {
				return Parts.SelectMany(part => part.ScienceData);
			}
		}

		private IEnumerable<Part> getParts() {
			foreach (var node in Node.Children.OfType<Node>().Where(node => node.Name == "part")) {
				yield return new Part(node as Node, this);
			}
		}
	}
}
