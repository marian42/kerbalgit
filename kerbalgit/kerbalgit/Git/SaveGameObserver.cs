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

		private Timer timer;

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

			string workingContent;
			using (var content = new StreamReader(repository.Info.WorkingDirectory + Path.DirectorySeparatorChar + FILENAME, Encoding.UTF8)) {
				workingContent = content.ReadToEnd();
			}
			
			return new Diff.Diff(parseSavegame(commitContent), parseSavegame(workingContent));
		}

		public Diff.Diff createDiff(string commitHash) {
			var commit = repository.Lookup<Commit>(commitHash);

			string newSavefile;
			using (var content = new StreamReader((commit[FILENAME].Target as Blob).GetContentStream(), Encoding.UTF8)) {
				newSavefile = content.ReadToEnd();
			}

			string oldSavefile;
			using (var content = new StreamReader((commit.Parents.First()[FILENAME].Target as Blob).GetContentStream(), Encoding.UTF8)) {
				oldSavefile = content.ReadToEnd();
			}

			return new Diff.Diff(parseSavegame(oldSavefile), parseSavegame(newSavefile));
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
			Console.Write(".");

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

		public void StartObserving() {
			Console.WriteLine("Observing " + Name + "...");
			Check();

			if (timer == null) {
				timer = new System.Timers.Timer(10000);
			}
			timer.Elapsed += Check;
			timer.AutoReset = true;
			timer.Enabled = true;
		}
	}
}
