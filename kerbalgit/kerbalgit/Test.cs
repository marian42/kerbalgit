using System;

namespace kerbalgit {
	class Test {
		static void Main(string[] args) {
			var parser = new Parser(@"C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\saves\1_0-career\persistent.sfs");
			var tree = parser.Parse();
			Console.WriteLine("Parsing complete.");

			var count = 0;
			tree.ForEach(_ => count++);
			Console.WriteLine("Found " + count + " Nodes.");

			Console.ReadLine();
		}
	}
}
