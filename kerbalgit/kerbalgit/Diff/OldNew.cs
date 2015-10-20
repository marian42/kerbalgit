
namespace kerbalgit.Diff {
	class OldNew<T> {
		public T Old;
		public T New;

		public void set(T value, bool old) {
			if (old) {
				Old = value;
			} else {
				New = value;
			}
		}
	}
}
