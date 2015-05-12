using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

public delegate void LineReceive(UserConnection sender, string Data);

// The UserConnection class encapsulates the functionality of a TcpClient connection
// with streaming for a single user

public class UserConnection
{
    const int READ_BUFFER_SIZE = 2000 * 16;
    // Overload the new operator to set up a read thread
    public UserConnection(TcpClient client)
    {
        this.client = client;

        // Start asynchronous read thread, save data into readBuffer
        if (this.client.GetStream().CanRead)
        {
            this.client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(StreamReceiver), null);
            var asyncCallback = new AsyncCallback(StreamReceiver);
            Console.WriteLine("User connected");
        }
    }

    private TcpClient client;
    private byte[] readBuffer = new byte[READ_BUFFER_SIZE];
    private string strName;
    public bool readySend = true;
    public bool clientReady = false;

    public string Name
    {
        get { return strName; }
        set { strName = value; }
    }

    public event LineReceive LineReceived;

    // Send positions array prepared by PositionsPacket class object
    public void SendPosPacket(byte[] data)
    {
        //Console.WriteLine("Sending pos...");
        readySend = false;
        lock (client.GetStream())
        {
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
        //readySend = true;
    }

    // Not used right now, bud don't telete it pls (._.)
    /*
    public void SendPos(int i, float x, float y, float z)
    {
        //Console.WriteLine("Sending pos...");
        //readySend = false;
        lock (client.GetStream())
        {
            byte[] data = new byte[17];
            data[0] = 5; // tag byte
            byte[] xByte = BitConverter.GetBytes(x);
            byte[] yByte = BitConverter.GetBytes(y);
            byte[] zByte = BitConverter.GetBytes(z);
            byte[] iByte = BitConverter.GetBytes(i);
            Array.Copy(iByte, 0, data, 1, 4);
            Array.Copy(xByte, 0, data, 5, 4);
            Array.Copy(yByte, 0, data, 9, 4);
            Array.Copy(zByte, 0, data, 13, 4);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
        //readySend = true;
    }
     */

    // Send a message to the user
    public void SendData(string inputData)
    {
        lock (client.GetStream())
        {
            byte[] inputDataByte = Encoding.ASCII.GetBytes(inputData);
            byte[] data = new byte[inputDataByte.Length + 1];
            data[0] = 1; // tag byte
            Array.Copy(inputDataByte, 0, data, 1, inputDataByte.Length);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            //Console.WriteLine("Sending reply with the length of " + data.Length);
        }
    }

    // Explicitly terminate connection to avoid receiving empty messages
    public void TerminateConnection()
    {
        client.GetStream().Flush();
        client.GetStream().Close();
        client.Close();
        Console.WriteLine("Terminating connection");
    }

    // Begins asynchronous read from stream, convert the data and pass it through event
    private void StreamReceiver(IAsyncResult ar)
    {
        int BytesRead;
        string strMessage = null;
        NetworkStream stream = client.GetStream();
        try
        {
            // Ensure that no other threads try to use the stream at the same time.
            lock (stream)
            {
                // Finish asynchronous read into readBuffer and get number of bytes read.
                BytesRead = stream.EndRead(ar);
            }
            //Console.WriteLine("Message received, size: " + BytesRead);
            switch (readBuffer[0])
            {
                case 1: // Inform server that client logged in - refuse next connections
                    byte[] userByte = new byte[BytesRead - 1];
                    Array.Copy(readBuffer, 1, userByte, 0, userByte.Length);
                    string user = Encoding.ASCII.GetString(userByte, 0, userByte.Length);
                    strMessage = "CONNECT|" + user;
                    break;
                case 2: // Receive player position sent by client
                    float xPos = System.BitConverter.ToSingle(readBuffer, 1);
                    float zPos = System.BitConverter.ToSingle(readBuffer, 5);
                    //
                    // CALL FUNCTION HERE
                    //
                    strMessage = "POS|" + xPos + "|" + zPos;
                    break;
                case 3: // Disconnect request
                    strMessage = "DISCONNECT";
                    break;
                case 7: // Confirmation from client about packet reception
                    strMessage = "ACK";
                    break;
                case 8: // Receive set of properties sent by client
                    float numP = System.BitConverter.ToInt32(readBuffer, 1);
                    float sizeP = System.BitConverter.ToSingle(readBuffer, 5);
                    float wStrP = System.BitConverter.ToSingle(readBuffer, 9);
                    float wStrFlucP = System.BitConverter.ToSingle(readBuffer, 13);
                    float wDirP = System.BitConverter.ToSingle(readBuffer, 17);
                    float wDirFlucP = System.BitConverter.ToSingle(readBuffer, 21);
                    float radiusP = System.BitConverter.ToSingle(readBuffer, 25);
                    float noiseP = System.BitConverter.ToSingle(readBuffer, 29);
                    //
                    // CALL FUNCTION HERE
                    //
                    /*
                    Console.WriteLine("New Properties: \nNumber of particles: " + numP + "\nSize: " + sizeP + "\nWind strength: " + wStrP + 
                        "\nWind strength fluctuation: " + wStrFlucP + "\nWind direction: " + wDirP + "\nWind direction fluctuation: " + wDirFlucP +
                        "\nRadius: " + radiusP + "\nNoise: " + noiseP);
                    */
                    strMessage = "PROP|" + numP + "|" + sizeP + "|" + wStrP + "|" + wStrFlucP + "|" + wDirP + "|" + wDirFlucP + "|" + radiusP + "|" + noiseP;
                    break;
            }

            // Pass the data
            LineReceived(this, strMessage);

            // Ensure that no other threads try to use the stream at the same time
            lock (stream)
            {
                // Start a new asynchronous read into readBuffer
                client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(StreamReceiver), null);
            }
        }
        catch (Exception e)
        {
            // Spams error on disconnecting, leave it commented (or get annoyed by it, idc)
            //Console.WriteLine(e.ToString());
        }
    }
}