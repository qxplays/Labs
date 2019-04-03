using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;



namespace Domain.PublicDataContracts {
    public static class XmlSerializer<T> where T : class, new() {
        public static readonly ConcurrentDictionary<Type, XmlSerializer> Serializers =
            new ConcurrentDictionary<Type, XmlSerializer>();

        public static readonly XmlSerializer SerializerInstance = new XmlSerializer(typeof(T));
        static XmlSerializer() { }

        public static T Deserialize(byte[] bytes) {
            var str = Encoding.UTF8.GetString(bytes);
            using (var stream = new StringReader(str)) {
                using (var readeer = new XmlTextReader(stream)) {
                    return (T) SerializerInstance.Deserialize(readeer);
                }
            }
        }

        static XmlSerializer GetXmlSerializerFor(Type type) {
            if (Serializers.TryGetValue(type, out var serializer))
                return serializer;

            serializer = new XmlSerializer(type);
            Serializers.TryAdd(type, serializer);

            return serializer;
        }
        

        public static string SerializeObject(object item) {
            using (var stream = new MemoryStream()) {
                using (var writer = new XmlTextWriter(stream, Encoding.UTF8) {Formatting = Formatting.None}) {
                    GetXmlSerializerFor(item.GetType()).Serialize(writer, item);

                    writer.Flush();

                    stream.Position = 0;
                    using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                        var builder = new StringBuilder((int) stream.Length);

                        while (!reader.EndOfStream) {
                            var chr = (char) reader.Read();

                            builder.Append(chr);
                        }

                        return builder.ToString();
                    }
                }
            }

        }
    }
}