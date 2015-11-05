using kerbalgit.Tree;
using System;

namespace kerbalgit.GameObjects {
	public class Orbit {
		public enum NamedSemiMajorAxis {
			Impact,
			Unstable,
			Low,
			High,
			Escape
		}

		public enum NamedInclination {
			Equatorial,
			Inclined,
			Polar
		}

		public enum NamedEccentricity {
			Circular,
			Eccentric,
			Parabolic
		}

		public readonly Node Node;

		public Orbit(Node node) {
			if (node.Name != "orbit") {
				throw new InvalidOperationException("Node is not an orbit.");
			}

			this.Node = node;			
		}

		public double Eccentricity {
			get {
				return Node.GetDouble("ECC");
			}
		}

		public double Inclination {
			get {
				return Node.GetDouble("INC");
			}
		}

		public double SemiMajorAxis {
			get {
				return Node.GetDouble("SMA");
			}
		}

		public double SemiMinorAxis {
			get {
				return SemiMajorAxis * Math.Sqrt(1.0 - Math.Pow(Eccentricity, 2));
			}
		}

		public NamedEccentricity NamedEccentricityValue {
			get {
				if (Eccentricity < 0.2) {
					return NamedEccentricity.Circular;
				} else if (Eccentricity < 1) {
					return NamedEccentricity.Eccentric;
				} else return NamedEccentricity.Parabolic;
			}
		}

		public NamedInclination NamedInclinationValue {
			get {
				if ((Inclination > -5 && Inclination < 5) || (Inclination > 175 || Inclination < 185)) {
					return NamedInclination.Equatorial;
				} else if ((Inclination > 85 && Inclination < 95) || (Inclination < - 85) || (Inclination > 265)) {
					return NamedInclination.Polar;
				} else return NamedInclination.Inclined;
			}
		}

		public double MinAltitude {
			get {
				return SemiMinorAxis - CelestialBody.Radius;
			}
		}

		public NamedSemiMajorAxis NamedSemiMajorAxisValue {
			get {
				if (SemiMajorAxis > CelestialBody.SphereOfInfluence) {
					return NamedSemiMajorAxis.Escape;
				} else if (SemiMinorAxis < CelestialBody.Radius) {
					return NamedSemiMajorAxis.Impact;
				} else if (MinAltitude < CelestialBody.EdgeOfAtmosphere) {
					return NamedSemiMajorAxis.Unstable;
				} else if (MinAltitude < CelestialBody.EdgeOfLowSpace) {
					return NamedSemiMajorAxis.Low;
				} else return NamedSemiMajorAxis.High;
			}
		}

		public bool Stable {
			get {
				return NamedSemiMajorAxisValue == NamedSemiMajorAxis.High || NamedSemiMajorAxisValue == NamedSemiMajorAxis.Low;
			}
		}

		public CelestialBody CelestialBody {
			get {
				return Planetarium.ById((int)Node.GetInt("REF"));
			}
		}

		private string enumToString(Enum value) {
			if (value.ToString() == NamedEccentricity.Circular.ToString() || value.ToString() == NamedInclination.Equatorial.ToString() || value.ToString() == "Normal") {
				return string.Empty;
			}
			return value.ToString().ToLower();
		}

		public string GetName(bool includeBody = true, bool includeArticle = true) {
			var result = string.Empty;

			if (Stable) {
				result = enumToString(NamedEccentricityValue) + " " + enumToString(NamedInclinationValue) + " ";

				if (NamedEccentricityValue == NamedEccentricity.Circular) {
					result += CommitMessage.AddMetricPrefix(MinAltitude) + "m";
				} else {
					result += enumToString(NamedSemiMajorAxisValue);
				}				
					
				result += " orbit";
				if (includeBody) {
					result += " around " + this.CelestialBody.Name;
				}				
			} else {
				result = enumToString(NamedSemiMajorAxisValue) + " trajectory ";

				if (includeBody) {
					switch (NamedSemiMajorAxisValue) {
						case NamedSemiMajorAxis.Impact:
							result += "into"; break;
						case NamedSemiMajorAxis.Unstable:
							result += "around"; break;
						case NamedSemiMajorAxis.Escape:
							result += "out of"; break;
						default: throw new NotImplementedException(NamedSemiMajorAxisValue.ToString());
					}
					result += " " + CelestialBody.Name;
				}
			}

			result = result.Replace("  ", " ").Trim();

			if (includeArticle) {
				result = ("aeiou8".Contains(result.Trim()[0] + "") ? "an " : "a ") + result;
			}

			return result;
		}

		public bool IsSimilar(Orbit orbit, bool compareAltitudeNumerically) {
			return (!compareAltitudeNumerically || Math.Abs(MinAltitude - orbit.MinAltitude) < 15000)
				&& NamedEccentricityValue == orbit.NamedEccentricityValue
				&& NamedInclinationValue == orbit.NamedInclinationValue
				&& NamedSemiMajorAxisValue == orbit.NamedSemiMajorAxisValue;
		}

		public override string ToString() {
			return GetName(true, false);
		}
	}
}
