using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
public static class JsonHelper
{
    public static string Encode(object DataObject)
    {
        try
        {
            var serializer = new DataContractJsonSerializer(DataObject.GetType());
            string result;
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, DataObject);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
        catch
        {
            return null;
        }

    }
    public static T Decode<T>(string DataString)
    {
        try
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            T deserialized;
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.Write(DataString);
                    sw.Flush();
                    ms.Position = 0;
                    deserialized = (T)serializer.ReadObject(ms);
                }
            }
            return deserialized;
        }
        catch
        {
            return default(T);
        }
    }
}