
namespace kerbalgit.Tree {
	public abstract class AbstractNode {
		public AbstractNode(string name, Node parent) {
			this.Name = name;
			this.Parent = parent;

			if (!(this is RootNode)) {
				parent.AddChild(this);
			}
		}

		public readonly string Name;

		public readonly Node Parent;

		public virtual string Path {
			get {
				return Parent.Path + "/" + Name;
			}
		}
	}
}
