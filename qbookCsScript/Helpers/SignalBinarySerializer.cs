using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB.Helpers
{
    public struct MeasurementBinary
    {
        public string Name;
        public string Text;
        public string Unit;
        public double Value;
        public double Epoch;
        public DateTime Timestamp;
    }

    public static class SignalBinarySerializer
    {
        public static byte[] SerializeMeasurements(List<MeasurementBinary> measurements)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(measurements.Count); // Anzahl der Signale speichern

                foreach (var measurement in measurements)
                {
                    WriteString(writer, measurement.Name);
                    WriteString(writer, measurement.Text);
                    WriteString(writer, measurement.Unit);
                    writer.Write(measurement.Value);
                    writer.Write(measurement.Epoch);
                    writer.Write(measurement.Timestamp.ToBinary());
                }
                return ms.ToArray();
            }
        }

        public static List<MeasurementBinary> DeserializeMeasurements(byte[] data)
        {
            List<MeasurementBinary> measurements = new List<MeasurementBinary>();
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                int count = reader.ReadInt32(); // Anzahl der Signale

                for (int i = 0; i < count; i++)
                {
                    string name = ReadString(reader);
                    string text = ReadString(reader);
                    string unit = ReadString(reader);
                    double value = reader.ReadDouble();
                    double epoch = reader.ReadDouble();
                    DateTime timestamp = DateTime.FromBinary(reader.ReadInt64());

                    measurements.Add(new MeasurementBinary
                    {
                        Name = name,
                        Text = text,
                        Unit = unit,
                        Value = value,
                        Epoch = epoch,
                        Timestamp = timestamp
                    });
                }
            }
            return measurements;
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                writer.Write(0);
            }
            else
            {
                byte[] stringBytes = Encoding.UTF8.GetBytes(value);
                writer.Write(stringBytes.Length);
                writer.Write(stringBytes);
            }
        }

        private static string ReadString(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return string.Empty;
            return Encoding.UTF8.GetString(reader.ReadBytes(length));
        }
    }
}
