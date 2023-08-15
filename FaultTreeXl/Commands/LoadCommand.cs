using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    /// <summary>
    /// This command loads the fault tree from an XML File
    /// </summary>
    public class LoadCommand : ICommand
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

        public void LoadFromFile(string fileName, FaultTreeModel mc)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(FaultTreeModel));
                FaultTreeModel data = serializer.Deserialize(streamReader) as FaultTreeModel;
                mc.Filename = fileName;
                mc.RootNode = data.RootNode;
                mc.Version = data.Version;
                mc.Notes = data.Notes;
                mc.ReDrawRootNode();
                mc.ShowingCutsets = false;
                mc.Dirty = false;
                mc.Notify(nameof(mc.SILLevelPFD));
                mc.Notify(nameof(mc.SILLevelPFH));
                mc.Status = $"Fault Tree loaded from file: {fileName}";
            }
        }

        public void Execute(object parameter)
        {
            string fileName = GetFileName();

            if (!string.IsNullOrEmpty(fileName))
            {
                if (parameter is FaultTreeModel mc)
                {
                    LoadFromFile(fileName, mc);
                }
            }

        }

        private string GetFileName()
        {
            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                InitialDirectory = desktopFolder,
                DefaultExt = ".xml",
                Filter = "Fault tree documents (.fta)|*.fta",
                CheckFileExists = true,

            };
            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileName;
            }
            return null;
        }

    }

}
