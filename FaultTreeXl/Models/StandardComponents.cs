using System.Collections.ObjectModel;

namespace FaultTreeXl
{
    public class StandardComponents : NotifyPropertyChangedItem
    {
        private ObservableCollection<GraphicItem> _standardParts = new ObservableCollection<GraphicItem>()
        {
            new Node {Name="ACT1", Description="Siemens PS2 Actuator", Lambda=1.52E-07M, PTI=8760, LifeTime=87600, ProofTestEffectiveness=1.0M},
            new Node {Name="BD120", Description="Lee Dickens", Lambda=1.95E-06M, PTI=8760, LifeTime=87600, ProofTestEffectiveness=1.0M},
            new Node {Name="MSR127", Description="Rockwell MSR127", Lambda=4.25E-08M, PTI=8760, LifeTime=87600, ProofTestEffectiveness=1.0M},
            new Node {Name="SRM", Description="Moores SRM", Lambda=6.25E-08M, PTI=4380, LifeTime=87600, ProofTestEffectiveness=1.0M},
            new Node {Name="DSIII", Description="Siemens DSIII", Lambda=4.57E-07M, PTI=4380, LifeTime=87600, ProofTestEffectiveness=1.0M},
            new Node {Name="BD320", Description="Lee Dickens BD320", Lambda=1.33E-06M, PTI=8760, LifeTime=87600, ProofTestEffectiveness=1.0M},
            new Node {Name="SR1", Description="Generic SR Welded", Lambda=2.83E-08M, PTI=8760, LifeTime=87600, ProofTestEffectiveness=1.0M},
            new Node {Name="N8", Description="Desc", Lambda=1E-06M, PTI=8760, LifeTime=87600, ProofTestEffectiveness=1.0M},
            new Node {Name="N9", Description="Desc", Lambda=1E-06M, PTI=8760, LifeTime=87600, ProofTestEffectiveness=1.0M},
            new Node {Name="N10", Description="Desc", Lambda=1E-06M, PTI=8760, LifeTime=87600, ProofTestEffectiveness=1.0M},
        };
        public ObservableCollection<GraphicItem> StandardParts
        {
            get => _standardParts;
            set => Changed(ref _standardParts, value);
        }
        public StandardComponents()
        {
            //StandardParts.Add(new Node {Name="N1", Description="Desc", Lambda=1E-06M, PTI=8760, LifeTime=87600, ProofTestEffectiveness=1.0M});
            //StandardParts.Add(new Node { Name = "N2", Description = "Desc", Lambda = 1E-06M, PTI = 8760, LifeTime = 87600, ProofTestEffectiveness = 1.0M });
            //StandardParts.Add(new Node { Name = "N3", Description = "Desc", Lambda = 1E-06M, PTI = 8760, LifeTime = 87600, ProofTestEffectiveness = 1.0M });
            //StandardParts.Add(new Node { Name = "N4", Description = "Desc", Lambda = 1E-06M, PTI = 8760, LifeTime = 87600, ProofTestEffectiveness = 1.0M });
            //StandardParts.Add(new Node { Name = "N5", Description = "Desc", Lambda = 1E-06M, PTI = 8760, LifeTime = 87600, ProofTestEffectiveness = 1.0M });
            //StandardParts.Add(new Node { Name = "N6", Description = "Desc", Lambda = 1E-06M, PTI = 8760, LifeTime = 87600, ProofTestEffectiveness = 1.0M });
            //StandardParts.Add(new Node { Name = "N7", Description = "Desc", Lambda = 1E-06M, PTI = 8760, LifeTime = 87600, ProofTestEffectiveness = 1.0M });
            //StandardParts.Add(new Node { Name = "N8", Description = "Desc", Lambda = 1E-06M, PTI = 8760, LifeTime = 87600, ProofTestEffectiveness = 1.0M });
            //StandardParts.Add(new Node { Name = "N9", Description = "Desc", Lambda = 1E-06M, PTI = 8760, LifeTime = 87600, ProofTestEffectiveness = 1.0M });
            //StandardParts.Add(new Node { Name = "N10", Description = "Desc", Lambda = 1E-06M, PTI = 8760, LifeTime = 87600, ProofTestEffectiveness = 1.0M });
        }

    }

}
