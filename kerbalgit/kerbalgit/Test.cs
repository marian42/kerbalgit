using kerbalgit.Git;
using System;
using System.Linq;

namespace kerbalgit {
	class Test {
		static void Main(string[] args) {
			var observer = new SaveGameObserver(@"C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\saves\1_0-career\");
			Console.WriteLine("Observing " + observer.Name + "...");
			observer.Check();

			var timer = new System.Timers.Timer(10000);
			timer.Elapsed += observer.Check;
			timer.AutoReset = true;
			timer.Enabled = true;

			Console.ReadLine();
		}
	}
}
