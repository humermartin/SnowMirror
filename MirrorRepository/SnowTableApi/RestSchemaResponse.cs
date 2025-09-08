using System;
using System.Xml.Serialization;

namespace MirrorRepository.SnowTableApi
{
    /// <summary>
    /// class RestSchemaResponse
    /// </summary>
    [Serializable]
    public class RestSchemaResponse 
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("internal_type")]
        public string InternalType { get; set; }

        [XmlAttribute("max_length")]
        public int? MaxLength { get; set; }

        [XmlAttribute("choice_list")]
        public bool? ChoiceList { get; set; }

        [XmlAttribute("display_field")]
        public string DisplayField { get; set; }

        [XmlAttribute("reference_table")]
        public string ReferenceTable { get; set; }

        [XmlAttribute("reference_field_max_length")]
        public int? ReferenceFieldMaxLength { get; set; }

    }

}
