using kerbalgit.Git;
using System;
using System.Linq;

namespace kerbalgit {
	class Test {
		private static void observeSavegame(SaveGameObserver observer) {
			Console.WriteLine("Observing " + observer.Name + "...");
			observer.Check();

			var timer = new System.Timers.Timer(10000);
			timer.Elapsed += observer.Check;
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		private static void createMessage(SaveGameObserver observer, string commitHash) {
			var diff = observer.createDiff(commitHash);
			Console.WriteLine(diff.Message);
		}

		static void Main(string[] args) {
			var observer = new SaveGameObserver(@"C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\saves\fueltest\");
			
			//observeSavegame(observer);

			createMessage(observer, "66ee600");		

			Console.ReadLine();
		}
	}
}
