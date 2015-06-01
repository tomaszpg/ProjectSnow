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
        private byte windStr;       //Siła wiatru
        private double[] windDir;   //wektor cosinusów kątów nachylenia wektora wiatru względem osi x, y, z
        private byte windStrFluc;   //Maksymalna fluktuancja siły wiatru
        private double windDirFluc; //Maksymalna fluktuancja kierunku wiatru- jednakowa dla wszystkich współrzędnych
        private double density;     //gęstośc ośrodka, w którym wystepuje opad (tutaj- gęstość powietrza)
        private double gravitation; //stała grawitacji
        private double C_coefficient;   //Współczynnik C- podobno zawsze równy 0.3
        private byte Scene_Height;  //Wysokość sceny
		private double[] windDir_now;
        public SnowEnvironment(int sPiecesNum_init, byte radius_int, byte windStr_init, double[] windDir_init, byte windStrFluc_init, double windDirFluc_init, double density_init, double gravitation_init, double C_coefficient_init, byte Scene_Height_init)
        {
            sPiecesNum = sPiecesNum_init;
            radius = radius_int;
            windStr = windStr_init;
            windDir = new double[3];
            windDir_now = new double[3];
            for (int i = 0; i < 3; i++)
                windDir[i] = windDir_init[i];
			for (int i = 0; i < 3; i++)
				windDir_now[i] = windDir_init[i];
            windStrFluc = windStrFluc_init;
            windDirFluc = windDirFluc_init;
            density = density_init;
            gravitation = gravitation_init;
            C_coefficient = C_coefficient_init;
            Scene_Height = Scene_Height_init;
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
        public double getWindStrFluc()
        {
            return windStrFluc;
        }
        public double[] getWindDir()
        {
            return windDir;
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
		public double[] getWindDir_now()
		{
			return windDir_now;
		}
		public void setWindDir_now(double[] setDir)
		{
			for (int i=0; i<3; i++)
				windDir_now [i] = setDir [i];
		}
    }
}
