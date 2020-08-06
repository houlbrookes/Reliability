
using FaultTreeXl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FT_Test
{


    [TestClass]
    public class ANDTests
    {
        Node SingleNode = null;
        OR TwoNodeOR = null;
        AND TwoNodeAND = null;
        AND TwoNodeANDHalfPTI = null;

        public Node NewNode(string name, decimal lambda = 1E-6M, decimal pti = 8760M, decimal c = 1, decimal lt = 8760)
        => new Node()
        {
            Name=name,
            Lambda = lambda,
            PTI = pti,
            ProofTestEffectiveness = c,
            LifeTime = lt
        };

        public OR NewOR(params GraphicItem[] nodes)
        {
            var result = new OR();
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
            SingleNode = NewNode("E");
            TwoNodeOR = NewOR(NewNode("A"), NewNode("B"));
            TwoNodeAND = NewAND(NewNode("C"), NewNode("D"));
            TwoNodeANDHalfPTI = NewAND(NewNode("F"), NewNode("G", pti: 8760 / 2));
        }

        [TestMethod]
        public void TestPDT()
        {

            AND and = NewAND(
                NewOR(NewNode("A"), NewNode("B")),
                NewOR(NewNode("C"), NewNode("D"))
                );

            Assert.AreEqual(
                (double)
                  (1M -
                  (1M - 1E-12M * 8760M * 8760M / 3M)
                * (1M - 1E-12M * 8760M * 8760M / 3M)
                * (1M - 1E-12M * 8760M * 8760M / 3M)
                * (1M - 1E-12M * 8760M * 8760M / 3M)),
                (double)and.PFD, 1E-7, "PFD Not calculated correctly for OR");
        }
    }

}
