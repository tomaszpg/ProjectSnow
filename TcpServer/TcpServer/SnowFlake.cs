using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
     [Serializable]
    class SnowFlake
    {
        private int id;
        private double[] Pos;
        private double mass;
        private double size;
        public SnowFlake(int id_init, double[] Pos_init, double mass_init, double size_init)
        {
            id = id_init;
            Pos = new double[3];
            for (int i = 0; i < 3; i++)
                Pos[i] = Pos_init[i];
            mass = mass_init;
            size = size_init;
        }
        //Funkcja pozwalająca nadać płatkom jakieś pozycje początkowe
        /*public double[] pozycja_startowa(int i, byte r, byte R, byte SceneHeight)    //i- indeks płatka, r- promień tworzenia płatków, R- promień sceny
        {
            Random random = new Random();
            double[] wynik = new double[3];
            wynik[0] = random.Next(0, (int)R) * Math.Pow(-1.0, (double)(random.Next(1, 2)));    //Losowanie współrzędnej x płatka z zakresu od 0 do R z losowym znakiem
            wynik[1] = SceneHeight;
            wynik[2] = Math.Sqrt(Math.Pow((double)random.Next(r, (int)R), 2.0) - Math.Pow(wynik[0], 2.0));  //wyznaczanie trzeciej współrzędnej płatka (z równania  okręgu o promieniu losowanym z przedziału <r,R> i środku (0,0)) z losowym znakiem
            return wynik;
        }*/
        //Funkcja obliczająca wartość prędkości granicznej płatka- wektor o zerowych współrzędnych x i z.
        public double[] LimitedSpeed(double mass, double gravity, double K)
        {
            double[] wynik = new double[3];
            wynik[0] = 0;
            wynik[1] = -Math.Sqrt(mass * gravity / K);
            wynik[2] = 0;
            return wynik;
        }
        //Najważniejsza funkcja- obliczanie nowej pozycji płatka będącej wypadkową warunków atmosferycznych, prędkości granicznej i aktualnego położenia płatka (może istnieć koniecznośc jego symetrycznego odbicia względem kamery)
        public void NewPosition(double[] PlayerPos, double[] LimitSpeed, double time, SnowEnvironment otoczenie)
        {

            Random random = new Random();
            double[] r = new double[3];
			//Wyznaczenie składowych wektora prędkości wiatru- z uwzględnieniem fluktuancji siły i kierunku wiatru
            for (int i = 0; i < 3; i++)
            {
				int random_variable=random.Next(0,100);
                if (random_variable >=20)
                {
                    otoczenie.setWindDir_now(otoczenie.getWindDir_now());
                    r[i] = (double)random.Next((int)System.Math.Max(otoczenie.getWindStr() - otoczenie.getWindStrFluc(), 0), (int)(otoczenie.getWindStr() + otoczenie.getWindStrFluc())) * otoczenie.getWindDir_now()[i];
                    System.Console.WriteLine("Skladowa wiatru= ");
                    System.Console.WriteLine(r[i]);
                    //System.Threading.Thread.Sleep(1000);
                }
                else
                {
                    for (double j = 80; j < 100.0; j += 5.0)
                    {

                        if ((double)random_variable > j)
                        {
                            double range_divisor = (100.0 - j) / 5.0;
                            r[i] = random.Next((int)System.Math.Max(otoczenie.getWindStr() - otoczenie.getWindStrFluc()/range_divisor, 0), (int)(otoczenie.getWindStr() + otoczenie.getWindStrFluc()/range_divisor)) * random.Next((int)(100 * System.Math.Max((otoczenie.getWindDir_now())[i] - otoczenie.getWindDirFluc()/range_divisor, 0)), (int)(100 * (otoczenie.getWindDir_now())[i])) * 0.01;
                            System.Console.WriteLine("Skladowa wiatru= ");
                            System.Console.WriteLine(i);
                        }
                    }
                }
            }

            double[] T = new double[3];

            for (int i = 0; i < 3; i++)
            {
                //Wyznaczenie skladowych wektora translacji płatka sniegu
                T[i] = r[i] * time;
                if (i == 1)
                    T[i] += LimitSpeed[1] * time*2.0;
            }

            for (int i = 0; i < 3; i++)
            {
                //Wykonanie translacji płatka
                Pos[i] += T[i];
            }
            //Sprawdzenie, czy nie wystąpiła kolizja z podłożem
            if (Pos[1] <= 0)
                Pos[1] = otoczenie.getSceneHeight();
            //Sprawdzenie, czy kamera nie oddaliła się od płatka o odległość większą niż promień sceny 
            double distance = Math.Sqrt(Math.Pow(PlayerPos[0] - Pos[0], 2) + Math.Pow(PlayerPos[2] - Pos[2], 2));
            if (distance >= otoczenie.getRadius())
            {
                Pos[0] = PlayerPos[0] + (PlayerPos[0] - Pos[0]);
                Pos[2] = PlayerPos[2] + (PlayerPos[2] - Pos[2]);
            }
        }
        //Funkcje zapewniajace dostep do pól prywatnych klasy
        public int getId()
        {
            return id;
        }

        public double getSize()
        {
            return size;
        }

        public double getMass()
        {
            return mass;
        }
        public double[] getPosition()
        {
            return Pos;
        }

    }
}
