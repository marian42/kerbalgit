using System.Collections.Generic;
using System.Linq;

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

		public override AbstractNode Get(string address) {
			if (address.Equals(string.Empty)) {
				return this;
			}

			var firstStep = address.Split('/').First();
			var remainingAddress = firstStep.Length == address.Length ? "" : address.Substring(firstStep.Length + 1);

			return Children.First(node => node.Name == firstStep).Get(remainingAddress);
		}
	}
}
