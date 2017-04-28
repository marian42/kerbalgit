using kerbalgit.Git;
using System;
using System.Linq;
using System.IO;

namespace kerbalgit {
	class ConsoleApp {
		private static void exitWithError(string message) {
			Console.WriteLine(message);
			Console.ReadLine();
			Environment.Exit(1);
		}

		private static string getSaveLocation() {
			try {
				string saveLocation =  File.ReadAllText("saveLocation.txt").Trim();
				if (!System.IO.Directory.Exists(saveLocation)) {
					ConsoleApp.exitWithError("The supplied folder doesn't exist: \n" + saveLocation);
					return "";
				}
				if (saveLocation.Last() != '\\' && saveLocation.Last() != '/') {
					saveLocation += "\\";
				}
				if (!File.Exists(saveLocation + "persistent.sfs")) {
					ConsoleApp.exitWithError("The file persistent.sfs was not found in the supplied folder. Make sure that the folder is a KSP save:\n" + saveLocation + "persistent.sfs");
					return "";
				}
				return saveLocation;
			}
			catch (FileNotFoundException) {
				ConsoleApp.exitWithError("File saveLocation.txt not found. Create a text file in this folder containing the folder of your KSP save.");
				return "";
			}
			
		}

		static void Main(string[] args) {
			if (args.Contains("-h") || args.Contains("--help")) {
				Console.WriteLine("Kerbalgit");
				return;
			}
			if (args.Contains("-r") || args.Contains("--rewrite")) {
				var repository = new KerbalRepository(ConsoleApp.getSaveLocation());
				var rewriter = new CommitMessageRewriter(repository);
				rewriter.Run();
				return;
			}

			var observer = new RepositoryObserver(ConsoleApp.getSaveLocation());

			Console.WriteLine("Observing " + observer.Repository.Name + "... Press any key to quit.");

			observer.StartObserving();

			Console.ReadLine();
		}
	}
}
