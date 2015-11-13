using kerbalgit.Tree;
using System.Collections.Generic;
using System.Linq;

namespace kerbalgit.GameObjects {
	public class Savegame {
		public readonly RootNode Node;
		public readonly IEnumerable<Vessel> Vessels;
		public readonly IEnumerable<TechTreeNode> ResearchedTech;

		public Savegame(RootNode node) {
			this.Node = node;

			Vessels = findVessels();
			ResearchedTech = findTech();
		}

		private IEnumerable<Vessel> findVessels() {
			var flightstateNode = Node.Get("flightstate") as Node;

			foreach (var node in flightstateNode.Children.OfType<Node>().Where(node => node.Name == "vessel")) {
				yield return new Vessel(node as Node);
			}
		}

		private IEnumerable<TechTreeNode> findTech() {
			var scenarioNode = Node.Children.FirstOrDefault(child => child.Name == "scenario" && child.GetValue("name") == "ResearchAndDevelopment") as Node;

			if (scenarioNode == null) {
				yield break;
			}

			foreach (var childNode in scenarioNode.Children) {
				if (childNode.Name == "tech") {
					yield return new TechTreeNode(childNode as Node);
				}
			}
		}

		public double Time {
			get {
				return Node.GetDouble("flightstate/UT");
			}
		}
	}
}
