using kerbalgit.Git;
using System;
using System.Linq;

namespace kerbalgit {
	class Test {
		static void Main(string[] args) {
			var observer = new RepositoryObserver(@"C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\saves\1_0-career\");

			//observer.RewriteAllCommits();

			observer.StartObserving();

			Console.ReadLine();
		}
	}
}
