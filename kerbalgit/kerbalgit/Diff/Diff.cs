using kerbalgit.GameObjects;
using kerbalgit.Tree;
using System.Collections.Generic;
using System.Linq;

namespace kerbalgit.Diff {
	public class Diff {
		private readonly RootNode oldSave;
		private readonly RootNode newSave;
		private Dictionary<long, OldNew<Part>> parts;

		private Dictionary<string, VesselInfo> oldVessels;
		private Dictionary<string, VesselInfo> newVessels;

		private List<CommitMessage> commitMessages;

		public Diff(RootNode oldSave, RootNode newSave) {
			this.oldSave = oldSave;
			this.newSave = newSave;
		}

		public void CreateDiff() {
			parts = new Dictionary<long, OldNew<Part>>();
			oldVessels = new Dictionary<string, VesselInfo>();
			newVessels = new Dictionary<string, VesselInfo>();
						
			findVessels(true);
			findVessels(false);
			matchParts();
			createCommitMessages();
		}

		private RootNode getSave(bool old) {
			return old ? oldSave : newSave;
		}

		private void findVessels(bool old) {
			var save = getSave(old);

			var flightstateNode = save.Get("flightstate") as Node;

			foreach (var node in flightstateNode.Children.OfType<Node>().Where(node => node.Name == "vessel")) {
				addVessel(new Vessel(node as Node), old);
			}
		}

		private void addVessel(Vessel vessel, bool old) {
			var vesselDict = old ? oldVessels : newVessels;

			vesselDict[vessel.Id] = new VesselInfo(vessel);

			foreach (var node in vessel.Node.Children.OfType<Node>().Where(node => node.Name == "part")) {
				addPart(new Part(node, vessel), old);
			}
		}

		private void addPart(Part part, bool old) {
			if (!parts.ContainsKey(part.Id)) {
				parts[part.Id] = new OldNew<Part>();
			}
			parts[part.Id].set(part, old);
		}

		private void matchParts() {
			foreach (var oldNewPart in parts.Values) {
				if (oldNewPart.Old == null && oldNewPart.New != null) {
					var newVessel = oldNewPart.New.vessel;
					newVessels[newVessel.Id].LostOrGainedParts = true;
				} else if (oldNewPart.Old != null && oldNewPart.New == null) {
					var oldVessel = oldNewPart.Old.vessel;
					oldVessels[oldVessel.Id].LostOrGainedParts = true;
				} else {
					var newVessel = oldNewPart.New.vessel;
					var oldVessel = oldNewPart.Old.vessel;
					newVessels[newVessel.Id].AddCorrespondingVessel(oldVessel);
					oldVessels[oldVessel.Id].AddCorrespondingVessel(newVessel);
				}
			}
		}

		private void createCommitMessages() {
			commitMessages = new List<CommitMessage>();

			analyzeUnDocking();
		}

		private void analyzeUnDocking() {
			foreach (var vesselInfo in oldVessels.Values.Where(vesselInfo => vesselInfo.CorrespondingVessels.Count() > 1)) {
				var motherShip = vesselInfo.CorrespondingVessels.OrderBy(vessel => vessel.Name.Length).First();
				var undockedShips = vesselInfo.CorrespondingVessels.Where(vessel => vessel != motherShip);

				addMessage("Undocked " + CommitMessage.Enumerate(undockedShips.Select(vessel => vessel.Name)) + " from " + motherShip.Name, 0);
			}

			foreach (var vesselInfo in oldVessels.Values.Where(vesselInfo => vesselInfo.LostOrGainedParts)) {
				addMessage(vesselInfo.Vessel.Name + " lost parts!", 2);
			}
		}

		public string Message {
			get {
				return string.Join("\n", commitMessages.OrderBy(message => message.Priority).Select(commitMessage => commitMessage.Message));
			}
		}

		private void addMessage(string message, int priority) {
			commitMessages.Add(new CommitMessage(message, priority));
		}
	}
}
