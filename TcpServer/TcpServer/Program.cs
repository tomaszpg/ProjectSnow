using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using MPI;

namespace TcpServer
{
    class Program
    {
        const int PORT_NUM = 10000;
        static private Hashtable clients = new Hashtable();
        static private UserConnection mainClient;
        static private TcpListener listener;
        static private Thread listenerThread;
        static private Thread genDataThread;
        static private bool userConnected = false;
        private static bool isGenerating = false;

        private static int particeNumber = 10000;       

        // Connect user if there is slot available, otherwise send refuse message
        static private void ConnectUser(string userName, UserConnection sender)
        {
            if (userConnected)
            {
                ReplyToSender("REFUSE", sender);
            }
            else
            {
                sender.Name = userName;
                UpdateStatus(userName + " has joined the server.");
                clients.Add(userName, sender);
                mainClient = sender;
                ReplyToSender("JOIN", sender);
                userConnected = true;
                genDataThread = new Thread(GenerateData);
                genDataThread.Start();
            }
        }

        // Disconnect given user
        static private void DisconnectUser(UserConnection sender)
        {
            sender.TerminateConnection();
            UpdateStatus(sender.Name + " has left the server.");
            clients.Remove(sender.Name);
            userConnected = false;
        }

        // Background listener thread, to allow reading messages without lagging main window
        static private void DoListen()
        {
            try
            {
                // Listen for new connections
                listener = new TcpListener(System.Net.IPAddress.Any, PORT_NUM);
                listener.Start();

                do
                {
                    // Create a new user connection using TcpClient
                    UserConnection client = new UserConnection(listener.AcceptTcpClient());
                    // Create an event handler to allow the UserConnection to communicate with the window
                    client.LineReceived += new LineReceive(OnLineReceived);
                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        //
        // EXAMPLE POSITION GENERATION, RUNNING IN SEPARATE THREAD
        //
        private static void GenerateData()
        {
            try
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                const int posCt = 20000;
                PostitionsPacket posPack = new PostitionsPacket(mainClient);
                for (int i = 0; i < posCt; i++)
                {
                    posPack.AddPosition(i, 123.111f, 123.222f, 123.333f);
                }
                posPack.SendPacket();

                timer.Stop();
                Console.WriteLine("Sent " + posCt + " positions in " + timer.ElapsedMilliseconds + " ms");
                timer.Reset();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // This is the event handler for the UserConnection when it receives a full line
        // Parse the cammand and parameters and take appropriate action
        static private void OnLineReceived(UserConnection sender, string data)
        {
            string[] dataArray;
            dataArray = data.Split((char)13);
            dataArray = dataArray[0].Split((char)124);
            // dataArray(0) is the command
            switch (dataArray[0])
            {
                case "POS": // Player position received
                    // Comment on regular position updates for efficiency
                    Console.WriteLine("Position received: " + dataArray[1] + " " + dataArray[2]);
                    break;
                case "CONNECT": // New user is trying to connect
                    ConnectUser(dataArray[1], sender);
                    break;
                case "DISCONNECT": // Disconnect request from user
                    DisconnectUser(sender);
                    break;
                case "ACK": // Confirmation from client about packet reception
                    mainClient.readySend = true;
                    //Console.WriteLine("RS");
                    break;
                case "PROP": // Confirmation from client about packet reception
                    Console.WriteLine("New properties received");
                    break;
            }
        }

        // Reply to sender
        static private void ReplyToSender(string strMessage, UserConnection sender)
        {
            sender.SendData(strMessage);
        }

        // Show line in console
        static private void UpdateStatus(string statusMessage)
        {
            Console.WriteLine(statusMessage);
        }

        static private int Compute(Random rnd)
        {
            
            return rnd.Next(1, 100);
        }

        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Intracommunicator comm = Communicator.world;

                if(comm.Rank == 0)
                {
                    listenerThread = new Thread(DoListen);
                    listenerThread.Start();
                    Console.WriteLine("Listener started");
                }

                Random rnd = new Random();
                int number = particeNumber / comm.Size;
                int rest = particeNumber % comm.Size;

                if(comm.Rank == comm.Size - 1)
                {
                    number += rest;
                }

                int[] particles = new int[number];
                bool run = true;
                while (run)
                {
                    for (int i = 0; i < number; i++)
                    {
                        //TODO podstawic metode liczaca
                        particles[i] = Compute(rnd);
                    }

                    int[][] values = new int[comm.Size][];

                    comm.Gather(particles, 0, ref values);
                    if (comm.Rank == 0)
                    {
                        //TODO dac wysylanie wyniku, znajduje sie on w zmiennej values, pierwszy index to process, pod drugim sa wartosci
                        for (int i = 0; i < comm.Size; i++)
                        {
                            for (int j = 0; j < values[i].Length; j++)
                            {                             
                                Console.WriteLine(i + " nr: " + j + " val " + values[i][j]);
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(33);
                   
                }
            }
        }
    }
}
