﻿namespace NurseCallMvcs.Models
{
    using System;
    using System.Text;
    using System.Linq;


    public class Packet
    {
        #region Messge Types (Static)
        public static int Send = BitConverter.ToInt32(Encoding.ASCII.GetBytes("SEND"), 0);
        #endregion

        // The Fields for the packet
        public int PacketType { get; set; }
        public int RollingMsgID { get; set; }
        public int MessageLength { get; set; }
        public byte[] Payload { get; set; } = new byte[0];


        #region Constructors
        // Creates a Packet from a byte array
        public Packet(byte[] bytes)
        {
            string str = Encoding.Default.GetString(bytes);
            Console.WriteLine(str);
            RollingMsgID = BitConverter.ToInt32(bytes.Skip(1).Take(4).ToArray(), 0);
            PacketType = ConvertToUInt(bytes, 2, 5, 2);
            MessageLength = ConvertToUInt(bytes, 2, 7, 2);
            Payload = GetPayLoad(bytes.Skip(9).ToArray(), MessageLength);
        }

        private byte[] GetPayLoad(byte[] bytes, int Length)
        {
            var data = bytes;
            var new_data = data.TakeWhile((v, index) => data.Skip(index).Any(w => w != 0x00)).ToArray();
            var ret_bytes = bytes.Take(Length).ToArray();
            return ret_bytes;
        }

        private int ConvertToUInt(byte[] bytes, int noofbytes = 2, int skip = 5, int take = 2)
        {
            try
            {
                var type = bytes.Skip(skip).Take(take).ToArray();
                if (BitConverter.IsLittleEndian)
                {
                    byte[] tempByteArray = new byte[noofbytes];
                    for (int i = 0; i < noofbytes; i++)
                        tempByteArray[i] = type[i];
                    return BitConverter.ToUInt16(tempByteArray, 0);
                }
                return BitConverter.ToUInt16(type, 4);
            }
            catch
            {
                return -1;
            }
        }
        #endregion // Constructors

        public override string ToString()
        {
            // Take some of the first few bits of data and turn that into a string
            String payloadStr;
            int payloadSize = Payload.Length;
            if (payloadSize > 8)
                payloadStr = Encoding.ASCII.GetString(Payload, 0, 8) + "...";
            else
                payloadStr = Encoding.ASCII.GetString(Payload, 0, payloadSize);

            // type string
            String typeStr = "UKNOWN";

            return string.Format(
                "[Packet:\n" +
                "  Type={0},\n" +
                "  PayloadSize={1},\n" +
                "  Payload=`{2}`]",
                typeStr, payloadSize, payloadStr);
        }


        // Gets the Packet as a byte array
        public byte[] GetBytes()
        {
            // Join the byte arrays
            byte[] bytes = new byte[4 + Payload.Length];
            return bytes;
        }
    }

    public class Message
    {
        public string callType;
        public string roomNumber;
        public bool isRoom;

        public Message(string callType, string roomNumber, bool isRoom)
        {
            this.callType = callType;
            this.roomNumber = roomNumber;
            this.isRoom = isRoom;
        }
    }
    public static class PacketHelper{
        
        public static Message GetMessage(Packet p)
        {
            try
            {
                var splitStr = '#';
                string str = Encoding.ASCII.GetString(p.Payload, 0, p.MessageLength);
                var split_Str = str.Replace("\u0001",splitStr.ToString()).Split(splitStr);
                var msg = split_Str.LastOrDefault()
                    .Trim().Replace("\0", " ").Split(' ')
                    .Where(s => !string.IsNullOrEmpty(s)).ToArray();
                var callType = msg.LastOrDefault()?.ToUpper();
                var roomNumber = msg.FirstOrDefault(x => int.TryParse(x, out int s));
                var isRoom =!msg.Any(x =>  x.ToUpper().Contains("TOILET"));
                return new Message(callType, roomNumber, isRoom);
            }
            catch
            {
                return null;
            }
        }

}
}
