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
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public StandardFailure Selected { get; set; }
        public Window CalledFromWindow { get; set; }

        public void Execute(object parameter)
        {
            if (parameter is GraphicItem item2Update)
            {
                item2Update.Lambda = Selected.Rate;
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

        public ObservableCollection<StandardFailure> FailureRates { get; set; } = new ObservableCollection<StandardFailure>
        {
            new StandardFailure{Type="Safety Relay", Name="MSR 127", Rate=4.25E-08M },
            new StandardFailure{Type="Safety Relay", Name="Moores SRM", Rate=1E-06M },
            new StandardFailure{Type="Relay (Armature)", Name="General", Rate=2.83E-07M },
            new StandardFailure{Type="Contactor", Name="General", Rate=1.0E-6M },
            new StandardFailure{Type="Trip Amp", Name="BD 120", Rate=8.74E-07M },
            new StandardFailure{Type="Trip Amp", Name="Moores STA", Rate=4.57E-07M },
            new StandardFailure{Type="Trip Amp", Name="PR Electronics TA", Rate=1E-06M },
            new StandardFailure{Type="Signal Conv", Name="Lee Dickens BD 300", Rate=8.79E-07M },
            new StandardFailure{Type="Pushbutton", Name="Fail to S/C", Rate=2.00E-07M },
            new StandardFailure{Type="Thermocouple", Name="Benign Env.", Rate=4.50E-09M },
            new StandardFailure{Type="Diode", Name="General", Rate=1.00E-09M },
            new StandardFailure{Type="LED", Name="General", Rate=5.00E-09M },
        };
    }

    public class GenericCommand<T> : ICommand
    {
        public Func<T, bool> CanExecuteProxy;
        public Action<T> ExecuteProxy;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            bool? result = true;
            if (parameter is T typedParam)
            {
                result = CanExecuteProxy?.Invoke(typedParam);
            }

            return result == true;
        }

        public void Execute(object parameter)
        {
            if (parameter is T typedParam)
            {
                if (CanExecuteProxy != null)
                {
                    ExecuteProxy?.Invoke(typedParam);
                }
            }
        }
    }
}
