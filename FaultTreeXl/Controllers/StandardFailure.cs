using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Linq;

namespace FaultTreeXl
{
    public class StandardFailure : NotifyPropertyChangedItem
    {
        public StandardFailure CopyFrom(StandardFailure item2Copy)
        {
            Name = item2Copy.Name;
            Rate = item2Copy.Rate;
            TotalRate = item2Copy.TotalRate;
            Type = item2Copy.Type;
            IsA = item2Copy.IsA;
            Source = item2Copy.Source;
            FailureModes = item2Copy.FailureModes;
            return this;
        }

        private string _type;
        [XmlAttribute]
        public string Type
        {
            get => _type;
            set => Changed(ref _type, value, nameof(Type));
        }

        private string _name;
        [XmlAttribute]
        public string Name
        {
            get => _name;
            set => Changed(ref _name, value, nameof(Name));
        }

        private decimal _rate;
        [XmlAttribute]
        public decimal Rate
        {
            get => _rate;
            set
            {
                Changed(ref _rate, value, nameof(Rate));
                Notify(nameof(SFF));
            }
        }

        private decimal _totalRate;
        [XmlAttribute]
        public decimal TotalRate
        {
            get => _totalRate == 0 ? _rate : _totalRate;
            set
            {
                Changed(ref _totalRate, value, nameof(TotalRate));
                Notify(nameof(SFF));
            }
        }

        private bool _isA;
        [XmlAttribute]
        public bool IsA
        {
            get => _isA;
            set => Changed(ref _isA, value, nameof(IsA));
        }

        private string _prefix = "NodeP";
        [XmlAttribute]
        public string Prefix
        {
            get => _prefix;
            set => Changed(ref _prefix, value, nameof(Prefix));
        }

        private string _source = "Not Referenced";
        [XmlAttribute]
        public string Source
        {
            get => _source;
            set => Changed(ref _source, value, nameof(Source));
        }

        public class FailureMode : NotifyPropertyChangedItem
        {
            [XmlIgnore]
            public Array ModeValues { get => Enum.GetValues(typeof(EnumMode)); }

            private string _name;
            [XmlAttribute]
            public string Name
            {
                get => _name;
                set => Changed(ref _name, value, nameof(Name));
            }

            private double _proportion;
            [XmlAttribute]
            public double Proportion
            {
                get => _proportion;
                set
                {
                    Changed(ref _proportion, value, nameof(Proportion));
                }
            }

            public enum EnumMode { DU, DD, S, NA }
            private EnumMode mode = EnumMode.DU;
            [XmlAttribute]
            public EnumMode Mode
            {
                get => mode;
                set => Changed(ref mode, value, nameof(Mode));
            }

        }

        private ObservableCollection<FailureMode> _failureModes = new ObservableCollection<FailureMode>();
        [XmlArray]
        public ObservableCollection<FailureMode> FailureModes
        {
            get => _failureModes;
            set
            {
                if (_failureModes != null)
                { // Remove events
                    _failureModes.CollectionChanged -= _failureModes_CollectionChanged;
                    foreach (var mode in _failureModes) mode.PropertyChanged -= Fm_PropertyChanged;
                }
                Changed(ref _failureModes, value, nameof(FailureModes));
                if (_failureModes != null)
                {// Add events
                    foreach (var mode in _failureModes) mode.PropertyChanged += Fm_PropertyChanged;
                    _failureModes.CollectionChanged += _failureModes_CollectionChanged;
                }
            }
        }

        private void _failureModes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    if (item is FailureMode fm)
                    {
                        fm.PropertyChanged += Fm_PropertyChanged;
                        Notify(nameof(SumOfModes));
                    }
            if (e.Action==System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                    if (item is FailureMode fm)
                    {
                        fm.PropertyChanged -= Fm_PropertyChanged;
                        Notify(nameof(SumOfModes));
                    }
            }
        }

        private void Fm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Notify(nameof(SumOfModes));
        }

        public double SumOfModes { get => FailureModes.Sum(mode => mode.Proportion); }
        public double SFF { get => 1D - (double)(Rate / TotalRate); }
        public decimal SafeRate { get => TotalRate - Rate; }
    }
}