using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    [Serializable]
    class SnowEnvironment
    {
        private int sPiecesNum;     //Liczba płatków
        private byte radius;        //Promień sceny
        private double windStr;       //Siła wiatru - aktualna
        private double windStrSET;    //siła wiatru - ustawiona
        private double windDir;   //kąt ustawienia wiatru w płaszczyźnie XZ - aktualny, tzn. brany do obliczeń
        private double windDirSET; //kąt ustawienia wiatru w płaszczyźnie XZ - ustawiony przez użytkownika w oknie parametrów
        private byte windStrFluc;   //Maksymalna fluktuancja siły wiatru
        private double windDirFluc; //Maksymalna fluktuancja kierunku wiatru- jednakowa dla wszystkich współrzędnych
        private double density;     //gęstośc ośrodka, w którym wystepuje opad (tutaj- gęstość powietrza)
        private double gravitation; //stała grawitacji
        private double C_coefficient;   //Współczynnik C- podobno zawsze równy 0.3
        private byte Scene_Height;  //Wysokość sceny
        public SnowEnvironment(int sPiecesNum_init, byte radius_int, byte windStr_init, double windDir_init, byte windStrFluc_init, double windDirFluc_init, double density_init, double gravitation_init, double C_coefficient_init, byte Scene_Height_init)
        {
            Random random = new Random();
            sPiecesNum = sPiecesNum_init;
            radius = radius_int;
            windStrSET = windStr_init;
            windDirSET = windDir_init;
            windStrFluc = windStrFluc_init;
            windDirFluc = windDirFluc_init;
            density = density_init;
            gravitation = gravitation_init;
            C_coefficient = C_coefficient_init;
            Scene_Height = Scene_Height_init;
            windDir=(double)random.Next((int)(windDirSET - windDirFluc), (int)(windDirSET + windDirFluc));
            windStr = (byte)random.Next((int)(windStrSET - windStrFluc), (int)(windStrSET + windStrFluc));
        }
        //Funkcje pozwalające korzystać z pól publicznych z zewnątrz klasy
        public double getGravitation()
        {
            return gravitation;
        }
        public double getDensity()
        {
            return density;
        }
        public double getRadius()
        {
            return radius;
        }
        public double getWindStr()
        {
            return windStr;
        }
        public double getWindStrSET()
        {
            return windStrSET;
        }
        public double getWindStrFluc()
        {
            return windStrFluc;
        }
        public double getWindDir()
        {
            return windDir;
        }
        public double getWindDirSET()
        {
            return windDirSET;
        }
        public double getWindDirFluc()
        {
            return windDirFluc;
        }
        public double getC_coefficient()
        {
            return C_coefficient;
        }
        public int getPiecesNumber()
        {
            return sPiecesNum;
        }
        public double getSceneHeight()
        {
            return Scene_Height;
        }
        public void setWindDir(double signDir)
		{
            windDir += signDir;
		}
        public void setWindStr(double signStr)
        {
            windStr += signStr*0.05;
            //System.Console.WriteLine(windStr);
            //System.Threading.Thread.Sleep(2000);
        }
    }
}
