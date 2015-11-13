﻿using kerbalgit.Tree;
using LibGit2Sharp;
using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Linq;
using System.Collections.Generic;

namespace kerbalgit.Git {
	class SaveGameObserver {
		private const string FILENAME = "persistent.sfs";
		private const string BRANCH_NAME_PREFIX = "reverted-mission-";

		private readonly string folder;
		private readonly Repository repository;
		private DateTime lastChanged;

		private Timer timer;

		public SaveGameObserver(string folder) {
			this.folder = folder;
			this.repository = new Repository(folder);
		}

		private static double getTime(RootNode saveGame) {
			return saveGame.GetDouble("flightstate/UT");
		}

		private static RootNode parseSavegame(string content) {
			var lines = content.Split('\n');
			var parser = new Parser(lines);
			var rootNode = parser.Parse();
			return rootNode;
		}

		private static RootNode getSavegame(Commit commit) {
			var blob = commit[FILENAME].Target as Blob;

			string commitContent;
			using (var content = new StreamReader(blob.GetContentStream(), Encoding.UTF8)) {
				commitContent = content.ReadToEnd();
			}

			return parseSavegame(commitContent);
		}

		private Diff.Diff createDiff() {
			string workingContent;
			using (var content = new StreamReader(repository.Info.WorkingDirectory + Path.DirectorySeparatorChar + FILENAME, Encoding.UTF8)) {
				workingContent = content.ReadToEnd();
			}

			var newSavegame = parseSavegame(workingContent);
			var parentCommit = findLatestCommitBefore(getTime(newSavegame));
			var oldSavegame = getSavegame(parentCommit);
			
			return new Diff.Diff(oldSavegame, newSavegame, parentCommit);
		}

		public Diff.Diff createDiff(string commitHash) {
			var commit = repository.Lookup<Commit>(commitHash);
			return createDiff(commit);
		}

		public Diff.Diff createDiff(Commit commit) {
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

		private Commit findLatestCommitBefore(double currentTime) {
			var commit = repository.Head.Tip;
			var commitTime = getTime(getSavegame(commit));

			while (commitTime > currentTime) {
				commit = commit.Parents.First();
				commitTime = getTime(getSavegame(commit));
			}

			return commit;
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

		/// <summary>
		/// Creates a new branch for the current head (reverted mission)
		/// and sets the previous branch (main mission) to the supplied commit
		/// </summary>
		private void safelyCheckoutCommit(Commit commit) {
			if (commit == repository.Head.Tip) {
				throw new InvalidOperationException("HEAD already points at this commit.");
			}

			var mainBranch = repository.Head;

			var branchNameIndex = 1;
			while (repository.Branches.Any(branch => branch.Name == BRANCH_NAME_PREFIX + branchNameIndex)) {
				branchNameIndex++;
			}

			repository.CreateBranch(BRANCH_NAME_PREFIX + branchNameIndex);
			repository.Refs.UpdateTarget(repository.Refs.Head, commit.Id);
			repository.Refs.UpdateTarget(mainBranch.CanonicalName, commit.Sha);
		}

		public void Commit(Diff.Diff diff) {
			if (diff.Commit != null && diff.Commit != repository.Head.Tip) {
				safelyCheckoutCommit(diff.Commit);
			}

			Console.WriteLine("Committing " + Name + ".");

			repository.Stage(FILENAME);

			Signature author = new Signature("Name", "email@example.com", DateTime.Now);

			Commit commit = repository.Commit(diff.Message, author, author);
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

			Commit(diff);
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

		private IReadOnlyCollection<Commit> getAllCommits() {
			var result = repository.Commits.ToList();

			foreach (var branch in repository.Branches) {
				var commit = branch.Tip;
				while (!result.Contains(commit) && commit.Parents.Any()) {
					result.Add(commit);
					commit = commit.Parents.First();
				}

				if (!result.Contains(commit)) {
					result.Add(commit);
				}
			}

			return result.AsReadOnly();
		}

		public void RewriteAllCommits() {
			repository.Refs.RewriteHistory(new RewriteHistoryOptions {
				BackupRefsNamespace = "backup/" + Guid.NewGuid().GetHashCode(),
				CommitHeaderRewriter = c =>
					CommitRewriteInfo.From(c, message: c.Parents.Any() ? createDiff(c).Message : c.Message),
			}, getAllCommits());

			Console.WriteLine("Rewriting commit messages complete.");
		}
	}
}
