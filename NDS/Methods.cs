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
    public interface IMethodState
    {
        double dt { get; set; }
        double eps { get; set; }

        void init(double _dt, double _eps, int n);
        void clear(int n);
    }
    public struct MethodStateRK4 : IMethodState
    {
        public double dt { get; set; }
        public double eps { get; set; }

        /// <summary>
        /// значения аргумента для i-ой производной
        /// </summary>
        public List<double> YY { get; set; }
        public List<double> Y1 { get; set; }
        public List<double> Y2 { get; set; }
        public List<double> Y3 { get; set; }
        public List<double> Y4 { get; set; }

        public MethodStateRK4(int n)
        {
            if (n < 0)
                throw new Exception("array length < 0");

            dt = 0;
            eps = 0;
            YY = new List<double>(new double[n]);
            Y1 = new List<double>(new double[n]);
            Y2 = new List<double>(new double[n]);
            Y3 = new List<double>(new double[n]);
            Y4 = new List<double>(new double[n]);
        }

        public void init(double _dt, double _eps, int n)
        {
            if (n < 0)
                throw new Exception("array length < 0");

            dt = _dt;
            eps = _eps;
            YY = new List<double>(new double[n]);
            Y1 = new List<double>(new double[n]);
            Y2 = new List<double>(new double[n]);
            Y3 = new List<double>(new double[n]);
            Y4 = new List<double>(new double[n]);
        }
        public void clear(int n)
        {
            if (n < 0)
                throw new Exception("array length < 0");

            YY = new List<double>(new double[n]);
            Y1 = new List<double>(new double[n]);
            Y2 = new List<double>(new double[n]);
            Y3 = new List<double>(new double[n]);
            Y4 = new List<double>(new double[n]);
        }
    }

    public abstract class IMethod
    {
        /// <summary>
        /// Текущее время
        /// </summary>
        public double t = 0;
        /// <summary>         
        /// Y[0] - само решение, Y[i] - i-тая производная решения
        /// </summary>
        public List<double> result;
        /// <summary>
        /// параметры для метода
        /// </summary>
        public IMethodState state;
        /// <summary>
        /// модель уравнений системы
        /// </summary>
        public IMethodFunction function;

        /// Выделение памяти под рабочие массивы
        /// <param name="func">Рассчетная модель системы</param>
        /// <param name="n">Размерность массивов</param>
        public IMethod(IMethodFunction func, int n)
        {
            function = func;
        }

        public abstract void setState(double _t, List<double> _res);
        public abstract void setState(double _t, List<double> _res, double _dt, double _eps);

        public abstract double getdt();

        /// Вычислить следующий шаг
        /// <param name="dt">текущий шаг по времени</param>
        abstract public void getNextStep();
    }
    public class MethodRK4 : IMethod
    {
        new public MethodStateRK4 state;

        public MethodRK4(IMethodFunction func, int n) : base(func, n)
        {
            if (n < 1)
                throw new Exception("array length < 1");

            state = new MethodStateRK4(n);
            result = new List<double>(n);
            t = 0;
        }

        public override void setState(double _t, List<double> _res)
        {
            t = _t;
            result = _res;
            state.clear(result.Count);
        }
        public override void setState(double _t, List<double> _res, double _dt, double _eps)
        {
            t = _t;
            result = _res;
            state.init(_dt, _eps, result.Count);
        }

        public override double getdt()
        {
            return state.dt;
        }

        public override void getNextStep()
        {
            int i = 0;
            int size = result.Count;
            if (state.dt < 0) return;

            state.Y1 = function.calculate(t, result);

            for (i = 0; i < size; i++)
                state.YY[i] = result[i] + state.Y1[i] * (state.dt / 2.0);
            state.Y2 = function.calculate(t + state.dt / 2.0, state.YY);

            for (i = 0; i < size; i++)
                state.YY[i] = result[i] + state.Y2[i] * (state.dt / 2.0);
            state.Y3 = function.calculate(t + state.dt / 2.0, state.YY);

            for (i = 0; i < size; i++)
                state.YY[i] = result[i] + state.Y3[i] * state.dt;
            state.Y4 = function.calculate(t + state.dt, state.YY);

            for (i = 0; i < size; i++)
                result[i] = result[i] + (state.dt / 6.0) * (state.Y1[i] + 2.0 * state.Y2[i] + 2.0 * state.Y3[i] + state.Y4[i]);

            t += state.dt;
        }
    }

}
