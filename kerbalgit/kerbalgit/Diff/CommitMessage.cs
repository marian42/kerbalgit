using System.Collections.Generic;
using System.Linq;

namespace kerbalgit {
	public class CommitMessage {
		public readonly string Message;
		public readonly int Priority;

		public CommitMessage(string message, int priority) {
			Message = message;
			Priority = priority;
		}

		public static string Enumerate(IEnumerable<string> items) {
			if (items.Count() == 1) {
				return items.First();
			}

			return string.Join(", ", items.Take(items.Count() - 1)) + " and " + items.Last();
		}
	}
}
