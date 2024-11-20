using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace _MessageType
{
    public enum MessageType
    {
        _NONE,
        POSITION,
        ACKNOWLEDGEMENTS,
        SHOOT,
        KILL,
        PAUSE,
        UNPAUSE,
        RESET,
        SETTINGS,

        _MESSAGE_TYPE_COUNT

    }
    public class Message 
    {
        public Message(MessageType type) => this.type = type;
        public uint id;
        public float time;
        public int playerID;
        public MessageType type;
    }

    public class Position : Message
    {
        public Position(Vector3 pos, float rot) : base(MessageType.POSITION)
        {
            this.pos = pos;
            this.rot = rot;
        }

        public Vector3 pos;
        public float rot;
    }
    public class Shoot : Message
    {
        public Shoot(int hitPlayerID) : base(MessageType.SHOOT)
        {
            this.hitPlayerID = hitPlayerID;
        }

        public int hitPlayerID;
    }

    public class Settings : Message
    {
        public Settings(string tankName, Color color) : base(MessageType.SETTINGS)
        {
            this.tankName = tankName;
            this.color = color;
        }

        public string tankName;
        public Color color;
    }


    public class Acknowledgements : Message
    {
        public Acknowledgements(List<uint> acks) : base(MessageType.ACKNOWLEDGEMENTS)
        {
            this.acks = acks;
        }

        public List<uint> acks;
    }


    public class Serializer
    {
        //From Data to Json string
        public static string ToJson(Message m)
        {
            return JsonUtility.ToJson(m);
        }

        //From Data to Bytes
        public static byte[] ToBytes(Message m)
        {
            return Encoding.ASCII.GetBytes(ToJson(m));
        }

        //From bytes to Data
        public static Message FromBytes(byte[] data, int size)
        {
            return FromJson(Encoding.ASCII.GetString(data, 0, size));
        }

        //From Json string to Data
        public static Message FromJson(string json)
        {
            Message m = JsonUtility.FromJson<Message>(json);

            //Check to reDeserialize in case of inherited class
            switch (m.type)
            {
                case MessageType.ACKNOWLEDGEMENTS:
                    {   
                        m = JsonUtility.FromJson<Acknowledgements>(json);
                        break;
                    }
                case MessageType.POSITION:
                    {
                        m = JsonUtility.FromJson<Position>(json);
                        break;
                    }
                case MessageType.SHOOT:
                    {
                        m = JsonUtility.FromJson<Shoot>(json);
                        break;
                    }
            }

            return m;
        }
    }

}
