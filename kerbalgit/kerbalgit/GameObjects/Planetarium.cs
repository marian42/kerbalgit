using System;
using System.Collections.Generic;
using System.Linq;

namespace kerbalgit.GameObjects {
	public class Planetarium {
		public static Lazy<Planetarium> Instance = new Lazy<Planetarium>();

		public readonly IReadOnlyCollection<CelestialBody> Bodies;

		public Planetarium() {
			var items = new List<CelestialBody>();

			items.Add(new CelestialBody("Kerbol", 0));
			items.Add(new CelestialBody("Kerbin", 1));
			items.Add(new CelestialBody("Moho", 4));
			items.Add(new CelestialBody("Eve", 5));
			items.Add(new CelestialBody("Gilly", 13));
			items.Add(new CelestialBody("Mun", 2));
			items.Add(new CelestialBody("Minmus", 3));
			items.Add(new CelestialBody("Duna", 6));
			items.Add(new CelestialBody("Ike", 7));
			items.Add(new CelestialBody("Dres", 15));
			items.Add(new CelestialBody("Jool", 8));
			items.Add(new CelestialBody("Laythe", 9));
			items.Add(new CelestialBody("Vall", 10));
			items.Add(new CelestialBody("Tylo", 12));
			items.Add(new CelestialBody("Bop", 11));
			items.Add(new CelestialBody("Pol", 14));
			items.Add(new CelestialBody("Eelo", 16));

			Bodies = items.AsReadOnly();
		}

		public static CelestialBody ById(int id) {
			return Instance.Value.Bodies.First(body => body.Id == id);
		}
	}
}
