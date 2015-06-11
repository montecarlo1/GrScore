using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GrScore
{
    /// <summary>
    /// Calculator.xaml 的交互逻辑
    /// </summary>
    public partial class Calculator : Window
    {
        public Calculator()
        {
            InitializeComponent();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(m1Percentage.Text) ||
                string.IsNullOrEmpty(m2Percentage.Text) ||
                string.IsNullOrEmpty(m3Percentage.Text) ||
                string.IsNullOrEmpty(m4Percentage.Text) ||
                string.IsNullOrEmpty(m1Score.Text) ||
                string.IsNullOrEmpty(m2Score.Text) ||
                string.IsNullOrEmpty(m3Score.Text) ||
                string.IsNullOrEmpty(m4Score.Text))
            {
                MessageBox.Show("请先输入数字");
                return;
            }
            double resultScore =
            (double.Parse(m1Percentage.Text) * double.Parse(m1Score.Text)
                + double.Parse(m2Percentage.Text) * double.Parse(m2Score.Text)
                + double.Parse(m3Percentage.Text) * double.Parse(m3Score.Text)
                + double.Parse(m4Percentage.Text) * double.Parse(m4Score.Text)) / 100.0;
            result.Text = resultScore.ToString("F2");
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            m1Percentage.Text = "";
            m2Percentage.Text = "";
            m3Percentage.Text = "";
            m4Percentage.Text = "";
            m1Score.Text = "";
            m2Score.Text = "";
            m3Score.Text = "";
            m4Score.Text = "";
            result.Text = "";
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
