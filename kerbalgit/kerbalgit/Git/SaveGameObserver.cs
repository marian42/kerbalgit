using kerbalgit.Tree;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace kerbalgit.Git {
	class SaveGameObserver {
		private const string FILENAME = "persistent.sfs";

		private readonly string folder;
		private readonly Repository repository;

		public SaveGameObserver(string folder) {
			this.folder = folder;
			this.repository = new Repository(folder);
		}

		private RootNode parseSavegame(string content) {
			var lines = content.Split('\n');
			var parser = new Parser(lines);
			var rootNode = parser.Parse();
			return rootNode;
		}

		private Diff.Diff createDiff() {
			var blob = repository.Head.Tip[FILENAME].Target as Blob;
			string commitContent;
			using (var content = new StreamReader(blob.GetContentStream(), Encoding.UTF8)) {
				commitContent = content.ReadToEnd();
			}

			var patch = repository.Diff.Compare<Patch>(new List<string>() { FILENAME });
			string workingContent;
			using (var content = new StreamReader(repository.Info.WorkingDirectory + Path.DirectorySeparatorChar + FILENAME, Encoding.UTF8)) {
				workingContent = content.ReadToEnd();
			}
					
			var diff = new Diff.Diff(parseSavegame(commitContent), parseSavegame(workingContent));
			diff.CreateDiff();

			return diff;
		}

		public string CommitMessage {
			get {
				return createDiff().Message;
			}
		}
	}
}
