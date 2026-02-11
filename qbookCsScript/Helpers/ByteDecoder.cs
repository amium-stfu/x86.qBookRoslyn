using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB
{
    public class ByteDecoder
    {
        public ByteDecoder()
        {
        }

        public ByteDecoder(string offset, string bits, string byteOrder, bool signed)
            : this(offset, bits, byteOrder, signed, 1, 0, "")
        {
        }

        public ByteDecoder(string msgOffset, string bits, string byteOrder, bool signed, double gain, double offset, string unit)
        {
            try
            {
                if (msgOffset != "")
                    MsgOffset = uint.Parse(msgOffset);
                if (bits != "")
                    MsgBits = uint.Parse(bits);

                BigEndian = false;
                if (byteOrder.Contains("B")) BigEndian = true;
                if (byteOrder.Contains("M")) BigEndian = true;

                Signed = signed;
                Gain = gain;
                Offset = offset;
            }
            catch (Exception ex)
            {
                //    log.Fatal("%alertEvaluate% " + ex.Message);
            }
        }

        public override string ToString()
        {
            return MsgOffset + "/" + MsgBits + "" + (BigEndian ? "M/BE" : "I/LE") + " [GA:" + Gain + "/OF:" + Offset + "]" + (Ieee ? "Ieee" : "");
            //            return Name + " " + MsgId + " " + MsgOffset + " " + MsgBits + " " + MuxOffset + "/" + MuxBits + "/" + MuxValue;
        }


        public uint MsgOffset = uint.MaxValue;
        public uint MsgBits = uint.MaxValue;
        public bool BigEndian = false;
        public bool Signed = false;
        public double Gain = 1;
        public bool Ieee = false;
        public double Offset = 0;

        public ByteDecoder Mux = null;
        public int MuxValue = -1;

        public void ValueDoubleToBytes(ref byte[] bytes, double value)
        {
            if (!double.IsNaN(Offset)) value -= Offset;
            if (!double.IsNaN(Gain)) value /= Gain;
            UInt64 data = (UInt64)value;// &0xffff;
            if ((MsgBits == 32) && Ieee)
            {
                byte[] byteIeee = BitConverter.GetBytes(Convert.ToSingle(value));
                /*
                data = ((((UInt64)byteIeee[0]) << 24) |
                    (((UInt64)byteIeee[1]) << 16) |
                    (((UInt64)byteIeee[2]) << 8) |
                    (((UInt64)byteIeee[3]) << 0));
                */
                data = ((((UInt64)byteIeee[0]) << 0) |
                    (((UInt64)byteIeee[1]) << 8) |
                    (((UInt64)byteIeee[2]) << 16) |
                    (((UInt64)byteIeee[3]) << 24));
            }
            else if ((MsgBits == 16) && !Signed)
            {
                if (value < 0)
                {
                    value = 0;
                }
                data = (UInt64)value;// &0xffff;
            }
            UInt64 mask = 0;
            if (MsgBits == 1) mask = 0x00000001L;
            if (MsgBits == 2) mask = 0x00000003L;
            if (MsgBits == 3) mask = 0x00000007L;
            if (MsgBits == 4) mask = 0x0000000fL;
            if (MsgBits == 5) mask = 0x0000001fL;
            if (MsgBits == 6) mask = 0x0000003fL;
            if (MsgBits == 7) mask = 0x0000007fL;
            if (MsgBits == 8) mask = 0x000000ffL;
            if (MsgBits == 9) mask = 0x000001ffL;
            if (MsgBits == 10) mask = 0x000003ffL;
            if (MsgBits == 11) mask = 0x000007ffL;
            if (MsgBits == 12) mask = 0x00000fffL;
            if (MsgBits == 13) mask = 0x00001fffL;
            if (MsgBits == 14) mask = 0x00003fffL;
            if (MsgBits == 15) mask = 0x00007fffL;
            if (MsgBits == 16) mask = 0x0000ffffL;
            if (MsgBits == 32) mask = 0xffffffffL;
            data = (UInt64)(data & mask);
            data = data << (int)MsgOffset;
            mask = mask << (int)MsgOffset;


            /*
            UInt64 data2 = 0;
            if (!BigEndian) // eg canopen, intel
            {
                for (int i = (bytes.Length - 1); i >= 0; i--)
                {
                    data2 <<= 8;
                    data2 |= bytes[i];
                }
                data2 >>= (int)MsgOffset;

            }
            else // forward motorola
            {

                for (int i = 0; i < bytes.Length; i++)
                //for (int i = 0; i < 6; i++)
                {
                    data2 <<= 8;
                    data2 |= bytes[i];
                }

                uint off2 = (7 - (MsgOffset % 8)) + (MsgOffset / 8) * 8;

                data2 >>= (int)((8 * bytes.Length) - off2 - MsgBits); // TOX diese function testen
                // alternativ für mv panel
                //              data >>= (int)(off2); // TOX diese function testen

            }
            */



            if (!BigEndian) // eg canopen, intel
            {
                UInt64 pre =
                    ((((UInt64)bytes[0]) << 0) |
                    (((UInt64)bytes[1]) << 8) |
                    (((UInt64)bytes[2]) << 16) |
                    (((UInt64)bytes[3]) << 24) |
                    (((UInt64)bytes[4]) << 32) |
                    (((UInt64)bytes[5]) << 40) |
                    (((UInt64)bytes[6]) << 48) |
                    (((UInt64)bytes[7]) << 56));

                pre &= ~mask;
                pre |= data;
                data = pre;

                bytes[0] = (byte)((data >> 0) & 0xff);
                bytes[1] = (byte)((data >> 8) & 0xff);
                bytes[2] = (byte)((data >> 16) & 0xff);
                bytes[3] = (byte)((data >> 24) & 0xff);
                bytes[4] = (byte)((data >> 32) & 0xff);
                bytes[5] = (byte)((data >> 40) & 0xff);
                bytes[6] = (byte)((data >> 48) & 0xff);
                bytes[7] = (byte)((data >> 56) & 0xff);
            }
            else // forward motorola
            {
                UInt64 pre =
                   ((((UInt64)bytes[0]) << 0) |
                   (((UInt64)bytes[1]) << 8) |
                   (((UInt64)bytes[2]) << 16) |
                   (((UInt64)bytes[3]) << 24) |
                   (((UInt64)bytes[4]) << 32) |
                   (((UInt64)bytes[5]) << 40) |
                   (((UInt64)bytes[6]) << 48) |
                   (((UInt64)bytes[7]) << 56));

                pre &= ~mask;
                pre |= data;
                data = pre;

                bytes[0] = (byte)((data >> 0) & 0xff);
                bytes[1] = (byte)((data >> 8) & 0xff);
                bytes[2] = (byte)((data >> 16) & 0xff);
                bytes[3] = (byte)((data >> 24) & 0xff);
                bytes[4] = (byte)((data >> 32) & 0xff);
                bytes[5] = (byte)((data >> 40) & 0xff);
                bytes[6] = (byte)((data >> 48) & 0xff);
                bytes[7] = (byte)((data >> 56) & 0xff);
                /*
                UInt64 pre =
    ((((UInt64)bytes[7]) << 0) |
    (((UInt64)bytes[6]) << 8) |
    (((UInt64)bytes[5]) << 16) |
    (((UInt64)bytes[4]) << 24) |
    (((UInt64)bytes[3]) << 32) |
    (((UInt64)bytes[2]) << 40) |
    (((UInt64)bytes[1]) << 48) |
    (((UInt64)bytes[0]) << 56));

                pre &= ~mask;
                pre |= data;
                data = pre;
                bytes[7] = (byte)((data >> 0) & 0xff);
                bytes[6] = (byte)((data >> 8) & 0xff);
                bytes[5] = (byte)((data >> 16) & 0xff);
                bytes[4] = (byte)((data >> 24) & 0xff);
                bytes[3] = (byte)((data >> 32) & 0xff);
                bytes[2] = (byte)((data >> 40) & 0xff);
                bytes[1] = (byte)((data >> 48) & 0xff);
                bytes[0] = (byte)((data >> 56) & 0xff);
            */
            }
        }


        public double ValueDoubleFromBytes(byte[] bytes)
        {
            uint iValue = uint.MaxValue;
            double value;

            //     Ieee = true;

            if (Ieee)
            {
                value = GetIeee754(bytes, (int)MsgOffset, BigEndian);
            }
            else
            {
                try
                {
                    iValue = GetInt(bytes, MsgOffset, MsgBits, BigEndian);
                    if (iValue == uint.MaxValue) return double.NaN;

                    //   if (MsgBits == 16)
                    //       value = (UInt16)

                    if ((Mux != null) && (MuxValue >= 0) && (MuxValue < 64) && (Mux.MsgBits > 0))
                    {
                        //                    uint muxValue = GetInt(bytes, MuxOffset, MuxBits, false) & MuxMask;
                        uint muxValue = GetInt(bytes, Mux.MsgOffset, Mux.MsgBits, BigEndian);
                        if (muxValue != MuxValue) return double.NaN;
                    }
                    //                if ((MsgBits == 16) && (value > 32768)) value = value -65536;

                }
                catch (Exception ex)
                {
                    //  log.Fatal("EX_Bitmask ValueDoubleFromBytes " + ex.ToString());
                }
                if (iValue == uint.MaxValue) return double.NaN;
                value = (double)iValue;
                if (MsgBits == 16)
                {
                    if (Signed) value = (Int16)iValue;
                    else value = (UInt16)iValue; // bie gmp nox fatal -> da keine neg Werte !!!
                    //if (MsgBits == 16) 
                }
                if (MsgBits == 32)
                {
                    if (Signed) value = (Int32)iValue;
                    else value = (UInt32)iValue; // bie gmp nox fatal -> da keine neg Werte !!!
                    //if (MsgBits == 16) 
                }
            }

            if (!double.IsNaN(Gain)) value *= Gain;
            if (!double.IsNaN(Offset)) value += Offset;

            return Math.Round(value, 8);
        }

        public uint ValueInt(byte[] bytes)
        {
            uint value = GetInt(bytes, MsgOffset, MsgBits, BigEndian);

            if (value == uint.MaxValue) return value;

            if ((Mux != null) && (Mux.MsgBits > 0) && (Mux.MsgOffset >= 0) && (Mux.MsgOffset < 64))
            {
                uint muxValue = GetInt(bytes, Mux.MsgOffset, Mux.MsgBits, false);// &MuxMask;
                if (muxValue != MuxValue) return uint.MaxValue;
            }
            if ((MsgBits == 16) && (value > 32768)) value = value - 65536;

            return value;
        }

        public static uint GetInt(byte[] bytes, uint offset, uint bits, bool bigEndian)
        {
            if ((offset < 64) && ((offset % 8) == 0))
            {
                if (bits == 8)
                    return GetInt8(bytes, offset);
                if (bits == 16)
                    return GetInt16(bytes, offset, bigEndian);
                if (bits == 32)
                    return GetInt32(bytes, offset, bigEndian);
                if (bits == 64)
                    return GetInt64(bytes, offset, bigEndian);
            }

            // if bits != 8,16, oder 32

            UInt64 data = 0;
            if (!bigEndian) // eg canopen, intel
            {
                for (int i = (bytes.Length - 1); i >= 0; i--)
                {
                    data <<= 8;
                    data |= bytes[i];
                }
                data >>= (int)offset;

            }
            else // forward motorola
            {

                for (int i = 0; i < bytes.Length; i++)
                //for (int i = 0; i < 6; i++)
                {
                    data <<= 8;
                    data |= bytes[i];
                }

                uint off2 = (7 - (offset % 8)) + (offset / 8) * 8;

                data >>= (int)((8 * bytes.Length) - off2 - bits); // TOX diese function testen
                // alternativ für mv panel
                //              data >>= (int)(off2); // TOX diese function testen

            }

            if (bits == 1) return (uint)(data & 0x00000001L);
            if (bits == 2) return (uint)(data & 0x00000003L);
            if (bits == 3) return (uint)(data & 0x00000007L);
            if (bits == 4) return (uint)(data & 0x0000000fL);
            if (bits == 5) return (uint)(data & 0x0000001fL);
            if (bits == 6) return (uint)(data & 0x0000003fL);
            if (bits == 7) return (uint)(data & 0x0000007fL);
            if (bits == 8) return (uint)(data & 0x000000ffL);
            if (bits == 9) return (uint)(data & 0x000001ffL);
            if (bits == 10) return (uint)(data & 0x000003ffL);
            if (bits == 11) return (uint)(data & 0x000007ffL);
            if (bits == 12) return (uint)(data & 0x00000fffL);
            if (bits == 13) return (uint)(data & 0x00001fffL);
            if (bits == 14) return (uint)(data & 0x00003fffL);
            if (bits == 15) return (uint)(data & 0x00007fffL);
            if (bits == 16) return (uint)(data & 0x0000ffffL);
            if (bits == 17) return (uint)(data & 0x0001ffffL);
            if (bits == 18) return (uint)(data & 0x0003ffffL);
            if (bits == 19) return (uint)(data & 0x0007ffffL);
            if (bits == 20) return (uint)(data & 0x000fffffL);
            if (bits == 21) return (uint)(data & 0x001fffffL);
            if (bits == 22) return (uint)(data & 0x003fffffL);
            if (bits == 23) return (uint)(data & 0x007fffffL);
            if (bits == 24) return (uint)(data & 0x00ffffffL);
            if (bits == 25) return (uint)(data & 0x01ffffffL);
            if (bits == 26) return (uint)(data & 0x03ffffffL);
            if (bits == 27) return (uint)(data & 0x07ffffffL);
            if (bits == 28) return (uint)(data & 0x0fffffffL);
            if (bits == 29) return (uint)(data & 0x1fffffffL);
            if (bits == 30) return (uint)(data & 0x3fffffffL);
            if (bits == 31) return (uint)(data & 0x7fffffffL);
            if (bits == 32) return (uint)(data & 0xffffffffL);

            return uint.MaxValue;
        }

        public static uint GetInt8(byte[] bytes, uint offset)
        {
            byte[] bytes_ = new byte[1];
            Array.Copy(bytes, offset / 8, bytes_, 0, 1);

            UInt32 value = 0;
            int byteCount = 1;
            for (int i = byteCount - 1; i >= 0; i--)
                value = (value << 8) + bytes_[i];
            return (uint)value;
        }

        public static uint GetInt16(byte[] bytes, uint offset, bool bigEndian)
        {
            byte[] bytes_ = new byte[2];
            Array.Copy(bytes, offset / 8, bytes_, 0, 2);

            UInt32 value = 0;
            int byteCount = 2;
            if (bigEndian) // forwards motorola, sent
            {
                for (int i = 0; i < byteCount; i++)
                    value = (value << 8) + bytes_[i];
            }
            else // intel, canopen
            {
                for (int i = byteCount - 1; i >= 0; i--)
                    value = (value << 8) + bytes_[i];
            }
            return (uint)value;
        }

        public static uint GetInt32(byte[] bytes, uint offset, bool bigEndian)
        {
            byte[] bytes_ = new byte[4];
            Array.Copy(bytes, offset / 8, bytes_, 0, 4);

            UInt32 value = 0;
            int byteCount = 4;

            //   bigEndian = true;

            if (bigEndian) // forwards motorola, sent
            {
                for (int i = 0; i < byteCount; i++)
                    value = (value << 8) + bytes_[i];
            }
            else // intel, canopen
            {
                for (int i = byteCount - 1; i >= 0; i--)
                    value = (value << 8) + bytes_[i];
            }
            return (uint)value;
        }

        public static uint GetInt64(byte[] bytes, uint offset, bool bigEndian)
        {
            byte[] bytes_ = new byte[8];
            Array.Copy(bytes, offset / 8, bytes_, 0, 8);
            if (bigEndian) bytes_.Reverse();

            UInt32 value = 0;
            int byteCount = 8;

            if (bigEndian) // forwards motorola, sent
            {
                for (int i = 0; i < byteCount; i++)
                    value = (value << 8) + bytes_[i];
            }
            else // intel, canopen
            {
                for (int i = byteCount - 1; i >= 0; i--)
                    value = (value << 8) + bytes_[i];
            }
            return (uint)value;
        }

        public static double Ieee754ToLong(byte[] ieee)
        {
            bool sign = (ieee[0] & 0x80) == 0x80;
            ieee[0] &= 0x7f;
            int exponent = (int)(((uint)ieee[0] << 1) + (ieee[1] >> 7));
            //data[1] &= 0x7f;
            long mantisse = ((long)ieee[1]) << 16;
            mantisse |= ((long)ieee[2]) << 8;
            mantisse |= (long)ieee[3];
            double sum = 0;
            double adder = 0.5;
            for (int i = 0; i < 23; i++)
            {
                if ((mantisse & 0x400000) == 0x400000)
                {
                    sum += adder;
                }
                adder /= 2;
                mantisse *= 2;
            }
            sum += 1;
            sum *= Math.Pow(2, exponent - 127);
            if (sign) sum = -sum;
            return sum;
        }

        public static double GetIeee754(byte[] bytes, int offset, bool bigEndian)
        {
            byte[] bytes_ = new byte[4];
            Array.Copy(bytes, offset / 8, bytes_, 0, 4);
            byte[] bytes2 = new byte[4];

            bytes2[0] = bytes_[3];
            bytes2[1] = bytes_[2];
            bytes2[2] = bytes_[1];
            bytes2[3] = bytes_[0];

            if (bigEndian)
                return Ieee754ToLong(bytes_);
            else
                return Ieee754ToLong(bytes2);
        }
    }
}
