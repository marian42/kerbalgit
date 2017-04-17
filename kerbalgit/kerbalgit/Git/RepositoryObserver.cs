using System;
using System.IO;
using System.Timers;

namespace kerbalgit.Git {
	class RepositoryObserver {
		private const string FILENAME = "persistent.sfs";
		
		public readonly KerbalRepository Repository;

		private Timer timer;

		private DateTime lastChanged;

		public RepositoryObserver(string folder) {
			Repository = new KerbalRepository(folder);
		}

		private bool fileHasChanged {
			get {
				var fileInfo = new FileInfo(Repository.Folder + FILENAME);
				return fileInfo.LastWriteTime != lastChanged;
			}
		}

		private void setLastChanged() {
			var fileInfo = new FileInfo(Repository.Folder + FILENAME);
			lastChanged = fileInfo.LastWriteTime;
		}		

		private void check() {
			Console.Write(".");

			if (!fileHasChanged) {
				return;
			}
			setLastChanged();

			Console.WriteLine("Reading " + Repository.Name + "...");
			var diff = Repository.CreateDiff();
			if (!diff.AnyChanges) {
				return;
			}

			Repository.Commit(diff);
		}

		private void check(Object source, ElapsedEventArgs e) {
			check();
		}

		public void StartObserving() {
			Console.WriteLine("Observing " + Repository.Name + "...");
			check();

			if (timer == null) {
				timer = new System.Timers.Timer(10000);
			}
			timer.Elapsed += check;
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		public void RewriteAllCommits() {
			var rewriter = new CommitMessageRewriter(Repository);
			rewriter.Run();
		}
	}
}
