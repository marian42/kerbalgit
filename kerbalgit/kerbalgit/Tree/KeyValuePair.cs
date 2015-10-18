
namespace kerbalgit.Tree {
	public class KeyValuePair : AbstractNode {
		public readonly string Value;

		public KeyValuePair(string name, string value, Node parent) : base(name, parent) {
			this.Value = value;
		}
	}
}
