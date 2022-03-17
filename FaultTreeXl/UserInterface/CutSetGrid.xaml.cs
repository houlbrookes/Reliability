using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FaultTreeXl.UserInterface
{
    /// <summary>
    /// Interaction logic for CutSetGrid.xaml
    /// </summary>
    public partial class CutSetGrid : Window
    {
        public CutSetGrid()
        {
            InitializeComponent();
        }
    }

    public class CutSetDisplay
    {
        public string Failure_Rate { get;  }
        public string PFD { get; private set; }
        public string MDT { get; private set; }
        public string Cut_Sets { get; private set; }

        public CutSetDisplay(decimal fr, decimal pfd, decimal mdt, string cs)
        {
            Failure_Rate = fr.ToString("E2");
            PFD = pfd.ToString("E2"); ;
            MDT = mdt.ToString("N0"); ;
            Cut_Sets = cs;
        }
    }

    public class CutSetGridContext : NotifyPropertyChangedItem
    {
        public List<CutSetDisplay> Data { get; set; }

        private List<CutSet> cutSets;
        public List<CutSet> CutSets
        {
            get => cutSets;
            set
            {
                Data = new List<CutSetDisplay>();
                foreach(var cs in value.OrderByDescending(c => c.PFD))
                {
                    Data.Add(new CutSetDisplay(cs.Lambda, cs.PFD, cs.MDT, string.Join(" | ", cs.Nodes.Select(c => c.Name))));
                }
                Notify(nameof(Data));
                Changed(ref cutSets, value, nameof(CutSets));
            }
        }
    }
}
