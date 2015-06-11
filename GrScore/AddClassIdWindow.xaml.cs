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
using System.Collections.ObjectModel;
using System.Xml;
using System.IO;

namespace GrScore
{
    /// <summary>
    /// AddClassIdWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddClassIdWindow : Window
    {
        private ObservableCollection<KeyValuePair<string, string>> _classes;

        public AddClassIdWindow(ObservableCollection<KeyValuePair<string, string>> classes)
        {
            InitializeComponent();
            _classes = classes;
            classlist.ItemsSource = classes;
            //foreach(var v in classes)
            //{
            //    classlist.Items.Add(v);
            //}
        }

        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(classId.Text) || string.IsNullOrEmpty(className.Text))
            {
                MessageBox.Show("请先填写完整!");
            }
            else
            {
                _classes.Add(new KeyValuePair<string, string>(classId.Text, className.Text));
                //写入xml文件
                XmlDocument doc = new XmlDocument();
                string path = Directory.GetCurrentDirectory() + "\\classes.xml";
                if (File.Exists(path))
                {
                    doc.Load(path);
                    XmlNode newElem = doc.CreateNode("element", "class", "");
                    newElem.InnerText = className.Text;
                    XmlAttribute att = doc.CreateAttribute("id");
                    att.Value = classId.Text;
                    newElem.Attributes.Append(att);
                    XmlElement root = doc.DocumentElement;
                    root.AppendChild(newElem);
                    doc.Save(path);
                }                
            }
        }


        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO 删除选定项目
            MessageBox.Show("功能开发中");
        }
    }
}
