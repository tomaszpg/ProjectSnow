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
        static private double[] PlayerPos = new double[3];       //Alokacja pamięci dla wektora położenia obserwatora (kamery)
        const double time_step = 0.033;               //odstęp między klatkami - stały [s]
        private const byte SCENE_HEIGHT = 20;
        private static SnowEnvironment otoczenie;
        const int PORT_NUM = 10000;
        static private Hashtable clients = new Hashtable();
        static private UserConnection mainClient;
        //static private Thread genDataThread;
        static private bool userConnected = false;
        private static bool isGenerating = false;
        private static bool newConfig = false;

        private static int particeNumber = 100;       
        private static double signDir = 1.0;
        private static double signStr = 1.0;
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
                //genDataThread = new Thread(GenerateData);
                //genDataThread.Start();
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
                TcpListener listener = new TcpListener(System.Net.IPAddress.Any, PORT_NUM);
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
                    PlayerPos[0] = float.Parse(dataArray[1]);
                    PlayerPos[2] = float.Parse(dataArray[2]);
                    Console.WriteLine("Position received: " + PlayerPos[0] + " " + PlayerPos[2]);
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
                    //Console.WriteLine("Parsing...");
                    double windDirInit = (double)float.Parse(dataArray[5]);
                    otoczenie = new SnowEnvironment(Int32.Parse(dataArray[1]), (byte)float.Parse(dataArray[7]), (byte)float.Parse(dataArray[3]), 
                        windDirInit, (byte)float.Parse(dataArray[4]), float.Parse(dataArray[6]), 1.25, 9.81, 0.3, SCENE_HEIGHT);
                    Console.WriteLine("NOP: " + dataArray[1] +"\nRadius: "+ dataArray[7] +"\nWind Strength: "+ dataArray[3] +"\nWind direction cos[1]: "+
                        windDirInit +"\nStrength fluctuation: "+ dataArray[4] +"\nDirection fluctuation: "+ dataArray[6]);
                    mainClient.clientReady = true;
                    newConfig = true;
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

        private static SnowFlake[] particles;
        private static int number;
        private static int pPerProc;
        private static double K;

        static private void InitComp(Intracommunicator comm)
        {
            newConfig = false;
            Random random = new Random(comm.Rank);
            pPerProc = otoczenie.getPiecesNumber() / comm.Size;
            number = otoczenie.getPiecesNumber() / comm.Size;
            int rest = otoczenie.getPiecesNumber() % comm.Size;

            if (comm.Rank == comm.Size - 1)
            {
                number += rest;
            }
            particles = new SnowFlake[number];

            for (int i = 0; i < number; i++)  //Inicjalizacja każdego płatka wraz z nadaniem pozycji startowej
            {
                double[] pozycja_startowa = new double[3];
                int promien = (int)otoczenie.getRadius();
                pozycja_startowa[0] = (double)(random.Next(1, 10000)) / 10000 * (double)promien * Math.Pow(-1.0, (double)(random.Next(1, 3)));    //Losowanie współrzędnej x płatka z zakresu od 0 do R z losowym znakiem
                pozycja_startowa[1] = (double)(random.Next(1, SCENE_HEIGHT * 1000)) / 1000;
                pozycja_startowa[2] = (double)(random.Next(1, 10000)) / 10000 * Math.Sqrt(promien * promien - pozycja_startowa[0] * pozycja_startowa[0]) *
                                      Math.Pow(-1.0, (double)(random.Next(1, 3)));
                particles[i] = new SnowFlake(pPerProc * comm.Rank + i, pozycja_startowa, 0.000001, 0.00000312);
            }
            K = otoczenie.getC_coefficient() * particles[0].getSize() * otoczenie.getDensity() * 0.5;
        }

        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Intracommunicator comm = Communicator.world;

                if (comm.Rank == 0)
                {
                    Thread listenerThread = new Thread(DoListen);
                    listenerThread.Start();
                    Console.WriteLine("Listener started");
                }

                while (true)
                {
                    if (comm.Rank == 0)
                    {
                        Console.WriteLine("Starting new connection");
                        // Wait in loop for user to connect
                        while (!userConnected)
                        {
                            Thread.Sleep(10);
                        }
                        Console.WriteLine("User connected, waiting for new properties");
                    }

                    comm.Broadcast(ref userConnected, 0);

                    // Wait in loop to get simulation properties
                    if (comm.Rank == 0)
                    {
                        while (!mainClient.clientReady)
                        {
                            Thread.Sleep(10);
                        }
                        Console.WriteLine("Properties received, starting simulation");
                    }

                    comm.Broadcast(ref otoczenie, 0);

                    InitComp(comm);

                    bool run = true;
                    while (run)
                    {
                        Random random = new Random();
                        Random randomDir = new Random();
                        Random randomStr = new Random();
                        if (otoczenie.getWindDir() >= otoczenie.getWindDirSET() + otoczenie.getWindDirFluc())
                            signDir = -1.0;
                        if (otoczenie.getWindDir() <= otoczenie.getWindDirSET() - otoczenie.getWindDirFluc())
                            signDir = 1.0;
                        if (otoczenie.getWindStr() >= otoczenie.getWindStrSET() + otoczenie.getWindStrFluc())
                            signStr = -1.0;
                        if (otoczenie.getWindStr() <= otoczenie.getWindStrSET() - otoczenie.getWindStrFluc())
                            signStr = 1.0;
                        int ranDir = randomDir.Next(0, 100);
                        int ranStr = randomStr.Next(0, 100);
                        if (ranDir <= 50)
                            signDir *= -1.0;
                        if (ranDir <= 50)
                            signDir *= -1.0;
                        otoczenie.setWindDir(signDir);
                        otoczenie.setWindStr(signStr);
                        try
                        {
                            comm.Broadcast(ref PlayerPos, 0);                             
                            for (int i = 0; i < number; i++)
                            {
                                particles[i].NewPosition(PlayerPos,
                                    particles[0].LimitedSpeed(particles[0].getMass(), otoczenie.getGravitation(), K),
                                    time_step, otoczenie);
                            }
                            SnowFlake[][] values = new SnowFlake[comm.Size][];

                            comm.Gather(particles, 0, ref values);

                            if (comm.Rank == 0 && userConnected)
                            {
                                PostitionsPacket posPack = new PostitionsPacket(mainClient);
                                for (int i = 0; i < comm.Size; i++)
                                {
                                    for (int j = 0; j < values[i].Length; j++)
                                    {
                                        double[] position = values[i][j].getPosition();
                                        posPack.AddPosition(values[i][j].getId(), (float) position[0],
                                            (float) position[1], (float) position[2]);
                                    }
                                }
                                if (userConnected)
                                {
                                    posPack.SendPacket();
                                }
                            }

                            comm.Broadcast(ref userConnected, 0);
                            if (!userConnected)
                            {
                                userConnected = false;
                                break;
                            }

                            comm.Broadcast(ref newConfig, 0);
                            if (newConfig)
                            {
                                comm.Broadcast(ref otoczenie, 0);
                                InitComp(comm);
                            }

                            Thread.Sleep(10);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Client is no longer subscribing, please restart server");
                            Console.WriteLine(e);
                            run = false;
                        }
                        random = null;
                    }
                }
            }
        }
    }
}
