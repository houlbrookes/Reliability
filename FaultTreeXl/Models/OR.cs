using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    public partial class OR : GraphicItem
    {
        public override string NodeType => "OR";


        [XmlIgnore]
        public override List<CutSet> CutSets
        {
            get
            // Just flatten the list of list of list of nodes
            // and remove duplicates
            {
                var allCutSets = new CutSetList(Nodes.SelectMany(n => n.CutSets));
                allCutSets.Distinct();
                allCutSets.RemoveSuperSets();
                return allCutSets.ToList();
            }
        }

        [XmlIgnore]
        public override decimal Lambda
        {
            get => Nodes.Sum(n => n.Lambda);
            set => base.Lambda = value;
        }
    }

    /// <summary>
    /// Simulation Extensions
    /// </summary>
    public partial class OR
    {
        public override SimulationState CalculateState(double currentClock)
        {
            SimulationState result = CurrentState;
            var timeElapsed = currentClock - LastEvent;

            var isBroken = Nodes.Any(n => n.CurrentState == SimulationState.Broken);
            if (isBroken && CurrentState==SimulationState.Working)
            {
                // Just failied
                CurrentState = SimulationState.Broken;
                Uptime += timeElapsed;
                FailureCount += 1;
                result = CurrentState;
                Parent?.CalculateState(currentClock);
                LastEvent = currentClock;
            }
            else if (!isBroken && CurrentState == SimulationState.Broken)
            {
                // Just repaired
                CurrentState = SimulationState.Working;
                Downtime += timeElapsed;
                RepairCount += 1;
                result = CurrentState;
                Parent?.CalculateState(currentClock);
                LastEvent = currentClock;
            }


            Notify(nameof(this.SimulatedFailureRate));
            Notify(nameof(this.SimulatedPFD));
            Notify(nameof(this.SimulatedMeanDowntime));


            return result;
        }
    }
}
