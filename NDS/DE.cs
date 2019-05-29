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

        public SystemParams(
            double _h,
            double _h1,
            double _lambda,
            double _p,
            double _mu0,
            double _mu,
            double _R,
            double _E,
            double _phi,
            double _gamma)
        {
            h = _h;
            h1 = _h1;
            lambda = _lambda;
            p = _p;
            mu0 = _mu0;
            mu = _mu0;
            R = _R;
            E = _E;
            phi = _phi;
            gamma = _gamma;
        }

        public void init(
            double _h,
            double _h1,
            double _lambda,
            double _p,
            double _mu0,
            double _mu,
            double _R,
            double _E,
            double _phi,
            double _gamma)
        {
            h = _h;
            h1 = _h1;
            lambda = _lambda;
            p = _p;
            mu0 = _mu0;
            mu = _mu0;
            R = _R;
            E = _E;
            phi = _phi;
            gamma = _gamma;
        }

        public void setParam(string paramName, double paramVal)
        {
            switch (paramName)
            {
                case "h":
                    h = paramVal;
                    break;
                case "h1":
                    h1 = paramVal;
                    break;
                case "lambda":
                    lambda = paramVal;
                    break;
                case "p":
                    p = paramVal;
                    break;
                case "mu0":
                    mu0 = paramVal;
                    break;
                case "mu":
                    mu = paramVal;
                    break;
                case "R":
                    R = paramVal;
                    break;
                case "E":
                    E = paramVal;
                    break;
                case "phi":
                    phi = paramVal;
                    break;
                case "gamma":
                    gamma = paramVal;
                    break;
                default:
                    break;
            }
        }
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

        public void setInit(double _t, List<double> _res, double _dt, double _eps)
        {
            time.Clear();
            result.Clear();

            time.Add(_t);
            result.Add(new List<double>(_res));
            method.setState(_t, new List<double>(_res), _dt, _eps);
        }
        public void setState(int index, double _t, List<double> _res, double _dt, double _eps)
        {
            if (index < 0)
                throw new Exception("index < 0");

            time[index] = _t;
            result[index] = new List<double>(_res);
            method.setState(_t, new List<double>(_res), _dt, _eps);
        }
        public void setState(int index, double _t, List<double> _res)
        {
            if (index < 0)
                throw new Exception("index < 0");

            time[index] = _t;
            result[index] = new List<double>(_res);
            method.setState(_t, new List<double>(_res));
        }
        public void setParam(SystemParams _sysParam)
        {
            sysParam = _sysParam;
            method.function.sysParam = _sysParam;
        }

        public void getNextStep()
        {
            method.getNextStep();
            time.Add(method.t);
            result.Add(new List<double>(method.result));
        }
        public List<double> getResult(int index)
        {
            List<double> res = new List<double>();
            foreach (List<double> r in result)
            {
                res.Add(r[index]);
            }
            return res;
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
            de1.setParam(_sysParam);
            de2.setParam(_sysParam);
        }

        public void solveDiffs(List<double> initStateDE1, List<double> initStateDE2, double dt, double T, double eps)
        {
            de1.setInit(0, initStateDE1, dt, eps);
            de2.setInit(0, initStateDE2, dt, eps);
            int i = 0; // 0 - initState
            bool isActive = true;

            while (isActive)
            {
                de1.getNextStep();
                de2.getNextStep();
                i++;

                if (de1.method.getdt() != de2.method.getdt())
                {
                    throw new Exception("no code here");
                    de1.result.RemoveAt(i);
                    de2.result.RemoveAt(i);
                    de1.time.RemoveAt(i);
                    de2.time.RemoveAt(i);
                    double _dt = de1.method.getdt() < de2.method.getdt()
                        ? de1.method.getdt()
                        : de2.method.getdt();
                    de1.setState(i - 1, de1.time.Last(), de1.result.Last(), _dt, eps);
                    de2.setState(i - 1, de2.time.Last(), de2.result.Last(), _dt, eps);
                    i--;
                    continue;
                }

                List<double> de1Res = de1.result.Last();
                List<double> de2Res = de2.result.Last();
                double de1t = de1.time.Last();
                double de2t = de2.time.Last();

                if (
                    (de1Res[0] - de2Res[0] <= de1.getF(de1t)) ||
                    (de1Res[0] - de2Res[0] <= de2.getF(de2t))
                   )
                {
                    de1.result[i][1] = de1.getHit(de1t, de1Res[1], de2Res[1]);
                    de2.result[i][0] = de1Res[0] - de1.getF(de1t);
                    de2.result[i][1] = de2.getHit(de2t, de1Res[1], de2Res[1]);
                    de1.method.setState(de1t, de1.result[i]);
                    de2.method.setState(de2t, de2.result[i]);
                }

                if (de1t > T || de2t > T)
                {
                    isActive = false;
                }
            }
        }

        public void solveBifurcation(List<double> initStateDE1, List<double> initStateDE2, double dt, double T, double eps, int hitsCount)
        {
            de1.setInit(0, initStateDE1, dt, eps);
            de2.setInit(0, initStateDE2, dt, eps);
            de1.result.Clear();
            de1.time.Clear();
            de2.result.Clear();
            de2.time.Clear();
            int i = 0; // 0 - initState
            bool isActive = true;
            int hitsEnter = 0;
            int hitsDone = 0;

            while (isActive)
            {
                de1.method.getNextStep();
                de2.method.getNextStep();
                i++;

                if (de1.method.getdt() != de2.method.getdt())
                {
                    throw new Exception("no code here");
                    de1.result.RemoveAt(i);
                    de2.result.RemoveAt(i);
                    de1.time.RemoveAt(i);
                    de2.time.RemoveAt(i);
                    double _dt = de1.method.getdt() < de2.method.getdt()
                        ? de1.method.getdt()
                        : de2.method.getdt();
                    de1.setState(i - 1, de1.time.Last(), de1.result.Last(), _dt, eps);
                    de2.setState(i - 1, de2.time.Last(), de2.result.Last(), _dt, eps);
                    i--;
                    continue;
                }

                List<double> de1Res = new List<double>(de1.method.result);
                List<double> de2Res = new List<double>(de2.method.result);
                double de1t = de1.method.t;
                double de2t = de2.method.t;

                if (
                    (de1Res[0] - de2Res[0] <= de1.getF(de1t)) ||
                    (de1Res[0] - de2Res[0] <= de2.getF(de2t))
                   )
                {
                    List<double> de1tmpRes = new List<double>();
                    List<double> de2tmpRes = new List<double>();
                    de1tmpRes.Add(de1Res[0]);
                    de1tmpRes.Add(de1.getHit(de1t, de1Res[1], de2Res[1]));
                    de2tmpRes.Add(de1Res[0] - de1.getF(de1t));
                    de2tmpRes.Add(de2.getHit(de2t, de1Res[1], de2Res[1]));
                    de1.method.setState(de1t, de1tmpRes);
                    de2.method.setState(de2t, de2tmpRes);

                    hitsEnter++;
                    if (hitsEnter > 300)
                    {
                        de1.result.Add(de1tmpRes);
                        de2.result.Add(de2tmpRes);
                        de1.time.Add(de1t);
                        de2.time.Add(de2t);
                        hitsDone++;
                    }
                }

                if (hitsDone >= hitsCount)
                {
                    isActive = false;
                }
            }
        }
    }

}
