using Microsoft.Win32;
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
    public class SaveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                string fileName = "";
                if (parameter is FaultTreeModel mc2)
                {
                    fileName = mc2.Filename;
                }
                fileName = GetFileName(fileName);
                if (!string.IsNullOrEmpty(fileName))
                {
                    if (parameter is FaultTreeModel mc)
                    {
                        using (StreamWriter sw = new StreamWriter(path: fileName, append: false))
                        {
                            XmlSerializer x = new XmlSerializer(mc.GetType());
                            x.Serialize(sw, mc);
                        }
                        mc.Status = $"Fault Tree saved to file: {fileName}";
                        mc.Filename = fileName;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private string GetFileName(string currentFilename)
        {
            string initialFolder = "";
            string stripFilename = "";
            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            try
            {
                initialFolder = Path.GetDirectoryName(currentFilename);
                stripFilename = Path.GetFileName(currentFilename);
            }
            catch
            {
                initialFolder = desktopFolder;
                stripFilename = "new Fault Tree.fta";
            }
            SaveFileDialog fileDialog = new SaveFileDialog()
            {
                InitialDirectory = initialFolder,
                DefaultExt = ".fta",
                Filter = "Fault Tree documents (.fta)|*.fta",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = stripFilename,
            };
            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileName;
            }
            return null;
        }
    }
}
