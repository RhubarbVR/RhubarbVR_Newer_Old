using Microsoft.VisualStudio.TestTools.UnitTesting;

using RNumerics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNumerics.Tests
{
	[TestClass()]
	public class StringHelperTests
	{
		[TestMethod()]
		public void TestDistances() {
			Assert.AreEqual(0, "Book".DamerauLevenshteinDistance("Book"));
			Assert.AreEqual(1, "Book".DamerauLevenshteinDistance("Booc"));
			Assert.AreEqual(2, "geeks".DamerauLevenshteinDistance("fogeeks"));
			Assert.AreEqual(3, "geeks".DamerauLevenshteinDistance("forgeeks"));
			Assert.AreEqual(1, "Booc".DamerauLevenshteinDistance("Book"));
			Assert.AreEqual(2, "fogeeks".DamerauLevenshteinDistance("geeks"));
			Assert.AreEqual(3, "forgeeks".DamerauLevenshteinDistance("geeks"));
			Assert.AreEqual(15, "urgduihgiudrh".DamerauLevenshteinDistance("fesmcvlksnmvieo"));
		}
	}
}