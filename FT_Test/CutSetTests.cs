
using FaultTreeXl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FT_Test
{
    [TestClass]
    public class CutSetTests
    {
        private Node SingleNode = null;
        private OR TwoNodeOR = null;
        private AND TwoNodeAND = null;
        private AND TwoNodeANDHalfPTI = null;

        public Node NewNode(string name = "A", decimal lambda = 1E-6M, decimal pti = 8760M, decimal c = 1, decimal lt = 8760)
        => new Node()
        {
            Name = name,
            Lambda = lambda,
            PTI = pti,
            ProofTestEffectiveness = c,
            LifeTime = lt
        };

        public OR NewOR(string name = "OR?", params GraphicItem[] nodes)
        {
            OR result = new OR() { Name = name };
            foreach (GraphicItem node in nodes)
            {
                result.Nodes.Add(node);
            }

            return result;
        }

        public AND NewAND(params GraphicItem[] nodes)
        {
            AND result = new AND();
            foreach (GraphicItem node in nodes)
            {
                result.Nodes.Add(node);
            }

            return result;
        }

        [TestInitialize]
        public void StartUp()
        {
            SingleNode = NewNode();
            TwoNodeOR = NewOR("OR1", NewNode(), NewNode());
            TwoNodeAND = NewAND(NewNode(), NewNode());
            TwoNodeANDHalfPTI = NewAND(NewNode(), NewNode(pti: 8760 / 2));
        }

        private CutSet NewCutSet(params GraphicItem[] nodes)
        {
            CutSet cs = new CutSet(nodes);
            return cs;
        }

        [TestMethod]
        public void TestZeroNodePDT()
        {
            CutSet cs = NewCutSet();
            Assert.AreEqual(1,
                (double)cs.PFD, 1E-7, "PFD Not calculated correctly for CutSet");
        }
        [TestMethod]
        public void TestOneNodePDT()
        {
            CutSet cs = NewCutSet(NewNode("A"));
            Assert.AreEqual(1E-06 * 8760 / 2,
                (double)cs.PFD, 1E-7, "PFD Not calculated correctly for CutSet");
        }
        [TestMethod]
        public void TestTwoNodesPFD()
        {
            CutSet cs = NewCutSet(NewNode("B"), NewNode("C"));
            double expectedPFD = (1E-06 * 8760) * (1E-06 * 8760) / 3;
            Assert.AreEqual(expectedPFD,
                (double)cs.PFD, 1E-7, "PFD Not calculated correctly for CutSet");
        }
        [TestMethod]
        public void TestThreeNodesPFD()
        {
            CutSet cs = NewCutSet(NewNode("A"), NewNode("B"), NewNode("C"));
            double expectedPFD = (1E-06 * 8760) * (1E-06 * 8760) * (1E-06 * 8760) / 4;
            Assert.AreEqual(expectedPFD,
                (double)cs.PFD, 1E-7, "PFD Not calculated correctly for CutSet");
        }

        [TestMethod]
        public void TestFourNodesPFD()
        {
            CutSet cs = NewCutSet(NewNode("A"), NewNode("B"), NewNode("C"), NewNode("D"));
            double expectedPFD = (1E-06 * 8760) * (1E-06 * 8760) * (1E-06 * 8760) * (1E-06 * 8760) / 5;
            Assert.AreEqual(expectedPFD,
                (double)cs.PFD, 1E-7, "PFD Not calculated correctly for CutSet");
        }

        [TestMethod]
        public void TestFiveNodesPFD()
        {
            CutSet cs = NewCutSet(NewNode("A"), NewNode("B"), NewNode("C"), NewNode("D"), NewNode("E"));
            double expectedPFD = (1E-06 * 8760) * (1E-06 * 8760) * (1E-06 * 8760) * (1E-06 * 8760) * (1E-06 * 8760) / 6;
            Assert.AreEqual(expectedPFD,
                (double)cs.PFD, 1E-7, "PFD Not calculated correctly for CutSet");
        }

        [TestMethod]
        public void TestTwoCutSets()
        {
            OR or1 = NewOR("OR1", NewNode("A"));
            OR or2 = NewOR("OR2", NewNode("C"));
            AND theAnd = NewAND(or1, or2);
            Assert.AreEqual("A", or1.CutSetsAsString);
            Assert.AreEqual("C", or2.CutSetsAsString);
            Assert.AreEqual("AC", theAnd.CutSetsAsString, "Combined CS not correct");
            double expectedPFD = (1E-06 * 8760) * (1E-06 * 8760) / 3;
            Assert.AreEqual(expectedPFD,
                (double)theAnd.PFD, 1E-7, "PFD Not calculated correctly for CutSet");
        }

        /// <summary>
        /// Tests if duplicated-names are removed
        /// from cut sets
        /// </summary>
        [TestMethod]
        public void TestDuplicateCutSets()
        {
            OR or1 = NewOR("OR1", NewNode("A"), NewNode("A"));
            Assert.AreEqual("A", or1.CutSets.AsString(), "Identical items in an OR");
        }

        [TestMethod]
        public void TestDuplicateCutSets2()
        {
            OR or1 = NewOR("OR1", 
                NewAND(NewNode("A"), NewNode("B")), 
                NewAND(NewNode("B"), NewNode("A")));
            Assert.AreEqual("AB", or1.CutSets.AsString(), "Identical items in an OR");
        }

        /// <summary>
        /// Tests if duplicated-names are removed
        /// from cut sets
        /// </summary>
        [TestMethod]
        public void TestDuplicateCutSets3()
        {
            var node = NewAND(NewNode("A"), NewNode("A"));
            Assert.AreEqual("A", node.CutSets.AsString(), "Identical items in an AND");
        }

        [TestMethod]
        public void TestZeroNodeLambda()
        {
            CutSet cs = NewCutSet();
            Assert.AreEqual(0,
                (double)cs.Lambda, 1E-7, "Lambda Not calculated correctly for CutSet");
        }
        [TestMethod]
        public void TestOneNodeLambda()
        {
            CutSet cs = NewCutSet(NewNode());
            Assert.AreEqual(1E-06,
                (double)cs.Lambda, 1E-7, "PFD Not calculated correctly for CutSet");
        }
        [TestMethod]
        public void TestTwoNodesLambda()
        {
            CutSet cs = NewCutSet(NewNode("A"), NewNode("B"));
            double expected = (1E-12 * 8760);
            Assert.AreEqual(expected,
                (double)cs.Lambda, 1E-7, "PFD Not calculated correctly for CutSet");
        }
        [TestMethod]
        public void TestThreeNodesLambda()
        {
            CutSet cs = NewCutSet(NewNode("A"), NewNode("B"), NewNode("C"));
            double expected = (1E-06) * (1E-06 * 8760) * (1E-06 * 8760);
            Assert.AreEqual(expected,
                (double)cs.Lambda, 1E-7, "PFD Not calculated correctly for CutSet");
        }

        [TestMethod]
        public void TestFourNodesLambda()
        {
            CutSet cs = NewCutSet(NewNode("A"), NewNode("B"), NewNode("C"), NewNode("D"));
            double expected = (1E-06) * (1E-06 * 8760) * (1E-06 * 8760) * (1E-06 * 8760);
            Assert.AreEqual(expected,
                (double)cs.Lambda, 1E-7, "PFD Not calculated correctly for CutSet");
        }

        [TestMethod]
        public void TestFiveNodesLambda()
        {
            CutSet cs = NewCutSet(NewNode("A"), NewNode("B"), NewNode("C"), NewNode("D"), NewNode("E"));
            double expectedPFD = (1E-06) * (1E-06 * 8760) * (1E-06 * 8760) * (1E-06 * 8760) * (1E-06 * 8760);
            Assert.AreEqual("ABCDE", cs.AsString());
            Assert.AreEqual(expectedPFD,
                (double)cs.Lambda, 1E-7, "PFD Not calculated correctly for CutSet");
        }

        [TestMethod]
        public void Test2AND()
        {
            AND and = NewAND(NewNode("A"), NewNode("B"), NewNode("C"));
            Assert.AreEqual("ABC", and.CutSets.AsString());
            AND and2 = NewAND(
                NewOR("OR1", NewNode("A")),
                NewOR("OR2", NewNode("B"), NewNode("C")));
            Assert.AreEqual("AB∪AC", and2.CutSets.AsString());
            AND and3 = NewAND(
                NewOR("OR1", NewNode("A"), NewNode("B")),
                NewOR("OR2", NewNode("C"), NewNode("D")));
            Assert.AreEqual("AC∪AD∪BC∪BD", and3.CutSets.AsString());
        }
        [TestMethod]
        public void Test3AND()
        {
            AND and4 = NewAND(
                NewOR("OR1", NewNode("A"), NewNode("B")),
                NewOR("OR2", NewNode("C"), NewNode("D")),
            NewOR("OR3", NewNode("E"), NewNode("F")));
            Assert.AreEqual("ACE∪ACF∪ADE∪ADF∪BCE∪BCF∪BDE∪BDF", and4.CutSets.AsString());
        }
        [TestMethod]
        public void Test4AND()
        {
            AND and5 = NewAND(
                NewOR("OR1", NewNode("A"), NewNode("B")),
                NewAND(NewNode("C"), NewNode("D")),
                NewOR("OR3", NewNode("E"), NewNode("F")));
            Assert.AreEqual("ACDE∪ACDF∪BCDE∪BCDF", and5.CutSets.AsString());
        }
        [TestMethod]
        public void Test5AND()
        {
            AND and6 = NewAND(
                NewOR("OR1", NewNode("A"), NewNode("B")),
                NewAND(NewNode("C"), NewOR("OR3", NewNode("E"), NewNode("F")))
                );
            Assert.AreEqual("ACE∪ACF∪BCE∪BCF", and6.CutSets.AsString());
        }
        [TestMethod]
        public void Test5ORwithShared()
        {
            // in this test a common component is shared
            AND and6 = NewAND(
                NewOR("OR1", NewNode("A"), NewNode("F")),
                NewOR("OR2", NewNode("B"), NewNode("F"))
                );
            Assert.AreEqual("AB∪F", and6.CutSets.AsString());
        }
    }
}
