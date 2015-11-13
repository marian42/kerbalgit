using kerbalgit.Tree;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace kerbalgit.GameObjects {
	public class TechTreeNode {
		private readonly Node node;

		public TechTreeNode(Node node) {
			if (node.Name != "tech") {
				throw new InvalidOperationException("Node is not a Tech node.");
			}

			this.node = node;
		}

		public string Name {
			get {
				var id = Id;
				var splitMarks = new List<int>();

				for (var i = 1; i < id.Length; i++) {
					var c = id[i];
					if ((char.IsUpper(c) || char.IsNumber(c)) && char.IsLower(id[i - 1])) {
						splitMarks.Add(i);
					}
				}

				splitMarks.Add(id.Length);

				var words = new List<string>();
				for (var i = 0; i < splitMarks.Count; i++) {
					words.Add(id.Substring(i == 0 ? 0 : splitMarks[i - 1], splitMarks[i] - (i == 0 ? 0 : splitMarks[i - 1])));
				}
				TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

				return string.Join(" ", words.Select(word => word == "adv" ? "Advanced" : textInfo.ToTitleCase(word)));
			}
		}

		public string Id {
			get {
				return node.GetValue("id");
			}
		}

		public override bool Equals(object obj) {
			if (!(obj is TechTreeNode)) {
				return false;
			}
			return (obj as TechTreeNode).Id == Id;
		}

		public override int GetHashCode() {
			return Id.GetHashCode();
		}
	}
}
