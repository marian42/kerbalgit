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

		public readonly Node Node;
		public readonly List<Part> parts;
		public readonly Orbit Orbit;

		public Vessel(Node node) {
			if (node.Name != "vessel") {
				throw new InvalidOperationException("Node is not a vessel.");
			}

			this.Node = node;
			this.parts = new List<Part>();
			this.Orbit = new Orbit(node.Get("orbit") as Node);
		}

		public string Id {
			get {
				return Node.GetValue("pid");
			}
		}

		public string Name {
			get {
				return Node.GetValue("name");
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
				return Enum.GetValues(typeof(Vessel.FlighState)).Cast<Vessel.FlighState>().First(flightState => flightState.ToString().ToLower() == Node.GetValue("sit").ToLower());
			}
		}

		public bool IsDebris {
			get {
				return Node.GetValue("type") == "Debris";
			}
		}

		public CelestialBody CelestialBody {
			get {
				return Orbit.CelestialBody;
			}
		}

		public string Location {
			get {
				return Node.GetValue("landedAt");
			}
		}

		public int Stage {
			get {
				return (int)Node.GetInt("stg");
			}
		}
	}
}
