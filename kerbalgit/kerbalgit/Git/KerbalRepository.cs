using LibGit2Sharp;
using System;
using System.IO;
using System.Text;
using System.Linq;
using kerbalgit.GameObjects;

namespace kerbalgit.Git {
	class KerbalRepository {
		private const string FILENAME = "persistent.sfs";
		private const string BRANCH_NAME_PREFIX = "reverted-mission-";

		public readonly string Folder;
		public readonly Repository Repository;
		
		public KerbalRepository(string folder) {
			this.Folder = folder;
			this.Repository = new Repository(folder);
		}

		private static Savegame getSavegame(Commit commit) {
			var blob = commit[FILENAME].Target as Blob;

			string commitContent;
			using (var content = new StreamReader(blob.GetContentStream(), Encoding.UTF8)) {
				commitContent = content.ReadToEnd();
			}

			return Parser.ParseSavegame(commitContent);
		}

		private Savegame getWorkingContentSavegame() {
			string workingContent;

			StreamReader contentStream = null;
			bool openedSuccessfuly = true;

			do {
				try {
					contentStream = new StreamReader(Repository.Info.WorkingDirectory + Path.DirectorySeparatorChar + FILENAME, Encoding.UTF8);
				}
				catch (IOException) {
					openedSuccessfuly = false;
					System.Threading.Thread.Sleep(1000);
				}
			} while (!openedSuccessfuly);

			workingContent = contentStream.ReadToEnd();
			contentStream.Close();

			return Parser.ParseSavegame(workingContent);
		}

		public Diff.Diff CreateDiff() {
			var newSavegame = getWorkingContentSavegame();
			var parentCommit = findLatestCommitBefore(newSavegame.Time);
			var oldSavegame = getSavegame(parentCommit);
			
			return new Diff.Diff(oldSavegame, newSavegame, parentCommit);
		}

		public Diff.Diff CreateDiff(string commitHash) {
			var commit = Repository.Lookup<Commit>(commitHash);
			return CreateDiff(commit);
		}

		public Diff.Diff CreateDiff(Commit commit) {
			string newSavefile;
			using (var content = new StreamReader((commit[FILENAME].Target as Blob).GetContentStream(), Encoding.UTF8)) {
				newSavefile = content.ReadToEnd();
			}

			string oldSavefile;
			using (var content = new StreamReader((commit.Parents.First()[FILENAME].Target as Blob).GetContentStream(), Encoding.UTF8)) {
				oldSavefile = content.ReadToEnd();
			}

			return new Diff.Diff(Parser.ParseSavegame(oldSavefile), Parser.ParseSavegame(newSavefile));
		}

		private Commit findLatestCommitBefore(double currentTime) {
			var commit = Repository.Head.Tip;
			var commitTime = getSavegame(commit).Time;

			while (commitTime >= currentTime) {
				commit = commit.Parents.First();
				commitTime = getSavegame(commit).Time;
			}

			return commit;
		}

		/// <summary>
		/// Creates a new branch for the current head (reverted mission)
		/// and sets the previous branch (main mission) to the supplied commit
		/// </summary>
		private void safelyCheckoutCommit(Commit commit) {
			if (commit == Repository.Head.Tip) {
				throw new InvalidOperationException("HEAD already points at this commit.");
			}

			var mainBranch = Repository.Head;

			var branchNameIndex = 1;
			while (Repository.Branches.Any(branch => branch.Name == BRANCH_NAME_PREFIX + branchNameIndex)) {
				branchNameIndex++;
			}

			Repository.CreateBranch(BRANCH_NAME_PREFIX + branchNameIndex);
			Repository.Refs.UpdateTarget(mainBranch.CanonicalName, commit.Sha);
		}

		public void Commit(Diff.Diff diff) {
			if (diff.Commit != null && diff.Commit != Repository.Head.Tip) {
				safelyCheckoutCommit(diff.Commit);
			}

			Console.WriteLine("Committing " + Name + ".");

			Repository.Stage(FILENAME);

			Signature author = new Signature("Name", "email@example.com", DateTime.Now);

			Commit commit = Repository.Commit(diff.Message, author, author);			
		}

		public string Name {
			get {
				return Folder.Split(Path.DirectorySeparatorChar).Last(s => s.Any());
			}
		}
	}
}
