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

    public abstract class IMethodFunction
    {
        public SystemParams sysParam;

        public IMethodFunction(SystemParams p)
        {
            sysParam = p;
        }

        public abstract List<double> calculate(double t, List<double> Y);
    }
    public class DE1_Function : IMethodFunction
    {
        public DE1_Function(SystemParams p) : base(p)
        {
        }

        public override List<double> calculate(double t, List<double> Y)
        {
            List<double> funcValue = new List<double>();
            funcValue.Add(Y[1]);
            funcValue.Add(-2 * sysParam.h * Y[1] - sysParam.p);
            return funcValue;
        }
    }
    public class DE2_Function : IMethodFunction
    {
        public DE2_Function(SystemParams p) : base(p)
        {
        }

        public override List<double> calculate(double t, List<double> Y)
        {
            List<double> funcValue = new List<double>();
            funcValue.Add(Y[1]);
            funcValue.Add(-2 * sysParam.h1 * Y[1] - sysParam.lambda * sysParam.lambda * Y[0] - sysParam.p);
            return funcValue;
        }
    }

    public abstract class BaseDE
    {
        public SystemParams sysParam;
        public IMethod method;
        protected IMethodFunction function;
        public List<double> time;
        public List<List<double>> result;

        public BaseDE(SystemParams _sysParam, int n)
        {
            sysParam = _sysParam;
            time = new List<double>();
            result = new List<List<double>>();
        }

        /// <summary>
        /// Послеударные уравнения скоростей
        /// </summary>
        /// <param name="t">текущее время</param>
        /// <param name="U">доударное x'-</param>
        /// <param name="U1">доударное x'1-</param>
        /// <returns></returns>
        abstract public double getHit(double t, double U, double U1);

        public void setState(double _t, List<double> _res)
        {
            time.Clear();
            result.Clear();

            time.Add(_t);
            result.Add(_res);
            method.setState(_t, _res);
        }
        public void getNextStep()
        {
            method.getNextStep();
            time.Add(method.t);
            result.Add(method.result);
        }

        public double getF(double t)
        {
            double F1 = sysParam.E - sysParam.mu * Math.Cos(t);
            double F2 = -sysParam.mu * sysParam.gamma * Math.Cos(t - sysParam.phi);
            return Math.Max(F1, F2);
        }
        public double getFdt(double t)
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
        public DE1(SystemParams _sysParam, int n) : base(_sysParam, n)
        {
            function = new DE1_Function(sysParam);
            method = new MethodRK4(function, n);
        }

        public override double getHit(double t, double U, double U1)
        {
            return ((sysParam.mu0 - sysParam.R) * U + (1 + sysParam.R) * U1 + (1 + sysParam.R) * getFdt(t)) / (1 + sysParam.mu0);
        }
    }
    public class DE2 : BaseDE
    {
        public DE2(SystemParams _sysParam, int n) : base(_sysParam, n)
        {
            function = new DE2_Function(sysParam);
            method = new MethodRK4(function, n);
        }

        public override double getHit(double t, double U, double U1)
        {
            return (sysParam.mu0 * (1 + sysParam.R) * U + (1 - sysParam.R * sysParam.mu0) * U1 - sysParam.mu0 * (1 + sysParam.R) * getFdt(t)) / (1 + sysParam.mu0);
        }
    }

    public class SystemDE
    {
        public DE1 de1;
        public DE2 de2;

        public SystemDE(SystemParams _sysParam, int n)
        {
            de1 = new DE1(_sysParam, n);
            de2 = new DE2(_sysParam, n);
        }

        public void setParam(SystemParams _sysParam)
        {
            de1.sysParam = _sysParam;
            de1.method.function.sysParam = _sysParam;
            de2.sysParam = _sysParam;
            de2.method.function.sysParam = _sysParam;
        }

        public void solveDiffs(List<double> initStateDE1, List<double> initStateDE2, double dt, double T)
        {
            int i = 0;

            de1.setState(0, initStateDE1);
            de2.setState(0, initStateDE2);

            while (de1.time.Last() <= T || de2.time.Last() <= T)
            {
                if (de1.time.Last() <= T)
                {
                    de1.getNextStep();
                }
                if (de2.time.Last() <= T)
                {
                    de2.getNextStep();
                }
               
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
