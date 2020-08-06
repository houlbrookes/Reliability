
using FaultTreeXl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FT_Test
{
    [TestClass]
    public class Voted2oo3Tests
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

        public Voted2oo3 New2oo3(params GraphicItem[] nodes)
        {
            var result = new Voted2oo3(nodes[0], nodes[1], nodes[2]);
            return result;
        }

        [TestMethod]
        public void TestCutSets()
        {
            var v2oo3 = New2oo3(NewNode("A"), NewNode("B"), NewNode("C"));
            var result = v2oo3.CutSets.AsString();
            Assert.AreEqual("AB∪AC∪BC", v2oo3.CutSets.AsString(), "Cut sets are not as expected");
            
        }

        [TestMethod]
        public void TestPFD()
        {
            var v2oo3 = New2oo3(NewNode("A"), NewNode("B"), NewNode("C"));
            var result =(double)v2oo3.PFD;
            decimal d1 = (decimal)(1E-6 * 8760)*(decimal)(1E-6 * 8760)/3;
            decimal d2 = (1 - d1)* (1 - d1) * (1 - d1);
            decimal expected = 1M - d2;
            Assert.AreEqual((double)expected, (double)result, 1E-6, "PFD are not as expected");
        }

        [TestMethod]
        public void TestLambda()
        {
            var v2oo3 = New2oo3(NewNode("A"), NewNode("B"), NewNode("C"));
            var result = (double)v2oo3.Lambda;
            double expected = 3 * (1E-6) * (1E-6) * 8760 ;

            Assert.AreEqual((double)expected, (double)result, 1E-6, "PFD are not as expected");
        }

        [TestMethod]
        public void TestMDT()
        {
            var v2oo3 = New2oo3(NewNode("A"), NewNode("B"), NewNode("C"));
            var result = (double)v2oo3.MDT;

            decimal d1 = (decimal)(1E-6 * 8760) * (decimal)(1E-6 * 8760) / 3;
            decimal d2 = (1 - d1) * (1 - d1) * (1 - d1);
            double PFD = (double)(1M - d2);
            double FailureRate = 3 * (1E-6) * (1E-6) * 8760;

            double expected = PFD/FailureRate;

            Assert.AreEqual((double)expected, (double)result, 1E-6, "PFD are not as expected");
        }
    }
}
