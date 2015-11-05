using kerbalgit.Tree;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Timers;

namespace kerbalgit.Git {
	class SaveGameObserver {
		private const string FILENAME = "persistent.sfs";

		private readonly string folder;
		private readonly Repository repository;
		private DateTime lastChanged;

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
			
			return new Diff.Diff(parseSavegame(commitContent), parseSavegame(workingContent));
		}

		private bool FileHasChanged {
			get {
				var fileInfo = new FileInfo(folder + FILENAME);
				return fileInfo.LastWriteTime != lastChanged;
			}
		}

		private void setLastChanged() {
			var fileInfo = new FileInfo(folder + FILENAME);
			lastChanged = fileInfo.LastWriteTime;
		}

		public void Commit(string message) {
			Console.WriteLine("Committing " + Name + ".");

			repository.Stage(FILENAME);

			Signature author = new Signature("Name", "email@example.com", DateTime.Now);

			Commit commit = repository.Commit(message, author, author);
			setLastChanged();
		}

		public void Check() {
			if (!FileHasChanged) {
				return;
			}
			setLastChanged();
			
			Console.WriteLine("Reading " + Name + "...");
			var diff = createDiff();
			if (!diff.AnyChanges) {
				return;
			}

			Commit(diff.Message);
		}

		public void Check(Object source, ElapsedEventArgs e) {
			Check();
			Console.Write(".");
		}

		public string CommitMessage {
			get {
				return createDiff().Message;
			}
		}

		public string Name {
			get {
				return folder.Split(Path.DirectorySeparatorChar).Last(s => s.Any());
			}
		}
	}
}
