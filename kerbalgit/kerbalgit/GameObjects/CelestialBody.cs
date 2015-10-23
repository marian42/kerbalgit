
namespace kerbalgit.GameObjects {
	public class CelestialBody {
		public readonly int Id;
		public readonly string Name;

		public readonly double Radius;
		public readonly double EdgeOfAtmosphere;
		public readonly double EdgeOfLowSpace;
		public readonly double SphereOfInfluence;

		public CelestialBody(string name, int id, double radius, double edgeOfAtmosphere, double edgeOfLowSpace, double sphereOfInfluence) {
			Id = id;
			Name = name;

			Radius = radius;
			EdgeOfAtmosphere = edgeOfAtmosphere;
			EdgeOfLowSpace = edgeOfLowSpace;
			SphereOfInfluence = sphereOfInfluence;
		}
	}
}
