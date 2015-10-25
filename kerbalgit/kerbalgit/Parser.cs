using kerbalgit.Tree;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace kerbalgit {
	class Parser {
		private readonly IEnumerable<string> lines;
		
		public RootNode Tree { get; private set; }

		private Node currentNode;

		public Parser(string filename) {
			lines = File.ReadAllLines(filename).ToList();			
		}

		public Parser(IEnumerable<string> lines) {
			this.lines = lines;
		}

		public RootNode Parse() {
			if (Tree != null) {
				return Tree;
			}

			var enumerator = (IEnumerator<string>)(lines.GetEnumerator());
			enumerator.MoveNext();

			Tree = new RootNode(enumerator.Current);
			currentNode = Tree;
			enumerator.MoveNext();
			enumerator.MoveNext();

			while (currentNode != null) {
				parseLine(enumerator.Current);
				enumerator.MoveNext();
			}

			return Tree;
		}

		private void parseLine(string line) {
			line = line.Trim();
			if (line.Equals("}")) {
				currentNode = currentNode.Parent;
			} else if (line.Contains("=")) {
				var parts = line.Split('=');
				var kvp = new KeyValuePair(parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : string.Empty, currentNode);
			} else if (line.Any() && line != "{") {
				currentNode = new Node(line.ToLower(), currentNode);
			}
		}
	}
}