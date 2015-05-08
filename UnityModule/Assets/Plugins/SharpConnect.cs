// This is the client DLL class code to use for the sockServer
// include this DLL in your Plugins folder under Assets
// using it is very simple
// Look at LinkSyncSCR.cs


using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace SharpConnect
{
    public class Connector
    {
        const int READ_BUFFER_SIZE = 2000 * 16;
        const int PORT_NUM = 10000;
        private TcpClient client;
        private byte[] readBuffer = new byte[READ_BUFFER_SIZE];
        public ArrayList lstUsers = new ArrayList();
        public string strMessage = string.Empty;
        public string res = string.Empty;
        public int counter = 0;

        public Connector() { }

        public string FnConnectResult(string sNetIP, int iPORT_NUM, string sUserName)
        {
            try
            {
                // The TcpClient is a subclass of Socket, providing higher level 
                // functionality like streaming.
                client = new TcpClient(sNetIP, PORT_NUM);
                // Start an asynchronous read invoking DoRead to avoid lagging the user
                // interface.
                client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
                // Make sure the window is showing before popping up connection dialog.

                AttemptLogin(sUserName);
                return "Connection Succeeded";
            }
            catch (Exception ex)
            {
                return "Server is not active.  Please start server and try again.      " + ex.ToString();
            }
        }
        public void AttemptLogin(string user)
        {
            byte[] userNameByte = Encoding.ASCII.GetBytes(user);
            byte[] data = new byte[userNameByte.Length + 1];
            data[0] = 1; // tag byte
            Array.Copy(userNameByte, 0, data, 1, userNameByte.Length);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public void SendPlayerPos(Vector3 pPosition)
        {
            byte[] data = new byte[9];
            data[0] = 2; // tag byte
            byte[] xByte = BitConverter.GetBytes(pPosition.x);
            byte[] zByte = BitConverter.GetBytes(pPosition.z);
            Array.Copy(xByte, 0, data, 1, 4);
            Array.Copy(zByte, 0, data, 5, 4);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public void SendProperties(int num, float size, float wStr, float wStrFluc, float wDir, float wDirFluc, float radius, float noise)
        {
            byte[] data = new byte[33];
            data[0] = 8; // tag byte
            byte[] numByte = BitConverter.GetBytes(num);
            byte[] sizeByte = BitConverter.GetBytes(size);
            byte[] wStrByte = BitConverter.GetBytes(wStr);
            byte[] wStrFlucByte = BitConverter.GetBytes(wStrFluc);
            byte[] wDirByte = BitConverter.GetBytes(wDir);
            byte[] wDirFlucByte = BitConverter.GetBytes(wDirFluc);
            byte[] radiusByte = BitConverter.GetBytes(radius);
            byte[] noiseByte = BitConverter.GetBytes(noise);
            Array.Copy(numByte, 0, data, 1, 4);
            Array.Copy(sizeByte, 0, data, 5, 4);
            Array.Copy(wStrByte, 0, data, 9, 4);
            Array.Copy(wStrFlucByte, 0, data, 13, 4);
            Array.Copy(wDirByte, 0, data, 17, 4);
            Array.Copy(wDirFlucByte, 0, data, 21, 4);
            Array.Copy(radiusByte, 0, data, 25, 4);
            Array.Copy(noiseByte, 0, data, 29, 4);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public void FnConfirm()
        {
            byte[] data = new byte[1];
            data[0] = 7; // tag byte
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
            //Debug.Log("ACK");
        }

        public void FnDisconnect()
        {
            byte[] data = new byte[1];
            data[0] = 3; // tag byte
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Close();
            client.Close();
        }

        private void DoRead(IAsyncResult ar)
        {
            try
            {
                Thread.Sleep(1);
                // Finish asynchronous read into readBuffer and return number of bytes read
                int BytesRead = client.GetStream().EndRead(ar);
                //Debug.Log("Message came, size: " + BytesRead);
                if (BytesRead < 1)
                {
                    // if no bytes were read server has shut down
                    res = "Disconnected";
                    return;
                }
                switch (readBuffer[0])
                {
                    case 1: // Server reply
                        byte[] msgByte = new byte[BytesRead - 1];
                        Array.Copy(readBuffer, 1, msgByte, 0, msgByte.Length);
                        strMessage = Encoding.ASCII.GetString(msgByte, 0, msgByte.Length);
                        break;
                        /*
                    case 5: // Object number and position
                        byte[] intNumber = new byte[4];
                        byte[] xPosByte = new byte[4];
                        byte[] yPosByte = new byte[4];
                        byte[] zPosByte = new byte[4];
                        Array.Copy(readBuffer, 1, intNumber, 0, 4);
                        Array.Copy(readBuffer, 5, xPosByte, 0, 4);
                        Array.Copy(readBuffer, 9, yPosByte, 0, 4);
                        Array.Copy(readBuffer, 13, zPosByte, 0, 4);
                        int num = System.BitConverter.ToInt32(intNumber, 0);
                        float xPos = System.BitConverter.ToSingle(xPosByte, 0);
                        float yPos = System.BitConverter.ToSingle(yPosByte, 0);
                        float zPos = System.BitConverter.ToSingle(zPosByte, 0);
                        strMessage = "ACK";
                        //Debug.Log("Ack nr: " + num);
                        //Debug.Log("Number: " + num + " X: " + xPos + " Y: " + yPos + " Z: " + zPos);
                        counter++;
                        break;
                        */
                    case 55: // Object number and position array
                        byte[] posCount = new byte[2];
                        Array.Copy(readBuffer, 1, posCount, 0, 2);
                        float ct = System.BitConverter.ToInt16(posCount, 0);
                        //Debug.Log("Positions: " + ct);
                        for (int i = 0; i < ct; i++)
                        {
                            counter++; // used in debug
                            int posNum = System.BitConverter.ToInt32(readBuffer, i * 16 + 3); // Object number
                            float xPos = System.BitConverter.ToSingle(readBuffer, i * 16 + 7); // x position
                            float yPos = System.BitConverter.ToSingle(readBuffer, i * 16 + 11); // y position
                            float zPos = System.BitConverter.ToSingle(readBuffer, i * 16 + 15); // z position
                            //
                            // CALL FUNCTION HERE
                            //
                        }
                        strMessage = "ACK";
                        break;
                }
                ProcessCommands(strMessage);
                // Start a new asynchronous read into readBuffer
                client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
            }
            catch
            {
                res = "Disconnected, read error";
            }
        }

        // Process the command received from the server, and take appropriate action
        private void ProcessCommands(string strMessage)
        {
            string[] dataArray;

            // Message parts are divided by "|"  Break the string into an array accordingly
            // Well, they're not now, but they were/will be (maybe?)
            dataArray = strMessage.Split((char)124);
            // dataArray(0) is the command.
            switch (dataArray[0])
            {
                case "JOIN":
                    // Server acknowledged login
                    res = "Server acknowledged login";
                    break;
                case "ACK":
                    // Received packet, send confirmation to server
                    FnConfirm();
                    break;
                case "REFUSE":
                    // Server refused login (another user connected)
                    res = "Server refused connection";
                    break;
            }
        }
    }
}