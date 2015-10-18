
namespace kerbalgit.Tree {
	public class RootNode : Node {
		public RootNode(string name) : base(name, null) { }

		public override string Path {
			get { return string.Empty; }
		}

		public override int Depth {
			get {
				return 0;
			}
		}
	}
}
