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
            byte rad=50;    //Promień od jakiego należy tworzyć płatki
            //
            Environment otoczenie = new Environment(SnowNumber, radius, windStr, windDir, windStrFluc, windDirFluc, 1.25, 9.81, 0.3, SceneHeight); //Inicjalizacja otoczenia- te dane (niektóre) są przypadkowe, w rzeczywistości będą pobierane od klienta
            SnowFlake[] platki = new SnowFlake[otoczenie.getPiecesNumber()];  //Alokacja pamięci dla tablicy obiektów- płatków
            for(int i=0;i<otoczenie.getPiecesNumber();i++)  //Inicjalizacja każdego płatka wraz z nadaniem pozycji startowej
            {
                platki[i]=new SnowFlake(i,platki[i].pozycja_startowa(i,rad, radius,SceneHeight),0.001, 0.00000312);
            }
            double K = otoczenie.getC_coefficient() * platki[0].getSize() * otoczenie.getDensity() * 0.5;   //Liczmy współczynnik K niezbędny do obliczenia prędkości granicznej- takiej samej dla każdego płatka (zakładamy, że są identyczne)
            //Wyznaczenie pozycji pojedyńczego płatka (np. pierwszego):
            platki[0].NewPosition(PlayerPos, platki[0].LimitedSpeed(platki[0].getMass(), otoczenie.getGravitation(), K), windStr, windDir, windStrFluc, windDirFluc, time_step, SceneHeight, radius);
        }
    }
}
