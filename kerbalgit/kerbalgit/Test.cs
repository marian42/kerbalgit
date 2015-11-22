using kerbalgit.Git;
using System;
using System.Linq;

namespace kerbalgit {
	class Test {
		private static void createMessage(KerbalRepository observer, string commitHash) {
			var diff = observer.CreateDiff(commitHash);
			Console.WriteLine(diff.Message);
		}

		static void Main(string[] args) {
			var observer = new RepositoryObserver(@"C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\saves\1_0-career\");

			//observer.RewriteAllCommits();

			observer.StartObserving();

			//createMessage(observer.Repository, "19108a3");		

			Console.ReadLine();
		}
	}
}
