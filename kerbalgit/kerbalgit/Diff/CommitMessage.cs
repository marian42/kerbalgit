using System;
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

		public static string AddMetricPrefix(double value, string format = "F0") {
			char[] incPrefixes = new[] { 'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y' };
			char[] decPrefixes = new[] { 'm', '\u03bc', 'n', 'p', 'f', 'a', 'z', 'y' };

			int degree = (int)Math.Floor(Math.Log10(Math.Abs(value)) / 3);
			double scaled = value * Math.Pow(1000, -degree);

			char? prefix = null;
			switch (Math.Sign(degree))
			{
				case 1:  prefix = incPrefixes[degree - 1]; break;
				case -1: prefix = decPrefixes[-degree - 1]; break;
			}

			return scaled.ToString(format) + prefix;
		}
	}
}
