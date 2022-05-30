using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TLM.EHC.Common.Historian
{
    public static class ObjectExtension
    {
        public static T CopyObject<T>(this object objSource)
        {
            using var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, objSource);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }

    }
}
