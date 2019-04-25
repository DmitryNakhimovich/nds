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
        GraphPane paneT;
        GraphPane paneD;
        // объект системы
        SystemDE sysObj;
        // параметры системы
        double h = 0.05;
        double h1 = 0.05;
        double lambda = 0.6;
        double p = 0.018;
        double mu0 = 5;
        double mu = 0.1;
        double R = 0.3;
        double E = 0.018;
        double phi = 0.6;
        double gamma = 4;
        SystemParams sysParam = new SystemParams();
        // начальные условия
        double x_0;
        double u_0;
        double x1_0;
        double u1_0;
        // вычисленные точки
        double[] x_res; // x
        double[] u_res; // x'
        double[] x1_res; // x1
        double[] u1_res; // x1'
        // массив координат для графиков
        double[] X_res; // X-axis
        double[] Y_res; // Y-axis
        string x_name = "x", y_name = "u";
        // параметры интегрирования
        double curMouseX, curMouseY; // положение мыши
        double dt; // шаг интегрирования
        double T; // время интерирования 
        double eps = 0.00001;
        // параметры анимации        
        int maxGraphDot = 0; // максимальное число точек T/dt
        int capGraphDot = 10000; // порог отображаемых точек на графике
        int startGraphDot = 0; // начало отрисовки анимации
        int stepGraphDot = 10; // шаг выборки из результатов
        int speedGraphDot = 100; // скорость отрисовки анимации
        int curGraphDot = 0; // текущая точка анимации     

        public Form1()
        {
            InitializeComponent();
            sysParam.init(h, h1, lambda, p, mu0, mu, R, E, phi, gamma);
            sysObj = new SystemDE(sysParam, 2);

            paneT = zedGraphControl1.GraphPane;
            paneT.XAxis.Title.Text = "X";
            paneT.YAxis.Title.Text = "Y";
            paneT.Title.Text = "";
            paneT.XAxis.Scale.Min = -Math.PI;
            paneT.XAxis.Scale.Max = Math.PI;

            paneD = zedGraphControl2.GraphPane;
            paneD.XAxis.Title.Text = "X";
            paneD.YAxis.Title.Text = "Y";
            paneD.Title.Text = "";
            paneD.XAxis.Scale.Min = -Math.PI;
            paneD.XAxis.Scale.Max = Math.PI;
        }

        // Инициализировать параметры системы из формы
        private void get_sysParam()
        {
            try
            {
                h = Convert.ToDouble(numericUpDown1.Text);
                h1 = Convert.ToDouble(numericUpDown2.Text);
                lambda = Convert.ToDouble(numericUpDown3.Text);
                p = Convert.ToDouble(numericUpDown4.Text);
                mu0 = Convert.ToDouble(numericUpDown5.Text);
                mu = Convert.ToDouble(numericUpDown6.Text);
                R = Convert.ToDouble(numericUpDown7.Text);
                E = Convert.ToDouble(numericUpDown9.Text);
                phi = Convert.ToDouble(numericUpDown8.Text);
                gamma = Convert.ToDouble(numericUpDown18.Text);

                sysParam.init(h, h1, lambda, p, mu0, mu, R, E, phi, gamma);

                //x_name = comboBox2.SelectedValue.ToString();
                //y_name = comboBox1.SelectedValue.ToString();
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите корректные данные в поля!");
            }
        }
        private void get_sysIntegr()
        {
            try
            {
                dt = Convert.ToDouble(numericUpDown12.Text);
                T = Convert.ToDouble(numericUpDown13.Text);
                maxGraphDot = Convert.ToInt32(T / dt);
                speedGraphDot = Convert.ToInt32(numericUpDown14.Text);
                capGraphDot = Convert.ToInt32(numericUpDown15.Text);

                x_0 = Convert.ToDouble(numericUpDown17.Text);
                u_0 = Convert.ToDouble(numericUpDown16.Text);
                x1_0 = Convert.ToDouble(numericUpDown11.Text);
                u1_0 = Convert.ToDouble(numericUpDown10.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите корректные данные в поля!");
            }
        }
        private void get_sysPlotParam()
        {
            x_name = comboBox3.Text.ToString();
            y_name = comboBox4.Text.ToString();
        }

        // применить параметры системы
        private void button1_Click(object sender, EventArgs e)
        {
            get_sysParam();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            get_sysIntegr();
        }

        // таймер отрисовки графика фазовых траекторий
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (curGraphDot > capGraphDot)
            {
                startGraphDot += speedGraphDot;
            }
            if (curGraphDot + speedGraphDot >= maxGraphDot)
            {
                timer1.Stop();
            }
            else
            {
                curGraphDot += speedGraphDot;
                textBox1.Text = curGraphDot.ToString();
                DrawGraphT_Anim(X_res, Y_res);
            }

        }

        // выбор массивов точек для осей графика
        private void get_sysPlotName()
        {
            switch (x_name)
            {
                case "x":
                    X_res = x_res;
                    break;
                case "u":
                    X_res = u_res;
                    break;
                case "x1":
                    X_res = x1_res;
                    break;
                case "u1":
                    X_res = u1_res;
                    break;
                default:
                    X_res = x_res;
                    break;
            }
            switch (y_name)
            {
                case "x":
                    Y_res = x_res;
                    break;
                case "u":
                    Y_res = u_res;
                    break;
                case "x1":
                    Y_res = x1_res;
                    break;
                case "u1":
                    Y_res = u1_res;
                    break;
                default:
                    Y_res = u_res;
                    break;
            }
        }

        // рисование графика фазовых траекторий 
        private void DrawGraphT_Anim(double[] dotsX, double[] dotsY)
        {
            paneT.CurveList.Clear();
            PointPairList list = new PointPairList();

            for (int k = startGraphDot; k < curGraphDot; k += stepGraphDot)
            {
                if (k > stepGraphDot)
                {
                    if (dotsX[k] > Math.PI)
                    {
                        dotsX[k] = dotsX[k] % (2 * Math.PI) - 2 * Math.PI;
                    }
                    if (dotsX[k] < -Math.PI)
                    {
                        dotsX[k] = dotsX[k] % (-2 * Math.PI) + 2 * Math.PI;
                    }

                    if (Math.Abs(dotsX[k - stepGraphDot] - dotsX[k]) <= Math.PI - 1)
                        list.Add(dotsX[k], dotsY[k]);
                    else
                        list.Add(PointPairBase.Missing, PointPairBase.Missing);
                }
            }
            LineItem myCurve = paneT.AddCurve("Фазовая траектория", list, Color.Blue, SymbolType.None);
            paneT.XAxis.Scale.Min = -Math.PI;
            paneT.XAxis.Scale.Max = Math.PI;
            paneT.YAxis.Scale.MaxAuto = true;
            paneT.YAxis.Scale.MinAuto = true;
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }
        private void DrawGraphT(double[] dotsX, double[] dotsY)
        {
            paneT.CurveList.Clear();
            PointPairList listT = new PointPairList();

            for (int k = maxGraphDot - capGraphDot; k < maxGraphDot; k += stepGraphDot)
            {
                if (k > stepGraphDot)
                {
                    if (dotsX[k] > Math.PI)
                    {
                        dotsX[k] = dotsX[k] % (2 * Math.PI) - 2 * Math.PI;
                    }
                    if (dotsX[k] < -Math.PI)
                    {
                        dotsX[k] = dotsX[k] % (-2 * Math.PI) + 2 * Math.PI;
                    }

                    if (Math.Abs(dotsX[k - stepGraphDot] - dotsX[k]) <= Math.PI - 1)
                        listT.Add(dotsX[k], dotsY[k]);
                    else
                        listT.Add(PointPairBase.Missing, PointPairBase.Missing);

                }
            }
            LineItem myCurve = paneT.AddCurve("Фазовая траектория", listT, Color.Red, SymbolType.None);
            paneT.XAxis.Scale.Min = -Math.PI;
            paneT.XAxis.Scale.Max = Math.PI;
            paneT.YAxis.Scale.MaxAuto = true;
            paneT.YAxis.Scale.MinAuto = true;
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        // отрисовка осцилограммы
        private void DrawGraphD(double[] dotsX)
        {
            paneD.CurveList.Clear();
            PointPairList listD = new PointPairList();

            for (int k = maxGraphDot - capGraphDot; k < maxGraphDot; k += stepGraphDot)
            {
                if (k > stepGraphDot)
                {


                    if (Math.Abs(dotsX[k - stepGraphDot] - dotsX[k]) <= Math.PI - 1)
                        listD.Add(k, dotsX[k]);
                    else
                        listD.Add(PointPairBase.Missing, PointPairBase.Missing);

                }
            }
            LineItem myCurve = paneD.AddCurve("Осцилограммы движения", listD, Color.Red, SymbolType.None);
            paneD.XAxis.Scale.Min = -Math.PI;
            paneD.XAxis.Scale.Max = Math.PI;
            paneD.YAxis.Scale.MaxAuto = true;
            paneD.YAxis.Scale.MinAuto = true;
            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();
        }

        // расчет осцилограммы движенияs
        private void button6_Click(object sender, EventArgs e)
        {
            get_sysIntegr();
            get_sysParam();
            get_sysPlotParam();
            double[] initCond = { x_0, u_0, x1_0, u1_0 };
            curGraphDot = 0;
            startGraphDot = 0;
            sysObj.setParam(sysParam);

            sysObj.solveDiffs(new List<double> { x_0, u_0 }, new List<double> { x1_0, u1_0 }, dt, T, eps);
            x_res = sysObj.de1.getResult(0).ToArray();
            u_res = sysObj.de1.getResult(1).ToArray();
            x1_res = sysObj.de2.getResult(0).ToArray();
            u1_res = sysObj.de2.getResult(1).ToArray();

            get_sysPlotName();
            DrawGraphD(X_res);

        }

        // расчет фазовых траекторий
        private void button3_Click(object sender, EventArgs e)
        {
            //get_sysIntegr();
            //get_sysParam();
            //get_sysPlotParam();
            //double[] initCond = { x_0, u_0, x1_0, u1_0 };
            //curGraphDot = 0;
            //startGraphDot = 0;
            //sysObj.SetParam(sysParam);

            //sysObj.SolveDiffs(initCond, dt, T);
            //x_res = sysObj.GetRes(0);              
            //u_res = sysObj.GetRes(1);
            //x1_res = sysObj.GetRes(2);
            //u1_res = sysObj.GetRes(3);    

            //get_sysPlotName(); 
            //// без анимации
            //if (!checkBox1.Checked)
            //{
            //    DrawGraphT(X_res, Y_res);
            //}
            //else
            //{
            //    timer1.Start();
            //}

        }

        // Остановить счет
        private void button4_Click(object sender, EventArgs e)
        {
            if (button4.Text == "Стоп")
            {
                timer1.Stop();
                button4.Text = "Продолжить";
            }
            else
            {
                timer1.Start();
                button4.Text = "Стоп";
            }
        }

    }
}
