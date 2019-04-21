﻿using System;
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

        public List<double> YY { get; set; }
        public List<double> Y1 { get; set; }
        public List<double> Y2 { get; set; }
        public List<double> Y3 { get; set; }
        public List<double> Y4 { get; set; }
        public List<double> FY { get; set; }

        public MethodStateRK4(int n)
        {
            if (n < 0)
                throw new Exception("array length < 0");

            dt = 0;
            eps = 0;
            YY = new List<double>(n);
            Y1 = new List<double>(n);
            Y2 = new List<double>(n);
            Y3 = new List<double>(n);
            Y4 = new List<double>(n);
            FY = new List<double>(n);
        }
        public void init(double _dt, double _eps, int n)
        {
            if (n < 0)
                throw new Exception("array length < 0");

            dt = _dt;
            eps = _eps;
            YY = new List<double>(n);
            Y1 = new List<double>(n);
            Y2 = new List<double>(n);
            Y3 = new List<double>(n);
            Y4 = new List<double>(n);
            FY = new List<double>(n);
        }
        public void clear(int n)
        {
            if (n < 0)
                throw new Exception("array length < 0");

            YY = new List<double>(n);
            Y1 = new List<double>(n);
            Y2 = new List<double>(n);
            Y3 = new List<double>(n);
            Y4 = new List<double>(n);
            FY = new List<double>(n);
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
        protected IMethodFunction function;

        /// Выделение памяти под рабочие массивы
        /// <param name="func">Рассчетная модель системы</param>
        /// <param name="n">Размерность массивов</param>
        public IMethod(IMethodFunction func, int n)
        {
            function = func;
        }

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

        public void setState(double _t, List<double> _res)
        {
            t = _t;
            result = _res;
            state.clear(result.Count);
        }

        public override void getNextStep()
        {
            int i = 0;
            if (state.dt < 0) return;

            state.Y1 = function.calculate(t, result);
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
