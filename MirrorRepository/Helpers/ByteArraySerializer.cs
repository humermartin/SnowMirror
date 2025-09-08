using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MirrorRepository.Helpers
{
    /// <summary>
    /// class byte array serializer
    /// </summary>
    public static class ByteArraySerializer
    {
        /// <summary>
        /// serialize object to byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(this T m)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, m);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// deserialize byte array to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return (T)new BinaryFormatter().Deserialize(ms);
            }
        }
    }
}
