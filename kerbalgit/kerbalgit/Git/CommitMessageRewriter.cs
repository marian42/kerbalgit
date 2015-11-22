using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kerbalgit.Git {
	class CommitMessageRewriter {
		private readonly KerbalRepository repository;

		private int totalCommits;
		private int completedCommits;

		public CommitMessageRewriter(KerbalRepository repository) {
			this.repository = repository;
		}

		private IReadOnlyCollection<Commit> getAllCommits() {
			var result = repository.Repository.Commits.ToList();

			foreach (var branch in repository.Repository.Branches) {
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

		private CommitRewriteInfo rewriteCommit(Commit commit) {
			onRewriteCommit();
			return CommitRewriteInfo.From(commit, message: commit.Parents.Any() ? repository.CreateDiff(commit).Message : commit.Message);
		}

		private void onRewriteCommit() {
			completedCommits++;
			Console.WriteLine("Rewriting commit messages... " + (100 * completedCommits / totalCommits) + "% complete.");
		}

		public void Run() {
			var commitsToUpdate = getAllCommits();
			totalCommits = commitsToUpdate.Count();
			completedCommits = 0;

			repository.Repository.Refs.RewriteHistory(new RewriteHistoryOptions {
				BackupRefsNamespace = "backup/" + Guid.NewGuid().GetHashCode(),
				CommitHeaderRewriter = rewriteCommit
			}, commitsToUpdate);

			Console.WriteLine("Rewriting commit messages complete.");
		}
	}
}
