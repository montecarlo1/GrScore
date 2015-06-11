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
    /// RegisterWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            checkCode(registerCodeText.Text);
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void checkCode(string code)
        { 
            //获取MAC地址
            string macAddr = "";
            //获取C盘硬盘号
            string diskid = "";

        }

        private void mailLink_Click(object sender, RoutedEventArgs e)
        {
             try    
             {
                 System.Diagnostics.Process.Start(mailLink.NavigateUri.ToString()); 
             }  
             catch 
             {
                
             }
        }

        private void tryBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
