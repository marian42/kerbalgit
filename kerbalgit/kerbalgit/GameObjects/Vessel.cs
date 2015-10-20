using kerbalgit.Tree;
using System;
using System.Collections.Generic;

namespace kerbalgit.GameObjects {
	public class Vessel {
		public readonly Node Node;
		public readonly List<Part> parts;

		public Vessel(Node node) {
			if (node.Name != "vessel") {
				throw new InvalidOperationException("Node is not a vessel.");
			}

			this.Node = node;
			this.parts = new List<Part>();
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
	}
}
