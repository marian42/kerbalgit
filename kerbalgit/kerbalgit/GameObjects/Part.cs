using kerbalgit.Tree;
using System;

namespace kerbalgit.GameObjects {
	public class Part {
		public readonly Node node;
		public readonly Vessel vessel;

		public Part(Node node, Vessel vessel) {
			if (node.Name != "part") {
				throw new InvalidOperationException("Node is not a part.");
			}

			this.node = node;
			this.vessel = vessel;
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
	}
}
