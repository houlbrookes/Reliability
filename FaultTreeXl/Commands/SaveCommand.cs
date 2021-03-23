using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    // Save the data under the current filename
    public class SaveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                if (parameter is FaultTreeModel faultTreeModel)
                {
                    var fileName = faultTreeModel.Filename;
                    if (!string.IsNullOrEmpty(fileName) && fileName != FaultTreeModel.NO_FILENAME)
                    {
                        using (StreamWriter streamWriter = new StreamWriter(path: fileName, append: false))
                        {
                            XmlSerializer x = new XmlSerializer(faultTreeModel.GetType());
                            x.Serialize(streamWriter, faultTreeModel);
                        }
                        faultTreeModel.Status = $"Fault Tree saved to file: {fileName}";
                        faultTreeModel.Dirty = false;
                        MessageBox.Show($"Saved to {fileName}", "Saved File");
                    }
                    else
                    {
                        var response = MessageBox.Show("Filename is blank, do you want to SaveAs?", "Saving", MessageBoxButton.YesNo);
                        if (response == MessageBoxResult.Yes)
                        {
                            ICommand saveAsCommand = new SaveAsCommand();
                            saveAsCommand.Execute(faultTreeModel);
                            faultTreeModel.Dirty = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }

}
