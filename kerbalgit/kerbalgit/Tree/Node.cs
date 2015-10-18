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
	}
}
