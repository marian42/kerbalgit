using kerbalgit.Git;
using System;
using System.Linq;

namespace kerbalgit {
	class Test {
		static void Main(string[] args) {
			var observer = new SaveGameObserver(@"C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\saves\test\");
			Console.WriteLine(observer.CommitMessage);
			observer.Check();
			
			Console.ReadLine();
		}
	}
}
