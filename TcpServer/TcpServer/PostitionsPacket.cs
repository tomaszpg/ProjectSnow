using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    public class PostitionsPacket
    {
        public PostitionsPacket(UserConnection sender)
        {
            this.client = sender;
            counter = 0;
        }

        private struct Position
        {
            // Structure holding single flake position and its number
            public Position(int num, float x, float y, float z)
            {
                number = num;
                xPos = x;
                yPos = y;
                zPos = z;
            }

            public int number;
            public float xPos;
            public float yPos;
            public float zPos;
        }

        // Maximum number of positions in one packet
        // !!! Larger packet - higher chance of splitting (and losing data as of now)
        // !!! Smaller packet - more communication initializations, longer transfer times
        private const int MAX_SIZE = 5000;

        private UserConnection client;
        private short counter;
        private Position[] posArray = new Position[MAX_SIZE];

        // Add position to the list, if the list is full, send the packet
        public void AddPosition(int numInit, float xInit, float yInit, float zInit)
        {
            posArray[counter] = new Position(numInit, xInit, yInit, zInit);
            counter++;
            if (counter >= MAX_SIZE)
                SendPacket();
        }

        // Send the packet to client, with the length based on current counter
        public void SendPacket()
        {
            if (counter != 0)
            {
                while (!client.readySend)
                {
                }
                int size = counter*16 + 3;
                byte[] data = new byte[size];
                byte[] positionCount = BitConverter.GetBytes(counter);
                data[0] = 55; // tag byte
                Array.Copy(positionCount, 0, data, 1, 2);
                for (int k = 0; k < counter; k++)
                {
                    byte[] numByte = BitConverter.GetBytes(posArray[k].number);
                    byte[] xByte = BitConverter.GetBytes(posArray[k].xPos);
                    byte[] yByte = BitConverter.GetBytes(posArray[k].yPos);
                    byte[] zByte = BitConverter.GetBytes(posArray[k].zPos);
                    Array.Copy(numByte, 0, data, k*16 + 3, 4);
                    Array.Copy(xByte, 0, data, k*16 + 7, 4);
                    Array.Copy(yByte, 0, data, k*16 + 11, 4);
                    Array.Copy(zByte, 0, data, k*16 + 15, 4);
                }
                client.SendPosPacket(data);
                counter = 0;
                //Thread.Sleep(1);
            }
        }
    }
}
