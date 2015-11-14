using kerbalgit.Tree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kerbalgit.GameObjects {
	public class Part {
		public readonly Node node;
		public readonly Vessel vessel;
		public readonly IEnumerable<ScienceData> ScienceData;

		public Part(Node node, Vessel vessel) {
			if (node.Name != "part") {
				throw new InvalidOperationException("Node is not a part.");
			}

			this.node = node;
			this.vessel = vessel;
			ScienceData = getScienceData();
		}

		public long Id {
			get {
				return node.GetInt("uid");
			}
		}

		public override int GetHashCode() {
			return Id.GetHashCode();
		}

		public override bool Equals(object obj) {
			return obj is Part && (obj as Part).Id == Id;
		}

		private IEnumerable<ScienceData> getScienceData() {
			foreach (var scienceContainer in node.Children.Where(childNode => childNode.Name == "module" && (childNode.GetValue("name") == "ModuleScienceContainer" || childNode.GetValue("name") == "ModuleScienceExperiment"))) {
				foreach (var childNode in (scienceContainer as Node).Children) {
					if (childNode.Name == "sciencedata") {
						yield return new ScienceData(childNode as Node);
					}
				}
			}
		}
	}
}
