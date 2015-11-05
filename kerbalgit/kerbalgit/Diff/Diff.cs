using kerbalgit.GameObjects;
using kerbalgit.Tree;
using System;
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

			createDiff();
		}

		private void createDiff() {
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
			analyzeDocking();
			trackLostAndFoundShips();
			compareVessels();
		}

		private void analyzeUnDocking() {
			foreach (var vesselInfo in oldVessels.Values.Where(vesselInfo => vesselInfo.CorrespondingVessels.Count() > 1)) {
				var motherShip = vesselInfo.CorrespondingVessels.OrderBy(vessel => vessel.Name.Length).First();
				var undockedShips = vesselInfo.CorrespondingVessels.Where(vessel => vessel != motherShip).Where(vessel => !vessel.IsDebris);
				var jettisonedShips = vesselInfo.CorrespondingVessels.Where(vessel => vessel != motherShip).Where(vessel => vessel.IsDebris);

				if (undockedShips.Any()) {
					addMessage("Undocked " + CommitMessage.Enumerate(undockedShips.Select(vessel => vessel.Name)) + " from " + motherShip.Name, 0);
				}
				if (jettisonedShips.Any()) {
					addMessage("Jettisoned " + jettisonedShips.Count() + (undockedShips.Count() == 1 ? " chunk " : " chunks ") + "from " + motherShip.Name, 0);
				}
			}
		}

		private void analyzeDocking() {
			foreach (var vesselInfo in newVessels.Values.Where(vesselInfo => vesselInfo.CorrespondingVessels.Count() > 1)) {
				var motherShip = vesselInfo.CorrespondingVessels.OrderBy(vessel => vessel.Name.Length).First();
				var dockedShips = vesselInfo.CorrespondingVessels.Where(vessel => vessel != motherShip);

				addMessage("Docked " + CommitMessage.Enumerate(dockedShips.Select(vessel => vessel.Name)) + " to " + motherShip.Name, 0);
			}
		}

		private void trackLostAndFoundShips() {
			foreach (var vesselInfo in oldVessels.Values.Where(vesselInfo => vesselInfo.CorrespondingVessels.Count() == 0)) {
				if (vesselInfo.Vessel.CelestialBody == Planetarium.Instance.Value.Kerbin && vesselInfo.Vessel.FlightStateValue != Vessel.FlighState.Orbiting) {
					addMessage("Recovered " + vesselInfo.Vessel.Name + ".", 2);
				} else {
					addMessage("Lost track of " + vesselInfo.Vessel.Name + " (probably recovered).", 2);
				}
			}

			foreach (var vesselInfo in newVessels.Values.Where(vesselInfo => vesselInfo.CorrespondingVessels.Count() == 0)) {
				switch (vesselInfo.Vessel.FlightStateValue) {
					case Vessel.FlighState.Prelaunch:
						addMessage("Put " + vesselInfo.Vessel.Name + " on the " + vesselInfo.Vessel.Location.ToLower() + ".", 0); break;
					case Vessel.FlighState.Landed:
						addMessage("Launched " + vesselInfo.Vessel.Name + " and landed on " + vesselInfo.Vessel.CelestialBody.Name + ".", 0); break;
					case Vessel.FlighState.Splashed:
						addMessage("Launched " + vesselInfo.Vessel.Name + " and splashed down on " + vesselInfo.Vessel.CelestialBody.Name + ".", 0); break;
					case Vessel.FlighState.Orbiting:
						addMessage("Launched " + vesselInfo.Vessel.Name + " into " + vesselInfo.Vessel.Orbit.GetName(true, true), 0); break;
					default: throw new NotImplementedException(vesselInfo.Vessel.FlightStateValue.ToString());
				}
			}
		}

		private void compareVessels() {
			HashSet<Vessel> visitedOldVessels = new HashSet<Vessel>();

			foreach (var vesselInfo in newVessels.Values.Where(vesselInfo => vesselInfo.CorrespondingVessels.Count() >= 1)) {
				var oldVessel = vesselInfo.CorrespondingVessels.OrderBy(vessel => vessel.Name.Length).First();
				var newVessel = vesselInfo.Vessel;

				if (!visitedOldVessels.Contains(oldVessel)) {
					compareVessels(oldVessel, vesselInfo.Vessel);
					visitedOldVessels.Add(oldVessel);
				}
			}
		}

		private void compareVessels(Vessel oldVessel, Vessel newVessel) {
			compareLocations(oldVessel, newVessel);
			compareStaging(oldVessel, newVessel);			
		}

		private void compareLocations(Vessel oldVessel, Vessel newVessel) {
			if (oldVessel.FlightStateValue == Vessel.FlighState.Prelaunch && newVessel.InFlight) {
				addMessage("Launched " + oldVessel.Name + " into " + newVessel.Orbit.GetName(true, true) + ".", 0);
			}
			if (oldVessel.FlightStateValue == Vessel.FlighState.Prelaunch && (newVessel.FlightStateValue == Vessel.FlighState.Landed && newVessel.FlightStateValue == Vessel.FlighState.Splashed)) {
				if (newVessel.CelestialBody == Planetarium.Instance.Value.Kerbin) {
					addMessage("Launched " + oldVessel.Name + " on Kerbin.", 0);
				} else {
					addMessage("Launched " + oldVessel.Name + " and " + newVessel.FlightStateValue.ToString().ToLower() + " on " + newVessel.CelestialBody.Name + ".", 0);
				}
			}
			if (oldVessel.InFlight && newVessel.FlightStateValue == Vessel.FlighState.Landed) {
				if (oldVessel.CelestialBody == newVessel.CelestialBody) {
					addMessage("Landed " + oldVessel.Name + " on " + newVessel.CelestialBody.Name + ".", 0);
				} else {
					addMessage(oldVessel.Name + " travelled from " + oldVessel.Orbit.GetName(true, true) + " to " + newVessel.CelestialBody.Name + " and landed there.", 0);
				}

			}
			if (oldVessel.InFlight && newVessel.FlightStateValue == Vessel.FlighState.Splashed) {
				if (oldVessel.CelestialBody == newVessel.CelestialBody) {
					addMessage(oldVessel.Name + " splashed down on " + newVessel.CelestialBody.Name + ".", 0);
				} else {
					addMessage(oldVessel.Name + " travelled from " + oldVessel.Orbit.GetName(true, true) + " to " + newVessel.CelestialBody.Name + " and splashed down.", 0);
				}
			}
			if ((oldVessel.FlightStateValue == Vessel.FlighState.Landed || oldVessel.FlightStateValue == Vessel.FlighState.Splashed) && newVessel.FlightStateValue == Vessel.FlighState.Orbiting) {
				addMessage(oldVessel.Name + " took off from " + oldVessel.CelestialBody + " and went into " + newVessel.Orbit.GetName(false, true), 0);
			}

			if (oldVessel.InFlight && newVessel.InFlight && !oldVessel.Orbit.IsSimilar(newVessel.Orbit, true)) {
				addMessage(oldVessel.Name + " changed from " + oldVessel.Orbit.GetName(true, true) + " to " + newVessel.Orbit.GetName(oldVessel.CelestialBody != newVessel.CelestialBody, true) + ".", 0);
			}
		}

		private void compareStaging(Vessel oldVessel, Vessel newVessel) {
			if (oldVessel.Stage == newVessel.Stage + 1) {
				addMessage(oldVessel.Name + " triggered stage " + oldVessel.Stage + ".", 3);
			} else if (oldVessel.Stage == newVessel.Stage + 2) {
				addMessage(oldVessel.Name + " triggered stages " + oldVessel.Stage + " and " + (oldVessel.Stage + 1) + ".", 3);
			} else if (oldVessel.Stage > newVessel.Stage) {
				addMessage(oldVessel.Name + " triggered stages " + oldVessel.Stage + " to " + (newVessel.Stage + 1) + ".", 3);
			} else if (oldVessels[oldVessel.Id].LostOrGainedParts) {
				addMessage(oldVessel.Name + " lost parts.", 4);
			}
		}

		public string Message {
			get {
				return string.Join("\n", commitMessages.OrderBy(message => message.Priority).Select(commitMessage => commitMessage.Message));
			}
		}

		public bool AnyChanges {
			get {
				return commitMessages.Any();
			}
		}

		private void addMessage(string message, int priority) {
			commitMessages.Add(new CommitMessage(message, priority));
		}

		public IReadOnlyCollection<Vessel> GetNewVessels() {
			return newVessels.Values.Select(info => info.Vessel).ToList().AsReadOnly();
		}		
	}
}
