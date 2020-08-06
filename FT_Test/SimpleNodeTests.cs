
using FaultTreeXl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FT_Test
{

    [TestClass]
    public class SimpleNodeTests
    {
        Node SingleNode = null;
        OR TwoNodeOR = null;
        AND TwoNodeAND = null;
        AND TwoNodeANDHalfPTI = null;

        public Node NewNode(string name, decimal lambda = 1E-6M, decimal pti = 8760M, decimal c = 1, decimal lt = 8760)
        => new Node() {
            Name = name,
            Lambda = lambda,
            PTI = pti,
            ProofTestEffectiveness = c,
            LifeTime = lt };

        public OR NewOR(params GraphicItem[] nodes)
        {
            var result = new OR() ;
            foreach (var node in nodes) result.Nodes.Add(node);
            return result;
        }

        public AND NewAND(params GraphicItem[] nodes)
        {
            var result = new AND();
            foreach (var node in nodes) result.Nodes.Add(node);
            return result;
        }

        [TestInitialize]
        public void StartUp()
        {
            SingleNode = NewNode("A");
            TwoNodeOR = NewOR(NewNode("B"), NewNode("C"));
            TwoNodeAND = NewAND(NewNode("D"), NewNode("E"));
            TwoNodeANDHalfPTI = NewAND(NewNode("E"), NewNode("F", pti: 8760 / 2));
        }

        [TestMethod]
        public void TestMDT_Node()
        {
            Assert.AreEqual(8760 / 2, (double)SingleNode.MDT, 1, "MDT Not calculated correctly for Node");
        }

        [TestMethod]
        public void TestMDT_OR()
        {
            var pfd = 1 - Math.Pow(1 - (1E-6 * 8760 / 2), 2);
            var fail = 2E-6;
            var expectedMDT = pfd / fail;

            Assert.AreEqual(expectedMDT, (double)TwoNodeOR.MDT, 1, "MDT Not calculated correctly for OR");
        }

        [TestMethod]
        public void TestMDT_AND()
        {
            Assert.AreEqual(8760 / 3, (double)TwoNodeAND.MDT, 0.2, "MDT Not calculated correctly for 1oo2 AND");
        }

        [TestMethod]
        public void TestMDT_AND_UnequalPTI()
        {
            Assert.AreEqual(8760 * 7 / 36, (double)TwoNodeANDHalfPTI.MDT, 0.2, "MDT Not calculated correctly for 1oo2 AND with one 1/2 PTI");
        }

        [TestMethod]
        public void TestPFD_Node()
        {
            Assert.AreEqual((1E-6) * 8760 / 2, (double)SingleNode.PFD, 1E-6, "PFD Not calculated correctly for Node");
        }

        [TestMethod]
        public void TestPFD_OR()
        {
            double expectedPFD = (double)(1M - (1M - 1E-6M * 8760M/2M) * (1M - 1E-6M * 8760M/2M));
            Assert.AreEqual(expectedPFD, (double)TwoNodeOR.PFD, 1E-6, "PFD Not calculated correctly for OR");
        }

        [TestMethod]
        public void TestPFD_AND()
        {
            Assert.AreEqual(Math.Pow(1E-6, 2) * Math.Pow(8760, 2) / 3, (double)TwoNodeAND.PFD, 1E-6, "PFD Not calculated correctly for 1oo2");
        }
        [TestMethod]
        public void TestPFD_AND_HalfProof()
        {
            Assert.AreEqual(Math.Pow(1E-6, 2) * Math.Pow(8760, 2) * 7 / 48, (double)TwoNodeANDHalfPTI.PFD, 1E-6, "MDT Not calculated correctly for 1oo2 AND with one 1/2 PTI");
        }

        [TestMethod]
        public void TestFailureRate_OR()
        {
            Assert.AreEqual(2E-6, (double)TwoNodeOR.Lambda, 1E-6, "Failure rate Not calculated correctly for OR");
        }

        [TestMethod]
        public void TestFailureRate_AND()
        {
            Assert.AreEqual(Math.Pow(1E-06, 2) * 8760, (double)TwoNodeAND.Lambda, 1E-9, "Failure rate Not calculated correctly for 1oo2");
        }

        [TestMethod]
        public void TestFailureRate_AND_HalfProof()
        {
            Assert.AreEqual(Math.Pow(1E-6, 2) * 8760 * 3 / 4, (double)TwoNodeANDHalfPTI.Lambda, 1E-8, "Failiure Rate Not calculated correctly for 1oo2 AND with one 1/2 PTI");
        }

    }
}
