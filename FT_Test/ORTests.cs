
using FaultTreeXl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FT_Test
{

    [TestClass]
    public class ORTests
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
            OR result = new OR();
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

        [TestMethod]
        public void TestMDT2()
        {
            OR or = NewOR(NewOR(NewNode("A"), NewNode("B")),
                          NewOR(NewNode("C"), NewNode("D")));

            double pfd = 1 - Math.Pow(1 - (1E-6 * 8760 / 2), 4);
            double fail = 4E-6;
            double expectedMDT = pfd / fail;

            Assert.AreEqual(expectedMDT, (double)or.MDT, 1, "MDT Not calculated correctly for OR");
        }

        [TestMethod]
        public void TestPFD()
        {
            OR or = NewOR(NewOR(NewNode("A"), NewNode("B")),
                          NewOR(NewNode("C"), NewNode("D")));
            Assert.AreEqual(
                (double)
                  (1M
                - (1M - 1E-06M * 8760M / 2M)
                * (1M - 1E-06M * 8760M / 2M)
                * (1M - 1E-06M * 8760M / 2M)
                * (1M - 1E-06M * 8760M / 2M)),
                (double)or.PFD, 1E-5, "PFD Not calculated correctly for OR");
        }

        [TestMethod]
        public void TestFailureRate()
        {
            OR or = NewOR(NewOR(NewNode("A"), NewNode("B")),
                          NewOR(NewNode("C"), NewNode("D")));

            double oneFailure = 1E-6;
            double allFailures = 4 * oneFailure;

            Assert.AreEqual(allFailures,
                (double)or.Lambda, 1E-6, "Failure Rate Not calculated correctly for OR");
        }

        [TestMethod]
        public void TestFailureRateORANDS()
        {
            OR or = NewOR(NewAND(NewNode("A"), NewNode("B")),
                          NewAND(NewNode("B"), NewNode("C")));

            double allFailures = 2 * 1E-12 * 8760;
            Assert.AreEqual(allFailures,
                (double)or.Lambda, 1E-6, "Failure Rate Not calculated correctly for OR + ANDs");

            double PFD = 1 - Math.Pow(1 - 1E-12 * 8760 * 8760 / 3, 2);
            Assert.AreEqual(PFD,
                (double)or.PFD, 1E-6, "PFD Not calculated correctly for OR + ANDs");
        }

    }
}
