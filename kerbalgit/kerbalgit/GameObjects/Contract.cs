using kerbalgit.Tree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kerbalgit.GameObjects {
	public class Contract {
		public enum ContractType {
			WorldFirstContract,
			SurveyContract,
			RecoverAsset,
			CollectScience,
			ARMContract,
			BaseContract,
			ISRUContract,
			SatelliteContract,
			ExploreBody,
			TourismContract,
			PlantFlag,
			PartTest,
			StationContract,
			Unknown
		}

		public enum ContractState {
			Offered,
			Active,
			Completed
		}

		public readonly Node Node;

		public Contract(Node node) {
			if (node.Name != "contract" && node.Name != "contract_finished") {
				throw new InvalidOperationException("Node is not a contract.");
			}

			this.Node = node;
		}

		public string Id {
			get {
				return Node.GetValue("guid");
			}
		}

		public ContractType Type {
			get {
				try {
					return Enum.GetValues(typeof(Contract.ContractType)).Cast<Contract.ContractType>().First(type => type.ToString().ToLower() == Node.GetValue("type").ToLower());
				}
				catch (InvalidOperationException) {
					throw new NotImplementedException("Unknown contract type: " + Node.GetValue("type"));
				}
			}
		}

		public ContractState State {
			get {
				try {
					return Enum.GetValues(typeof(Contract.ContractState)).Cast<Contract.ContractState>().First(state => state.ToString().ToLower() == Node.GetValue("state").ToLower());
				}
				catch (InvalidOperationException) {
					throw new NotImplementedException("Unknown contract state: " + Node.GetValue("state"));
				}
			}
		}

		public CelestialBody Body {
			get {
				return Planetarium.ById((int)Node.GetInt("body"));
			}
		}

		public CelestialBody TargetBody {
			get {
				return Planetarium.ById((int)Node.GetInt("targetBody"));
			}
		}

		public string Name {
			get {
				switch (Type) {
					case ContractType.PlantFlag:
						return "plant flag on " + Body;
					case ContractType.RecoverAsset:
						var items = new List<string>();

						if (!string.IsNullOrEmpty(Node.GetValue("partName"))) {
							items.Add("part " + Node.GetValue("partName"));
						}
						if (!string.IsNullOrEmpty(Node.GetValue("kerbalName"))) {
							items.Add(Node.GetValue("kerbalName"));
						}

						return "recover " + CommitMessage.Enumerate(items) + " from " + TargetBody;
					case ContractType.CollectScience:
						return "collect science from " + Body;
					case ContractType.PartTest:
						return "test part " + Node.GetValue("part") + " on " + Planetarium.ById((int)Node.GetInt("dest"));
					case ContractType.StationContract:
						return "bring station to " + TargetBody;
					case ContractType.TourismContract:
						return "ferry " + (Node.GetValue("tourists").Count(c => c == '|') + 1) + " tourists";
					case ContractType.WorldFirstContract:
						var type = Node.GetValue("targetType").ToLower();
						switch (type) {
							case "reachspace": return "reach space";
							case "flyby": return "fly by " + TargetBody;
							case "spacewalk": return "perform spacewalk above " + TargetBody;
							case "landing": return "land on " + TargetBody;
							case "firstlaunch": return "launch a spacecraft";
							case "orbit": return "orbit " + TargetBody;
							case "rendezvous": return "perform a rendezvous";
							default: throw new NotImplementedException("Unknown world first contract tpye: " + type);
						}
					case ContractType.SatelliteContract:
						return "place satelltie in orbit around " + TargetBody;
					case ContractType.ExploreBody:
						return "explore " + Body;
					case ContractType.ISRUContract:
						return "gather " + Node.GetValue("resourceTitle") + " from " + TargetBody;
					case ContractType.SurveyContract:
						return "survey " + TargetBody;
					default: throw new NotImplementedException("No name defined for contract type " + Type);
				}
			}
		}
	}
}
