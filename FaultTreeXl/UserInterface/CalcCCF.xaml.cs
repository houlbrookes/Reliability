using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for CalcCCF.xaml
    /// </summary>
    public partial class CalcCCF : Window, INotifyPropertyChanged
    {
        internal T Changed<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null)
        {
            property = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return property;
        }

        private FaultTreeModel _faultTreeModel = null;
        public FaultTreeModel FTA { get => _faultTreeModel; set => Changed(ref _faultTreeModel, value); }

        private decimal _beta = 0.1M;
        public decimal Beta { get => _beta;
            set
            {
                Changed(ref _beta, value);
                NewLambda = FTA.HighlightedNode.Lambda * Beta;
            }
        }

        private decimal _newLambda = 0;
        public decimal NewLambda { get => _newLambda; set => Changed(ref _newLambda, value); }

        public CalcCCF()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
