using kerbalgit.Git;
using System;
using System.Linq;

namespace kerbalgit {
	class Test {
		private static void createMessage(SaveGameObserver observer, string commitHash) {
			var diff = observer.createDiff(commitHash);
			Console.WriteLine(diff.Message);
		}

		static void Main(string[] args) {
			var observer = new SaveGameObserver(@"C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\saves\fueltest\");

			observer.StartObserving();

			//createMessage(observer, "ad214bc");		

			Console.ReadLine();
		}
	}
}
