
using FaultTreeXl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FT_Test
{
    [TestClass]
    public class DiagnosedFaultTests
    {
        public Node NewNode(string name, decimal lambda = 1E-6M, decimal pti = 8760M, decimal c = 1, decimal lt = 8760)
        => new Node()
        {
            Name = name,
            Lambda = lambda,
            PTI = pti,
            ProofTestEffectiveness = c,
            LifeTime = lt
        };

        public DiagnosedFaultNode NewDiagnosedNode(string name, decimal lambda = 1E-6M, decimal mdt = 72M)
        => new DiagnosedFaultNode()
        {
            Name = name,
            Lambda = lambda,
            PTI = mdt,
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
        }

        [TestMethod]
        public void TestPDT()
        {

            DiagnosedFaultNode node = NewDiagnosedNode("A");
            double Expected = 1E-6 * 72;
            double Actual = (double)node.PFD;
            Assert.AreEqual(Expected, Actual, 1E-7, "PFD Not calculated correctly for Single Diagnosed Failure");
        }
        [TestMethod]
        public void TestPDT2()
        {
            AND anAND = NewAND(NewDiagnosedNode("A"), NewNode("B"));
            double Expected = 1E-6 * 72 * 1E-6 * 8760 / 2;
            double Actual = (double)anAND.PFD;
            Assert.AreEqual(Expected, Actual, 1E-7, "PFD Not calculated correctly for Diagnosed / Undiagnosed");
        }
        [TestMethod]
        public void TestPDT3()
        {
            AND anAND = NewAND(NewDiagnosedNode("A"), NewNode("B"), NewNode("C"));
            double Expected = 1E-6 * 72 * 1E-6 * 8760 * 1E-6 * 8760 / 3;
            double Actual = (double)anAND.PFD;
            Assert.AreEqual(Expected, Actual, 1E-7, "PFD Not calculated correctly for Diagnosed / Undiagnosed");
        }
        [TestMethod]
        public void TestPDT4()
        {
            AND anAND = NewAND(NewDiagnosedNode("A"), NewDiagnosedNode("B"), NewNode("C"), NewNode("D"));
            double Expected = 1E-6 * 72 * 1E-6 * 72 * 1E-6 * 8760 * 1E-6 * 8760 / 3;
            double Actual = (double)anAND.PFD;
            Assert.AreEqual(Expected, Actual, 1E-7, "PFD Not calculated correctly for Diagnosed / Undiagnosed");
        }
    }

}
