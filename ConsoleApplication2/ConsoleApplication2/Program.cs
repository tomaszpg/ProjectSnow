using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Zmienne pomocnicze
            double[] PlayerPos=new double[3];       //Alokacja pamięci dla wektora położenia obserwatora (kamery)
            double time_step = 0.033;               //odstęp między klatkami - stały [s]
            //
            //Parametry pobierane od klienta (Unity)
            int SnowNumber=200;             //Liczba płatków- np. 200
            byte radius=10;                //promień sceny- np. 10 m 
            byte windStr = 5;              //Siła wiatru - tzn. jego prędkość- np. 5 m/s   
            double[] windDir=new double[3]; //Alokacja pamięci dla wektora cosinusów kierunkowych wektora wiatru
            windDir[0] = 0.5; windDir[1] = 0.5; windDir[2] = 0.5;   //Przykładowe cosinusy wektora wiatru
            byte windStrFluc=1;             //Fluktuancja siły wiatru - np. maksymalna zmiana = 1 m/s             
            double windDirFluc = 0.1;       //Fluktuancja kierunku wiatru - np. maksymalna zmiana cosinusów kierunkowych o wartośc 0.1
            byte SceneHeight=10;           //Wysokość sceny (tzn. tego walca gdzie się wszystko odbywa), np. 10 m
            PlayerPos[0]=0.0; PlayerPos[1]=0.0; PlayerPos[2]=0.0;   //Przykładowa pozycja gracza: 0,0,0
            //
            Environment otoczenie = new Environment(SnowNumber, radius, windStr, windDir, windStrFluc, windDirFluc, 1.25, 9.81, 0.3, SceneHeight); //Inicjalizacja otoczenia- te dane (niektóre) są przypadkowe, w rzeczywistości będą pobierane od klienta
            SnowFlake[] platki = new SnowFlake[otoczenie.getPiecesNumber()];  //Alokacja pamięci dla tablicy obiektów- płatków
            for(int i=0;i<otoczenie.getPiecesNumber();i++)  //Inicjalizacja każdego płatka wraz z nadaniem pozycji startowej
            {
                Random random = new Random();
                double[] pozycja_startowa = new double[3];
                int promien = random.Next(0, (int)radius);
                pozycja_startowa[0] =  (double)promien* Math.Pow(-1.0, (double)(random.Next(1, 2)));    //Losowanie współrzędnej x płatka z zakresu od 0 do R z losowym znakiem
                pozycja_startowa[1] = SceneHeight;
                pozycja_startowa[2] = Math.Sqrt(Math.Pow((double)promien, 2.0) - Math.Pow(pozycja_startowa[0], 2.0));  //wyznaczanie trzeciej współrzędnej płatka (z równania  okręgu o promieniu losowanym z przedziału <r,R> i środku (0,0)) z losowym znakiem
                platki[i]=new SnowFlake(i,pozycja_startowa,0.000001, 0.00000312);
            }
            double K = otoczenie.getC_coefficient() * platki[0].getSize() * otoczenie.getDensity() * 0.5;   //Liczmy współczynnik K niezbędny do obliczenia prędkości granicznej- takiej samej dla każdego płatka (zakładamy, że są identyczne)
            //Wyznaczenie pozycji pojedyńczego płatka (np. pierwszego):
            platki[0].NewPosition(PlayerPos, platki[0].LimitedSpeed(platki[0].getMass(), otoczenie.getGravitation(), K),time_step, otoczenie);
            System.Console.WriteLine((platki[0].getPosition())[0].ToString());
            System.Console.WriteLine((platki[1].getPosition())[0].ToString());
            System.Console.WriteLine((platki[2].getPosition())[0].ToString());
            System.Console.WriteLine(K.ToString());
            System.Console.WriteLine(platki[0].LimitedSpeed(platki[0].getMass(), otoczenie.getGravitation(), K)[1].ToString());
            for (int i = 0; i < 10000; i++)
            {
                platki[0].NewPosition(PlayerPos, platki[0].LimitedSpeed(platki[0].getMass(), otoczenie.getGravitation(), K), time_step, otoczenie);
                System.Console.WriteLine(platki[0].getPosition()[0].ToString() + "\t" + platki[0].getPosition()[1].ToString() + "\t"+platki[0].getPosition()[2].ToString());
            }
            Console.ReadKey();
        }
    }
}
