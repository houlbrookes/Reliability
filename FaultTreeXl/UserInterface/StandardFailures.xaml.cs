using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Xml.Serialization;

namespace FaultTreeXl
{
    /// <summary>
    /// Interaction logic for StandardFailures.xaml
    /// </summary>
    public partial class StandardFailures : Window
    {
        public StandardFailures()
        {
            InitializeComponent();
        }

    }

    public class StandardFailure
    {
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public decimal Rate { get; set; }
    }

    public class UpdateWithStandardFailureCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        private StandardFailure _Selected = null;
        public StandardFailure Selected
        {
            get => _Selected;
            set
            {
                _Selected = value;
            }
        }
        public Window CalledFromWindow { get; set; }

        public void Execute(object parameter)
        {
            if (parameter is GraphicItem item2Update)
            {
                if (Selected != null)
                {
                    item2Update.Lambda = Selected.Rate;
                }
            }
            CalledFromWindow?.Close();
        }
    }

    public class StandardFailiuresController
    {
        public ICommand Save { get; set; } = new GenericCommand<ObservableCollection<StandardFailure>>
        {
            CanExecuteProxy = p => true,
            ExecuteProxy = p => SaveList(p),
        };

        public ICommand Load { get; set; } = new GenericCommand<ObservableCollection<StandardFailure>>
        {
            CanExecuteProxy = p => true,
            ExecuteProxy = p => LoadList(p),
        };

        public StandardFailiuresController()
        {
            LoadList(FailureRates);
        }

        public GraphicItem ItemToUpdate { get; set; }
        public StandardFailure Selected { get; set; }

        public ICommand UpdateCommand { get; set; } = new UpdateWithStandardFailureCommand();
        public Window CalledFromWindow
        {
            set => (UpdateCommand as UpdateWithStandardFailureCommand).CalledFromWindow = value;
        }

        private static void SaveList(ObservableCollection<StandardFailure> list)
        {
            string fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "StandardParts.xml");
            //Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            //System.Reflection.Assembly.GetExecutingAssembly().Location
            using (StreamWriter sw = new StreamWriter(path: fileName, append: false))
            {
                XmlSerializer x = new XmlSerializer(list.GetType());
                x.Serialize(sw, list);
            }

        }

        public static void LoadList(ObservableCollection<StandardFailure> list)
        {
            string fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "StandardParts.xml");
            //Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            //System.Reflection.Assembly.GetExecutingAssembly().Location
            using (StreamReader streamReader = new StreamReader(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<StandardFailure>));
                ObservableCollection<StandardFailure> data = serializer.Deserialize(streamReader) as ObservableCollection<StandardFailure>;
                list.Clear();
                foreach (StandardFailure x in data)
                {
                    list.Add(x);
                }
            }
        }

        public ObservableCollection<StandardFailure> FailureRates { get; set; } = new ObservableCollection<StandardFailure> { };
    }
}
