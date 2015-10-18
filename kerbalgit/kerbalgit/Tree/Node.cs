using System.Collections.Generic;

namespace kerbalgit.Tree {
	public class Node : AbstractNode {
		public Node(string name, Node parent)	: base(name, parent) {
			Children = new List<AbstractNode>();
		}
		
		public ICollection<AbstractNode> Children { get; private set; }

		public void AddChild(AbstractNode node) {
			Children.Add(node);
		}

		public override void ForEach(System.Action<AbstractNode> method) {
			base.ForEach(method);

			foreach (var child in Children) {
				child.ForEach(method);
			}
		}
	}
}
