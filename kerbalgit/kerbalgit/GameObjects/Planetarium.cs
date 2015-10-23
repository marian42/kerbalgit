using System;
using System.Collections.Generic;
using System.Linq;

namespace kerbalgit.GameObjects {
	public class Planetarium {
		public static Lazy<Planetarium> Instance = new Lazy<Planetarium>();

		public readonly IReadOnlyCollection<CelestialBody> Bodies;

		public Planetarium() {
			var items = new List<CelestialBody>();

			items.Add(new CelestialBody("Kerbol", 0, 261600000, 0, 1000000000, double.PositiveInfinity));
			items.Add(new CelestialBody("Kerbin", 1, 600000, 70000, 250000, 84159286));
			items.Add(new CelestialBody("Moho", 4, 250000, 0, 80000, 9646663));
			items.Add(new CelestialBody("Eve", 5, 700000, 90000, 400000, 85109365));
			items.Add(new CelestialBody("Gilly", 13, 13000, 0, 6000,  126123));
			items.Add(new CelestialBody("Mun", 2, 200000, 0, 60000,  2429559));
			items.Add(new CelestialBody("Minmus", 3, 60000, 0, 30000, 2247428));
			items.Add(new CelestialBody("Duna", 6, 320000, 50000, 140000, 47921949));
			items.Add(new CelestialBody("Ike", 7, 130000, 0, 50000, 1049599));
			items.Add(new CelestialBody("Dres", 15, 138000, 0, 25000, 32832840));
			items.Add(new CelestialBody("Jool", 8, 6000000, 200000, 4000000, 2455985185));
			items.Add(new CelestialBody("Laythe", 9, 500000, 50000, 200000, 3723646));
			items.Add(new CelestialBody("Vall", 10, 300000, 0, 90000, 2406401));
			items.Add(new CelestialBody("Tylo", 12, 600000, 0, 250000, 10856518));
			items.Add(new CelestialBody("Bop", 11, 65000, 0, 25000, 1221061));
			items.Add(new CelestialBody("Pol", 14, 44000, 0, 22000, 1042139));
			items.Add(new CelestialBody("Eelo", 16, 210000, 0, 60000, 119082942));

			Bodies = items.AsReadOnly();
		}

		public static CelestialBody ById(int id) {
			return Instance.Value.Bodies.First(body => body.Id == id);
		}
	}
}
