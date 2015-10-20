using System;

namespace kerbalgit {
	class Test {
		static void Main(string[] args) {
			var parser = new Parser(@"C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\saves\test\landed.sfs");
			var oldSave = parser.Parse();
			parser = new Parser(@"C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\saves\test\orbiting.sfs");
			var newSave = parser.Parse();
			Console.WriteLine("Parsing complete.");

			var diff = new Diff.Diff(oldSave, newSave);
			diff.CreateDiff();

			Console.WriteLine(diff.Message);
			Console.WriteLine("Diff complete.");
			
			Console.ReadLine();
		}
	}
}
