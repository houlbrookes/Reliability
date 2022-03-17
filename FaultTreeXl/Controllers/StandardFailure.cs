using System.Xml.Serialization;

namespace FaultTreeXl
{
    public class StandardFailure: NotifyPropertyChangedItem
    {
        public StandardFailure CopyFrom(StandardFailure item2Copy)
        {
            Name = item2Copy.Name;
            Rate = item2Copy.Rate;
            TotalRate = item2Copy.TotalRate;
            Type = item2Copy.Type;
            IsA = item2Copy.IsA;
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

        public double SFF { get => 1D-(double)(Rate / TotalRate); }
        public decimal SafeRate { get => TotalRate - Rate; }
    }
}