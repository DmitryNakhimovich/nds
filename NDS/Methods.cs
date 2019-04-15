using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ZedGraph;

namespace NDS
{
    public struct MethodState
    {       
        // Вспомагательные для метода
        public List<double> YY;
        public List<double> Y1;
        public List<double> Y2;
        public List<double> Y3;
        public List<double> Y4;
    }
    public abstract class IMethod
    {
        protected MethodState methodState;

        /// Выделение памяти под рабочие массивы
        /// <param name="n">Размерность массивов</param>
        protected void Init(int n)
        {
            if (N < 1)
                throw new Exception("array length < 1");

            Y = new double[N];
            YY = new double[N];
            Y1 = new double[N];
            Y2 = new double[N];
            Y3 = new double[N];
            Y4 = new double[N];
            FY = new double[N];
        }
        /// Расчет правых частей системы
        /// <param name="t">текущее время</param>
        /// <param name="Y">вектор решения</param>
        /// <returns>правая часть</returns>
        abstract protected double[] Calculate(double t, double[] Y);

        /// Следующий шаг метода Рунге-Кутта
        /// <param name="dt">текущий шаг по времени</param>
        abstract public void NextStep(double dt);
    }
    public abstract class MethodRK4 : IMethod
    {
        /// <summary>
        /// Текущее время
        /// </summary>
        public double t = 0;

        /// Искомое решение, Y[0] - само решение, Y[i] - i-тая производная решения
        public List<double> Y;
               
        /// Внутренние переменные 
        private double[] YY, Y1, Y2, Y3, Y4;
        protected double[] FY;

        // "N" размерность системы
        public MethodRK4(int N)
        {
            Init(N);
        }
        public MethodRK4() { }
      
        /// Установка начальных условий
        /// <param name="t0">Начальное время</param>
        /// <param name="Y0">Начальное условие</param>
        public void SetInit(double t0, double[] Y0)
        {
            t = t0;
            if (Y == null)
                Init(Y0.Length);

            for (int i = 0; i < Y.Length; i++)
                Y[i] = Y0[i];
        }

        public override void NextStep(double dt)
        {
            int i;
            if (dt < 0) return;

            // рассчитать Y1
            Y1 = Calculate(t, Y);
            for (i = 0; i < Y.Length; i++)
                YY[i] = Y[i] + Y1[i] * (dt / 2.0);

            // рассчитать Y2
            Y2 = Calculate(t + dt / 2.0, YY);
            for (i = 0; i < Y.Length; i++)
                YY[i] = Y[i] + Y2[i] * (dt / 2.0);

            // рассчитать Y3
            Y3 = Calculate(t + dt / 2.0, YY);
            for (i = 0; i < Y.Length; i++)
                YY[i] = Y[i] + Y3[i] * dt;

            // рассчитать Y4
            Y4 = Calculate(t + dt, YY);

            // рассчитать решение на новом шаге
            for (i = 0; i < Y.Length; i++)
                Y[i] = Y[i] + dt / 6.0 * (Y1[i] + 2.0 * Y2[i] + 2.0 * Y3[i] + Y4[i]);

            // рассчитать следующее время
            t += dt;
        }
    }
        
}
