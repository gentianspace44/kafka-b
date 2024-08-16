using System.Xml;
using System.Xml.Serialization;

namespace VPS.Helpers
{
    public static class XmlConvert
    {
        public static T Deserialize<T>(string xmlString)
        {
            XmlSerializer xmlSerialize = new(typeof(T));

            using TextReader reader = new StringReader(xmlString);
            return (T)xmlSerialize.Deserialize(reader);
        }

        public static string? Serialize<T>(T obj)
        {
            var serializeRequest = new XmlSerializer(typeof(T));

            using var stringWriter = new StringWriter();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };

            using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings);
            serializeRequest.Serialize(xmlWriter, obj);

            return stringWriter.ToString();
        }
    }
}
