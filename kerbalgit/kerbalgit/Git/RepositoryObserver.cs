using System;
using System.IO;
using System.Timers;

namespace kerbalgit.Git {
	class RepositoryObserver {
		private const string FILENAME = "persistent.sfs";
		
		private readonly KerbalRepository repository;

		private Timer timer;

		private DateTime lastChanged;

		public RepositoryObserver(string folder) {
			repository = new KerbalRepository(folder);
		}

		private bool fileHasChanged {
			get {
				var fileInfo = new FileInfo(repository.Folder + FILENAME);
				return fileInfo.LastWriteTime != lastChanged;
			}
		}

		private void setLastChanged() {
			var fileInfo = new FileInfo(repository.Folder + FILENAME);
			lastChanged = fileInfo.LastWriteTime;
		}		

		private void check() {
			Console.Write(".");

			if (!fileHasChanged) {
				return;
			}
			setLastChanged();

			Console.WriteLine("Reading " + repository.Name + "...");
			var diff = repository.CreateDiff();
			if (!diff.AnyChanges) {
				return;
			}

			repository.Commit(diff);
		}

		private void check(Object source, ElapsedEventArgs e) {
			check();
		}

		public void StartObserving() {
			Console.WriteLine("Observing " + repository.Name + "...");
			check();

			if (timer == null) {
				timer = new System.Timers.Timer(10000);
			}
			timer.Elapsed += check;
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		public void RewriteAllCommits() {
			var rewriter = new CommitMessageRewriter(repository);
			rewriter.Run();
		}
	}
}
