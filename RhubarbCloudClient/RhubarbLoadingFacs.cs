using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

namespace RhubarbCloudClient
{
	public static class RhubarbLoadingFacts
	{
		public static string[] RhubarbFacts = new string[] {
			"Fact.Rhubarb.webclient",
			"Fact.Rhubarb.RhuEngine",
			"Fact.Rhubarb.BackEnds",
			"Fact.Rhubarb.SkinMeshes",
			"Fact.Rhubarb.EcmaScipt",
			"Fact.Rhubarb.JavaScript",
			"Fact.Rhubarb.2",
			"Fact.Rhubarb.vegetable",
			"Fact.Rhubarb",
			"Fact.Rhubarb.OpenSorce",
			"Fact.Rhubarb.Pie"
		};

		public static string[] RandomFacts = new string[] {
			"Fact.Rand0",
			"Fact.Rand1",
			"Fact.Rand2",
			"Fact.Rand3",
			"Fact.Rand4",
			"Fact.Rand5",
			"Fact.Rand6",
			"Fact.Rand7",
			"Fact.Rand8",
			"Fact.Rand9",
			"Fact.Rand10",
			"Fact.Rand11",
			"Fact.Rand12",
			"Fact.Rand13",
			"Fact.Rand14",
			"Fact.Rand15",
			"Fact.Rand16",
			"Fact.Rand17",
			"Fact.Rand18",
			"Fact.Rand19",
			"Fact.Rand20",
		};


		public static string[] FunnyFacts = new string[] {
			"Fact.Fun.FaolanTrains",
			"Fact.Fun.RayTraxing.removed",
			"Fact.Fun.RayTraxing.added",
			"Fact.Fun.Pineapples",
			"Fact.Fun.Trains",
			 "Fact.Fun.Trees",
			 "Fact.Fun.NoDont",
			"Fact.Fun.Index",
			 "Fact.Fun.WaterDrinking",
			"Fact.Fun.Mine",
			"Fact.Fun.Cat",
			"Fact.Fun.Blank",
		};

		public static string[] GetSleepFacts = new string[] {
			"Fact.Sleep.Rise",
			"Fact.Sleep.Dark",
			"Fact.Sleep.Week",
		};
		public static string GetRandomFunnyFact(Random random) {
			return FunnyFacts[random.Next(FunnyFacts.Length - 1)];
		}

		public static string GetRandomFact(Random random) {
			return RandomFacts[random.Next(RandomFacts.Length - 1)];
		}

		public static string GetRandomRhubarbFact(Random random) {
			return RhubarbFacts[random.Next(RhubarbFacts.Length - 1)];
		}

		public static string GetRandomFactNoDate(Random random) {
			var ran = random.Next(9);
			if (ran <= 1) {
				return GetRandomFunnyFact(random);
			}
			if (ran <= 3) {
				return GetRandomFact(random);
			}
			return GetRandomRhubarbFact(random);
		}

		public static string GetRandomFact(Localisation loc) {
			var currentTime = DateTime.Now;
			var randomGen = new Random();
			if (currentTime.Hour <= 5) {
				if (randomGen.Next(10) <= 1) {
					var num = randomGen.Next(GetSleepFacts.Length);
					return loc.GetLocalString(num == GetSleepFacts.Length ? $"Fact.Fun.Blank;{currentTime.Hour}" : GetSleepFacts[num]);
				}
			}
			var random = GetRandomFactNoDate(randomGen);
			var daySting = $"Fact.Month{currentTime.Month}.Day{currentTime.Day}";
			var valueDayFact = loc.GetLocalString(daySting);
			return daySting != valueDayFact ? (randomGen.Next(10) <= 3) ? valueDayFact : loc.GetLocalString(random) : loc.GetLocalString(random);

		}
	}
}
