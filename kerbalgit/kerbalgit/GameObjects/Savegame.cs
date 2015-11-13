using kerbalgit.Tree;
using System.Collections.Generic;
using System.Linq;

namespace kerbalgit.GameObjects {
	public class Savegame {
		public readonly RootNode Node;
		public readonly IEnumerable<Vessel> Vessels;

		public Savegame(RootNode node) {
			this.Node = node;

			Vessels = findVessels();
		}

		private IEnumerable<Vessel> findVessels() {
			var flightstateNode = Node.Get("flightstate") as Node;

			foreach (var node in flightstateNode.Children.OfType<Node>().Where(node => node.Name == "vessel")) {
				yield return new Vessel(node as Node);
			}
		}

		public double Time {
			get {
				return Node.GetDouble("flightstate/UT");
			}
		}
	}
}
