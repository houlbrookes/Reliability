using System.Xml.Serialization;

namespace FaultTreeXl
{
    public class StandardFailure
    {
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public decimal Rate { get; set; }
    }
}