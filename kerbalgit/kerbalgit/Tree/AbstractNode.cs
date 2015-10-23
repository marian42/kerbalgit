using System;
using System.Globalization;
using System.Linq;

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

		public virtual int Depth {
			get {
				return Parent.Depth + 1;
			}
		}

		public override string ToString() {
			return (new String(' ', Depth)) + Name; 
		}

		public virtual void ForEach(Action<AbstractNode> method) {
			method(this);
		}

		public virtual AbstractNode Get(string address) {
			if (address.Equals(string.Empty)) {
				return this;
			}

			throw new InvalidOperationException("Node has no children.");
		}

		public string GetValue(string address) {
			return (Get(address) as KeyValuePair).Value;
		}

		public long GetInt(string address) {
			return long.Parse(GetValue(address));
		}

		public double GetDouble(string address) {
			return double.Parse(GetValue(address), CultureInfo.InvariantCulture);
		}
	}
}