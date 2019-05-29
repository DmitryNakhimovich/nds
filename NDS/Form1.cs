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
    public partial class Form1 : Form
    {
        //графики
        GraphPane graphPane_Oscilogramm;
        GraphPane graphPane_Phase;
        GraphPane graphPane_Bifurcation;
        // объект системы
        SystemDE sysDE;
        // параметры системы        
        SystemParams sysParam;
        // начальные условия
        double x_0, u_0, x1_0, u1_0;
        // вычисленные точки
        List<double> t_res; // t
        List<double> x_res; // x
        List<double> u_res; // x'
        List<double> x1_res; // x1
        List<double> u1_res; // x1'
        // координаты точек для графиков
        List<double> graphData_X; // X-axis
        List<double> graphData_Y; // Y-axis
        // тип осей для графиков
        string graphLabel_X = "x"; // X-axis
        string graphLabel_Y = "u"; // Y-axis
        // параметры интегрирования
        double dt; // шаг 
        double T; // макс время 
        double eps; // точность
        // параметры анимации        
        int maxGraphDot = 0; // максимальное число точек T/dt
        int capGraphDot = 100000; // порог отображаемых точек на графике
        int speedGraphDot = 100; // скорость отрисовки анимации
        int startGraphDot = 0; // начало отрисовки анимации
        int stepGraphDot = 1; // шаг выборки из результатов        
        int curGraphDot = 0; // текущая точка анимации     

        public Form1()
        {
            InitializeComponent();

            sysDE = new SystemDE(sysParam, 2);
            graphPane_Oscilogramm = zedGraphControl2.GraphPane;
            graphPane_Phase = zedGraphControl1.GraphPane;
            graphPane_Bifurcation = zedGraphControl3.GraphPane;
        }

        // Инициализировать параметры системы из формы
        private void getSysParamData()
        {
            try
            {
                sysParam.init(
                    Convert.ToDouble(numericUpDown1.Text),
                    Convert.ToDouble(numericUpDown2.Text),
                    Convert.ToDouble(numericUpDown3.Text),
                    Convert.ToDouble(numericUpDown4.Text),
                    Convert.ToDouble(numericUpDown5.Text),
                    Convert.ToDouble(numericUpDown6.Text),
                    Convert.ToDouble(numericUpDown7.Text),
                    Convert.ToDouble(numericUpDown9.Text),
                    Convert.ToDouble(numericUpDown8.Text),
                    Convert.ToDouble(numericUpDown18.Text)
                    );
            }
            catch (Exception)
            {
                MessageBox.Show("Введите корректные данные в поля!");
            }
        }
        private void getSysIntegrationData()
        {
            try
            {
                dt = Convert.ToDouble(numericUpDown12.Text);
                T = Convert.ToDouble(numericUpDown13.Text);
                eps = Convert.ToDouble(numericUpDown19.Text);
                maxGraphDot = Convert.ToInt32(T / dt + 1);
                speedGraphDot = Convert.ToInt32(numericUpDown14.Text);
                capGraphDot = Convert.ToInt32(numericUpDown15.Text);

                x_0 = Convert.ToDouble(numericUpDown17.Text);
                u_0 = Convert.ToDouble(numericUpDown16.Text);
                x1_0 = Convert.ToDouble(numericUpDown11.Text);
                u1_0 = Convert.ToDouble(numericUpDown10.Text);

                if (x_0 <= 0 || x_0 == x1_0)
                {
                    throw new Exception("x_0 > 0 || x_0 != x1_0");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Введите корректные данные в поля!");
            }
        }
        // выбор массивов точек для осей графика
        private void getSysPlotData()
        {
            graphLabel_X = comboBox3.Text.ToString();
            graphLabel_Y = comboBox4.Text.ToString();

            switch (graphLabel_X)
            {
                case "x":
                    graphData_X = x_res;
                    break;
                case "u":
                    graphData_X = u_res;
                    break;
                case "x1":
                    graphData_X = x1_res;
                    break;
                case "u1":
                    graphData_X = u1_res;
                    break;
                default:
                    graphData_X = x_res;
                    break;
            }
            switch (graphLabel_Y)
            {
                case "x":
                    graphData_Y = x_res;
                    break;
                case "u":
                    graphData_Y = u_res;
                    break;
                case "x1":
                    graphData_Y = x1_res;
                    break;
                case "u1":
                    graphData_Y = u1_res;
                    break;
                default:
                    graphData_Y = u_res;
                    break;
            }
        }

        // применить параметры системы
        private void button1_Click(object sender, EventArgs e)
        {
            getSysParamData();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            getSysIntegrationData();
            getSysPlotData();
        }

        // таймер отрисовки графика фазовых траекторий
        private void timer1_Tick(object sender, EventArgs e)
        {
            //if (curGraphDot > capGraphDot)
            //{
            //    startGraphDot += speedGraphDot;
            //}
            //if (curGraphDot + speedGraphDot >= maxGraphDot)
            //{
            //    timer1.Stop();
            //}
            //else
            //{
            //    curGraphDot += speedGraphDot;
            //    textBox1.Text = curGraphDot.ToString();
            //    DrawGraphT_Anim(X_res, Y_res);
            //}

        }
        // Остановить счет
        private void button4_Click(object sender, EventArgs e)
        {
            //if (button4.Text == "Стоп")
            //{
            //    timer1.Stop();
            //    button4.Text = "Продолжить";
            //}
            //else
            //{
            //    timer1.Start();
            //    button4.Text = "Стоп";
            //}
        }

        // график осцилограммы
        private void drawGraph_Oscilogramm()
        {
            getSysPlotData();

            graphPane_Oscilogramm.CurveList.Clear();
            PointPairList pointList_x = new PointPairList();
            PointPairList pointList_x1 = new PointPairList();

            int k = 0;
            foreach (double r in graphData_X)
            {
                pointList_x.Add(t_res[k], r);
                k++;
            }
            k = 0;
            foreach (double r in graphData_Y)
            {
                pointList_x1.Add(t_res[k], r);
                k++;
            }
            LineItem curve_x = graphPane_Oscilogramm.AddCurve(graphLabel_X, pointList_x, Color.Red, SymbolType.None);
            LineItem curve_x1 = graphPane_Oscilogramm.AddCurve(graphLabel_Y, pointList_x1, Color.Blue, SymbolType.None);

            graphPane_Oscilogramm.XAxis.Title.Text = "Время";
            graphPane_Oscilogramm.YAxis.Title.Text = "Координаты";
            graphPane_Oscilogramm.Title.Text = "Осцилограммы движения";
            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();
        }
        // старт осцилограммы движения
        private void button6_Click(object sender, EventArgs e)
        {
            getSysIntegrationData();
            getSysParamData();
            curGraphDot = 0;
            startGraphDot = 0;

            sysDE.setParam(sysParam);
            sysDE.solveDiffs(new List<double> { x_0, u_0 }, new List<double> { x1_0, u1_0 }, dt, T, eps);
            t_res = sysDE.de1.time.Count > sysDE.de2.time.Count ? sysDE.de1.time : sysDE.de2.time;
            x_res = sysDE.de1.getResult(0);
            u_res = sysDE.de1.getResult(1);
            x1_res = sysDE.de2.getResult(0);
            u1_res = sysDE.de2.getResult(1);

            drawGraph_Oscilogramm();
        }

        // график фазовых траекторий 
        private void drawGraph_Phase_Anim(double[] dotsX, double[] dotsY)
        {
            //paneT.CurveList.Clear();
            //PointPairList list = new PointPairList();

            //for (int k = startGraphDot; k < curGraphDot; k += stepGraphDot)
            //{
            //    if (k > stepGraphDot)
            //    {
            //        if (dotsX[k] > Math.PI)
            //        {
            //            dotsX[k] = dotsX[k] % (2 * Math.PI) - 2 * Math.PI;
            //        }
            //        if (dotsX[k] < -Math.PI)
            //        {
            //            dotsX[k] = dotsX[k] % (-2 * Math.PI) + 2 * Math.PI;
            //        }

            //        if (Math.Abs(dotsX[k - stepGraphDot] - dotsX[k]) <= Math.PI - 1)
            //            list.Add(dotsX[k], dotsY[k]);
            //        else
            //            list.Add(PointPairBase.Missing, PointPairBase.Missing);
            //    }
            //}
            //LineItem myCurve = paneT.AddCurve("Фазовая траектория", list, Color.Blue, SymbolType.None);
            //paneT.XAxis.Scale.Min = -Math.PI;
            //paneT.XAxis.Scale.Max = Math.PI;
            //paneT.YAxis.Scale.MaxAuto = true;
            //paneT.YAxis.Scale.MinAuto = true;
            //zedGraphControl1.AxisChange();
            //zedGraphControl1.Invalidate();
        }
        private void drawGraph_Phase()
        {
            getSysPlotData();

            graphPane_Phase.CurveList.Clear();
            PointPairList pointList = new PointPairList();

            //for (int k = maxGraphDot - capGraphDot; k < maxGraphDot; k += stepGraphDot)
            //{
            //    if (k > stepGraphDot)
            //    {
            //        if (dotsX[k] > Math.PI)
            //        {
            //            dotsX[k] = dotsX[k] % (2 * Math.PI) - 2 * Math.PI;
            //        }
            //        if (dotsX[k] < -Math.PI)
            //        {
            //            dotsX[k] = dotsX[k] % (-2 * Math.PI) + 2 * Math.PI;
            //        }

            //        if (Math.Abs(dotsX[k - stepGraphDot] - dotsX[k]) <= Math.PI - 1)
            //            listT.Add(dotsX[k], dotsY[k]);
            //        else
            //            listT.Add(PointPairBase.Missing, PointPairBase.Missing);

            //    }
            //}

            int k = 0;
            foreach (double t in t_res)
            {
                pointList.Add(graphData_X[k], graphData_Y[k]);
                k++;
            }
            LineItem curve = graphPane_Phase.AddCurve("Фазовая траектория", pointList, Color.Red, SymbolType.None);

            graphPane_Phase.XAxis.Title.Text = graphLabel_X;
            graphPane_Phase.YAxis.Title.Text = graphLabel_Y;
            graphPane_Phase.Title.Text = "Фазовые траектории";
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }
        // старт фазовых траекторий
        private void button3_Click(object sender, EventArgs e)
        {
            getSysIntegrationData();
            getSysParamData();
            curGraphDot = 0;
            startGraphDot = 0;

            sysDE.setParam(sysParam);
            sysDE.solveDiffs(new List<double> { x_0, u_0 }, new List<double> { x1_0, u1_0 }, dt, T, eps);
            t_res = sysDE.de1.time.Count > sysDE.de2.time.Count ? sysDE.de1.time : sysDE.de2.time;
            x_res = sysDE.de1.getResult(0);
            u_res = sysDE.de1.getResult(1);
            x1_res = sysDE.de2.getResult(0);
            u1_res = sysDE.de2.getResult(1);

            drawGraph_Phase();

            // анимация
            //if (!checkBox1.Checked)
            //{
            //    DrawGraphT(X_res, Y_res);
            //}
            //else
            //{
            //    timer1.Start();
            //}

        }

        // график бифуркационных диаграмм
        private void drawGraph_Bifurcation()
        {
            getSysPlotData();

            graphPane_Bifurcation.CurveList.Clear();
            PointPairList pointList = new PointPairList();

            int k = 0;
            foreach (double p in t_res)
            {
                pointList.Add(p, graphData_Y[k]);
                k++;
            }
            LineItem curve = graphPane_Bifurcation.AddCurve("Бифуркационная диаграмма", pointList, Color.Black, SymbolType.Square);
            curve.Line.IsVisible = false;
            curve.Symbol.Fill.Color = Color.Black;
            curve.Symbol.Fill.Type = FillType.Solid;
            curve.Symbol.Size = 2;

            graphPane_Bifurcation.XAxis.Title.Text = graphLabel_X;
            graphPane_Bifurcation.YAxis.Title.Text = graphLabel_Y;
            graphPane_Bifurcation.Title.Text = "Бифуркационные диаграммы";
            zedGraphControl3.AxisChange();
            zedGraphControl3.Invalidate();
        }
        // старт бифуркационных диаграмм
        private void button8_Click(object sender, EventArgs e)
        {
            getSysIntegrationData();
            getSysParamData();
            curGraphDot = 0;
            startGraphDot = 0;

            string bifParamName = comboBox5.Text.ToString();
            double p_start = Convert.ToDouble(numericUpDown20.Text);
            double p_end = Convert.ToDouble(numericUpDown21.Text);
            double p_delta = Convert.ToDouble(numericUpDown22.Text);
            int hitsCount = Convert.ToInt32(numericUpDown23.Text);
            t_res = new List<double>();
            x_res = new List<double>();
            u_res = new List<double>();
            x1_res = new List<double>();
            u1_res = new List<double>();

            for (double p = p_start; p < p_end; p = p + p_delta)
            {
                sysParam.setParam(bifParamName, p);
                sysDE.setParam(sysParam);
                sysDE.solveBifurcation(new List<double> { x_0, u_0 }, new List<double> { x1_0, u1_0 }, dt, T, eps, hitsCount);

                int ptc = sysDE.de1.time.Count > sysDE.de2.time.Count ? sysDE.de1.time.Count : sysDE.de2.time.Count;
                List<double> ptmp = new List<double>(ptc);
                for (int i = 0; i < ptc; i++) ptmp.Add(p);
                t_res.AddRange(ptmp);
                x_res.AddRange(sysDE.de1.getResult(0));
                u_res.AddRange(sysDE.de1.getResult(1));
                x1_res.AddRange(sysDE.de2.getResult(0));
                u1_res.AddRange(sysDE.de2.getResult(1));
            }

            drawGraph_Bifurcation();
        }

    }
}
