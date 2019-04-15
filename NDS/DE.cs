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
    public struct SystemParams
    {
        public double h;
        public double h1;
        public double lambda;
        public double p;
        public double mu;
        public double mu0;
        public double R;
        public double E;
        public double phi;
        public double gamma;
    }
    public abstract class BaseDE
    {        
        public SystemParams sysParam;
        public IMethod method;

        /// Начальные значения системы
        /// <param name="N">размер системы</param>
        /// <param name="SysParam"> h, h1, lambda, p, mu0, mu, R, E, phi, gamma </param>
        public BaseDE(int n, SystemParams p)
        {
            method.Init(n);
            SetParam(p);
        }      
        public void SetParam(SystemParams p)
        {
            sysParam = p;
        }
        /// <summary>
        /// Послеударные уравнения скоростей
        /// </summary>
        /// <param name="t">текущее время</param>
        /// <param name="U">доударное x'-</param>
        /// <param name="U1">доударное x'1-</param>
        /// <returns></returns>
        abstract public double CalcHit(double t, double U, double U1);
        public double F(double t)
        {
            double F1 = sysParam.E - sysParam.mu * Math.Cos(t);
            double F2 = -sysParam.mu * sysParam.gamma * Math.Cos(t - sysParam.phi);
            return Math.Max(F1, F2);
        }
        public double Fdt(double t)
        {
            double F1 = sysParam.E - sysParam.mu * Math.Cos(t);
            double F2 = -sysParam.mu * sysParam.gamma * Math.Cos(t - sysParam.phi);
            if (F1 > F2)
            {
                return sysParam.mu * Math.Sin(t);
            }
            return sysParam.mu * sysParam.gamma * Math.Sin(t - sysParam.phi);
        }
    }
    public class DE1 : BaseDE
    {
        public DE1(int n, SystemParams p) : base(n, p)
        {
        }

        /// Описание системы
        /// <param name="t">Время</param>
        /// <param name="Y">Решение</param>
        /// <returns>Правая часть</returns>
        protected override double[] Calculate(double t, double[] Y)
        {
            // система уравнений модели
            FY[0] = Y[1];
            FY[1] = -2 * h * Y[1] - p;

            return FY;
        }
        public override double CalcHit(double t, double Y, double Y1)
        {
            return ((mu0 - R) * Y + (1 + R) * Y1 + (1 + R) * Fdt(t)) / (1 + mu0);
        }
    }

    public class DE2 : BaseDE
    {
        public DE2(int N, double[] SysParam) : base(N, SysParam)
        {
        }

        /// Описание системы
        /// <param name="t">Время</param>
        /// <param name="Y">Решение</param>
        /// <returns>Правая часть</returns>
        protected override double[] Calculate(double t, double[] Y)
        {
            // система уравнений модели
            FY[0] = Y[1];
            FY[1] = -2 * h1 * Y[1] - lambda * lambda * Y[0] - p;

            return FY;
        }
        public override double CalcHit(double t, double Y, double Y1)
        {
            return (mu0 * (1 + R) * Y + (1 - R * mu0) * Y1 - mu0 * (1 + R) * Fdt(t)) / (1 + mu0);
        }
    }

    public class SystemDE
    {
        // объекты диф. ур.
        public DE1 de1;
        public DE2 de2;
        public double t;
        public double[,] res;

        public SystemDE(int N, double[] sysParam)
        {
            de1 = new DE1(N, sysParam);
            de2 = new DE2(N, sysParam);
            t = 0;
        }

        public void SetParam(double[] sysParam)
        {
            de1.SetParam(sysParam);
            de2.SetParam(sysParam);
        }

        public double[] GetRes(int resIndex)
        {
            double[] tmp = new double[res.GetLength(1)];
            for (int i = 0; i < res.GetLength(1); i++)
            {
                tmp[i] = res[resIndex, i];
            }
            return tmp;
        }

        //вычисление
        public void SolveDiffs(double[] initState, double dt, double T)
        {
            res = new double[4, (int)(T / dt + 1)];

            de1.SetInit(0, initState);
            de2.SetInit(0, initState);
            int j = 0;
            double t = 0;
            while (t <= T)
            {
                de1.NextStep(dt);
                de2.NextStep(dt);
                if (de1.Y[0] - de2.Y[0] > de1.F(t) || de1.Y[0] - de2.Y[0] > de2.F(t))
                {
                    res[0, j] = de1.Y[0];
                    res[1, j] = de1.Y[1];
                    res[2, j] = de2.Y[0];
                    res[3, j] = de2.Y[1];
                }
                else
                {
                    res[0, j] = de1.Y[0];
                    res[1, j] = de1.CalcHit(t, de1.Y[1], de2.Y[1]);
                    res[2, j] = de1.Y[0] - de1.F(t); // de2.Y[0];
                    res[3, j] = de2.CalcHit(t, de1.Y[1], de2.Y[1]);
                    de1.SetInit(de1.t, new double[] { res[0, j], res[1, j], res[2, j], res[3, j] });
                    de2.SetInit(de2.t, new double[] { res[0, j], res[1, j], res[2, j], res[3, j] });
                }

                t = de1.t;
                j++;
            }
        }
    }

}
