using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    public class NotifyPropertyChangedItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _dirty = false;
        [XmlIgnore]
        public bool Dirty 
        { 
            get => _dirty;
            set
            {
                if (_dirty != value)
                {
                    _dirty = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Dirty)));
                }
            }
        }

        internal T Changed<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null, bool updateDirty=true)
        {
            property = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (updateDirty)
                Dirty = true;
            return property;
        }

        internal void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
