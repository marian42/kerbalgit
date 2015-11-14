using kerbalgit.Tree;
using System;

namespace kerbalgit.GameObjects {
	public class ScienceData {
		private readonly Node node;

		public ScienceData(Node node) {
			if (node.Name != "sciencedata") {
				throw new InvalidOperationException("Node is not a ScienceData node.");
			}

			this.node = node;
		}

		public string Id {
			get {
				return node.GetValue("subjectID");
			}
		}

		public string Title {
			get {
				return node.GetValue("title");
			}
		}

		public override string ToString() {
			return Title;
		}

		public override int GetHashCode() {
			return Id.GetHashCode();
		}

		public override bool Equals(object obj) {
			return obj is ScienceData && (obj as ScienceData).Id == Id;
		}

		public int Data {
			get {
				return (int)node.GetInt("data");
			}
		}
	}
}
