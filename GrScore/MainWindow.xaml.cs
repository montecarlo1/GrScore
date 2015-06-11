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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using Visifire.Charts;
using System.Data;
using SqliteORM;
using System.Windows.Shell;

namespace GrScore
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public enum QueryMode
        {
            SiglePersonSigleClass,
            SiglePersonMultiClass,
            MultiPersonSingleClass,
            加权平均分
        }

        private ObservableCollection<ScoreModel> _classScores;

        private string baseUrl = null;
        private string querypara = null;
        private ObservableCollection<KeyValuePair<string, string>> classes = new ObservableCollection<KeyValuePair<string, string>>();
        private QueryMode _currentQueryMode = QueryMode.SiglePersonSigleClass;
        //查询过的stuid
        private ObservableCollection<KeyValuePair<string, string>> savedStuIds = new ObservableCollection<KeyValuePair<string, string>>();
        private string stuName = "";

        #region 命令
        private ICommand _CalculatorCommand;
        public ICommand CalculatorCommand
        {
            get
            {
                if (_CalculatorCommand == null)
                { 
                    _CalculatorCommand = new RelayCommand(
                        param =>{
                            Calculator ca = new Calculator();
                            ca.Show();
                        },
                        (e) =>
                        {
                            return true;
                        });
                }
                return _CalculatorCommand;
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;            
            //课程下拉列表更新
            classes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(classes_CollectionChanged);
            savedStuIds.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(savedStuIds_CollectionChanged);
            ClassScores.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ClassScores_CollectionChanged);
            //获取查询地址
            IniFile inifile = new IniFile(Directory.GetCurrentDirectory() + "\\config.ini");
            var keys = inifile.ReadKeys("QUERY");
            baseUrl = inifile.IniReadValue("QUERY", "baseurl");
            querypara = inifile.IniReadValue("QUERY", "param");

            //获取xml课程列表
            XmlDocument doc = new XmlDocument();
            string path = Directory.GetCurrentDirectory() + "\\classes.xml";
            if (File.Exists(path))
            {
                doc.Load(path);
                XmlNodeList lis = doc.GetElementsByTagName("class");
                for (int i = 0; i < lis.Count; i++)
                {
                    classes.Add(new KeyValuePair<string, string>(lis[i].Attributes["id"].Value, lis[i].InnerText));   
                }                
            }
            else
            {
                MessageBox.Show("没有课程,请先添加课程","提示",MessageBoxButton.OK,MessageBoxImage.Information);
                AddClassMap_Click(null, null);
            }

            //获取stuid
            XmlDocument doc1 = new XmlDocument();
            string path1 = Directory.GetCurrentDirectory() + "\\stuid.xml";
            if (File.Exists(path1))
            {
                doc.Load(path1);
                XmlNodeList lis = doc.GetElementsByTagName("stu");
                for (int i = 0; i < (lis.Count<10?lis.Count:10); i++)
                {
                    SavedStuIds.Add(new KeyValuePair<string, string>(lis[i].Attributes["id"].Value, lis[i].InnerText));
                }
            }              
        }

        void ClassScores_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChange("ClassScores");
        }
        
        /// <summary>
        /// 加入新的stuid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void savedStuIds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //写入xml文件
            XmlDocument doc = new XmlDocument();
            string path = Directory.GetCurrentDirectory() + "\\stuid.xml";
            if (File.Exists(path))
            {
                if (!string.IsNullOrEmpty(stuName) && !string.IsNullOrEmpty(stuidTextBox.Text))
                {                    
                    doc.Load(path);

                    //XmlNodeList lis = doc.GetElementsByTagName("stu");
                    //for (int i = 0; i < lis.Count; i++)
                    //{
                    //    if (!string.Equals(lis[i].InnerText,stuName))
                    //    {
                           
                    //    }                        
                    //}

                    XmlNode newElem = doc.CreateNode("element", "stu", "");
                    newElem.InnerText = stuName;
                    XmlAttribute att = doc.CreateAttribute("id");
                    att.Value = stuidTextBox.Text;
                    newElem.Attributes.Append(att);
                    XmlElement root = doc.DocumentElement;
                    root.AppendChild(newElem);
                    doc.Save(path);
                }
            }
            NotifyPropertyChange("ClassList");
        }

        /// <summary>
        /// 下拉列表更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void classes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {            
            NotifyPropertyChange("ClassList");
        }

        /// <summary>
        /// 查询模式
        /// </summary>
        public QueryMode CurrentQueryMode
        {
            set
            {
                _currentQueryMode = value;
                switch (_currentQueryMode)
                {
                    case QueryMode.SiglePersonSigleClass:
                        stuidControl.Visibility = Visibility.Visible;
                        multiStuIdTextBox.Visibility = Visibility.Collapsed;
                        showChartBtn.IsEnabled = false;
                        break;
                    case QueryMode.SiglePersonMultiClass:
                        stuidControl.Visibility = Visibility.Visible;
                        multiStuIdTextBox.Visibility = Visibility.Collapsed;
                        showChartBtn.IsEnabled = false;
                        break;
                    case QueryMode.MultiPersonSingleClass:
                        stuidControl.Visibility = Visibility.Collapsed;
                        multiStuIdTextBox.Visibility = Visibility.Visible;
                        showChartBtn.IsEnabled = true;
                        break;
                    case QueryMode.加权平均分:
                        stuidControl.Visibility = Visibility.Collapsed;
                        multiStuIdTextBox.Visibility = Visibility.Visible;
                        showChartBtn.IsEnabled = true;
                        break;
                    default:
                        break;
                }
                NotifyPropertyChange("CurrentQueryMode");
            }
            get
            {
                return _currentQueryMode;
            }
        }

        /// <summary>
        /// 成绩列表
        /// </summary>
        public ObservableCollection<ScoreModel> ClassScores
        {
            get
            {
                if (_classScores == null)
                {
                    _classScores = new ObservableCollection<ScoreModel>();
                }
                return _classScores;
            }
            set
            {                
                _classScores = value;
                NotifyPropertyChange("ClassScores");
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> SavedStuIds
        {
            get
            {
                if (savedStuIds == null)
                {
                    savedStuIds = new ObservableCollection<KeyValuePair<string, string>>();
                }
                return savedStuIds;
            }
            set
            {
                savedStuIds = value;
                NotifyPropertyChange("SavedStuIds");
            }
        }

        public IEnumerable<string> ClassList
        {
            get 
            {
                var classNames = from t in classes select t.Value;
                return classNames;
            }
        }
        
        /// <summary>
        /// 查询/刷新按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryBtn_Click(object sender, RoutedEventArgs e)
        {
            ClassScores.Clear();
            initWorker(CurrentQueryMode);
            return;
            
            Action action = new Action(() =>
            {
                System.Threading.Thread.Sleep(50);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        ClassScores.Clear();
                        string q_url = baseUrl;
                        string stuid = stuidTextBox.Text;// "0172";
                        //单人单科
                        if (CurrentQueryMode == QueryMode.SiglePersonSigleClass)
                        {
                            if (!string.IsNullOrEmpty(stuid))
                            {
                                string q_para = querypara.Replace("$stuid$", stuid).
                                                Replace("#classid#", classes[selectedClass.SelectedIndex].Key);
                                //string q_para = "studentid=xdleess20130621zq0172&degreecourseno=0022001";
                                string q_result = Helper.HttpGet(q_url, q_para);
                                //ResultTextBox.Text += q_result;
                                var scoremodel = Helper.AnalyseHtmlResult(q_result, stuid);
                                ClassScores.Add(scoremodel);
                                //if (!savedStuIds.Contains(new KeyValuePair<string, string>(stuid, stuName = scoremodel.StudentName)))
                                //{
                                //    savedStuIds.Add(new KeyValuePair<string, string>(stuid, stuName = scoremodel.StudentName));
                                //}
                            }
                            else
                            {
                                MessageBox.Show("先输入学生id");
                            }
                        }
                        //单人多科
                        else if (CurrentQueryMode == QueryMode.SiglePersonMultiClass)
                        {
                            string q_para;
                            ScoreModel scoremodel = null;
                            foreach (var v in classes)
                            {
                                q_para = querypara.Replace("$stuid$", stuid).
                                            Replace("#classid#", v.Key);
                                string q_result = Helper.HttpGet(q_url, q_para);
                                scoremodel = Helper.AnalyseHtmlResult(q_result, stuid);
                                ClassScores.Add(scoremodel);
                            }
                            //if (!savedStuIds.Contains(new KeyValuePair<string, string>(stuid, stuName = scoremodel.StudentName)))
                            //{
                            //    savedStuIds.Add(new KeyValuePair<string, string>(stuid, stuName = scoremodel.StudentName));
                            //}
                        }
                        //多人单科
                        else if (CurrentQueryMode == QueryMode.MultiPersonSingleClass)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(stuid_start_TextBox.Text) && !string.IsNullOrEmpty(stuid_end_TextBox.Text))
                                {
                                    for (int i = 0; i < (int.Parse(stuid_end_TextBox.Text) - int.Parse(stuid_start_TextBox.Text)) + 1; i++)
                                    {
                                        string q_para = querypara.Replace("$stuid$", (int.Parse(stuid_start_TextBox.Text) + i).ToString("D4")).
                                                        Replace("#classid#", classes[selectedClass.SelectedIndex].Key);
                                        //string q_para = "studentid=xdleess20130621zq0172&degreecourseno=0022001";
                                        string q_result = Helper.HttpGet(q_url, q_para);
                                        //ResultTextBox.Text += q_result;
                                        var scoremodel = Helper.AnalyseHtmlResult(q_result, (int.Parse(stuid_start_TextBox.Text) + i).ToString("D4"));
                                        ClassScores.Add(scoremodel);
                                        //if (!savedStuIds.Contains(new KeyValuePair<string, string>(stuid,scoremodel.StudentName)))
                                        //{
                                        //    savedStuIds.Add(new KeyValuePair<string, string>(stuid, stuName = scoremodel.StudentName));
                                        //}
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (CurrentQueryMode == QueryMode.加权平均分)
                        {
                            MessageBox.Show("加权");
                        }
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show(e1.Message);
                    }
                }), System.Windows.Threading.DispatcherPriority.SystemIdle, null);
            });
            action.BeginInvoke(null, null);            
        }

        #region 异步任务
        BackgroundWorker worker;

        private class WorkerArgs
        {
            public QueryMode QueryMode { set; get; }
            public string Url { set; get; }
            public string StudentId { set; get; }//学生Id
            public string StudentName { set; get; }//姓名
            public string StudentNumber { set; get; }//学号
            public string StudentStartId { set; get; }
            public string StudentEndId { set; get; }
            public bool isStudentNoRangeValid = false;
            public string StudentStartNo { set; get; }
            public string StudentEndNo { set; get; }
            public int SelectedClassIndex { set; get; }
        }

        /// <summary>
        /// 初始化异步任务
        /// </summary>
        private void initWorker(QueryMode queryMode)
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            WorkerArgs args = new WorkerArgs() { Url = baseUrl ,SelectedClassIndex = selectedClass.SelectedIndex};
            if (queryMode == QueryMode.SiglePersonSigleClass)
            {
                if (string.IsNullOrEmpty(stuidTextBox.Text) && string.IsNullOrEmpty(stuNameTextBox.Text))
                {
                    MessageBox.Show("先输入学生id或者姓名");
                    return;
                }
                if(!string.IsNullOrEmpty(stuidTextBox.Text))
                {
                    args.StudentId = stuidTextBox.Text;//学生id
                }
                if (!string.IsNullOrEmpty(stuNameTextBox.Text))
                {
                    args.StudentName = stuNameTextBox.Text;//学生姓名
                }

                worker.DoWork += new DoWorkEventHandler(worker_DoWork_单人单科);
            }
            else if (queryMode == QueryMode.SiglePersonMultiClass)
            {
                if (string.IsNullOrEmpty(stuidTextBox.Text) && string.IsNullOrEmpty(stuNameTextBox.Text))
                {
                    MessageBox.Show("先输入学生id或者姓名");
                    return;
                }
                if (!string.IsNullOrEmpty(stuidTextBox.Text))
                {
                    args.StudentId = stuidTextBox.Text;//学生id
                }
                if (!string.IsNullOrEmpty(stuNameTextBox.Text))
                {
                    args.StudentName = stuNameTextBox.Text;//学生姓名
                }

                worker.DoWork += new DoWorkEventHandler(worker_DoWork_单人多科);
            }
            else if (queryMode == QueryMode.MultiPersonSingleClass)
            {
                if (!string.IsNullOrEmpty(stuNo_start_TextBox.Text) && !string.IsNullOrEmpty(stuNo_end_TextBox.Text))
                {
                    args.StudentStartNo = stuNo_start_TextBox.Text;
                    args.StudentEndNo = stuNo_end_TextBox.Text;
                    args.isStudentNoRangeValid = true;
                }
                else if (!string.IsNullOrEmpty(stuid_start_TextBox.Text) && !string.IsNullOrEmpty(stuid_end_TextBox.Text))
                {
                    args.StudentStartId = stuid_start_TextBox.Text;
                    args.StudentEndId = stuid_end_TextBox.Text;
                    args.isStudentNoRangeValid = false;
                }
                else
                {
                    MessageBox.Show("先输入学号范围或者学生id范围");
                    return;
                }

                worker.DoWork += new DoWorkEventHandler(worker_DoWork_多人单科);
            }
            else if (queryMode == QueryMode.加权平均分)
            {
                if (!string.IsNullOrEmpty(stuNo_start_TextBox.Text) && !string.IsNullOrEmpty(stuNo_end_TextBox.Text))
                {
                    args.StudentStartNo = stuNo_start_TextBox.Text;
                    args.StudentEndNo = stuNo_end_TextBox.Text;
                    args.isStudentNoRangeValid = true;
                }
                else if (!string.IsNullOrEmpty(stuid_start_TextBox.Text) && !string.IsNullOrEmpty(stuid_end_TextBox.Text))
                {
                    args.StudentStartId = stuid_start_TextBox.Text;
                    args.StudentEndId = stuid_end_TextBox.Text;
                    args.isStudentNoRangeValid = false;
                }
                else
                {
                    MessageBox.Show("先输入学号范围或者学生id范围");
                    return;
                }

                worker.DoWork += new DoWorkEventHandler(worker_DoWork_加权平均);
            }
            args.QueryMode = queryMode;
            worker.RunWorkerAsync(args);
            QueryBtn.IsEnabled = false;
            QueryBtn.Content = "正在查询...";
        }

        ScoreModel tempModel;
        /// <summary>
        /// 进度改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            tempModel = e.UserState as ScoreModel;
            if (e.UserState != null)
            {
                ClassScores.Add(tempModel);
            }

            processValue.Text = e.ProgressPercentage.ToString();
            progressbar.Value = e.ProgressPercentage;
            TaskbarItemInfo.ProgressValue = (double)e.ProgressPercentage / 100;

            if (tempModel != null)
            {
                if (CurrentQueryMode == QueryMode.加权平均分 || CurrentQueryMode == QueryMode.MultiPersonSingleClass)
                {
                    //计算均分
                    avaScoreAll.Text = (((double.Parse(avaScoreAll.Text) * (ClassScores.Count - 1)) + double.Parse(tempModel.Score))
                        / ClassScores.Count).ToString("F2");
                }
            }
        }

        /// <summary>
        /// 执行完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            QueryBtn.IsEnabled = true;
            QueryBtn.Content = "查询/刷新";
            if (CurrentQueryMode == QueryMode.SiglePersonMultiClass)
            { 
                //计算加权均分(除去不计算加权的课程)
                var list = ClassScores.Where(t =>(!string.IsNullOrEmpty(t.Score) && (!string.IsNullOrEmpty(t.CreditGet) && isValidNumber(t.CreditGet))));//学分不为空的
                double totalCredit = list.Select(t => double.Parse(t.CreditGet)).Sum();
                if (totalCredit != 0)
                {
                    avaScore.Text = (list.Select(t => double.Parse(t.Score == "" ? "0.0" : t.Score) * double.Parse(t.CreditGet)).Sum() / totalCredit).ToString("F2");
                }
            }
        }

        private bool isValidNumber(string num)
        {
            double result = -1.0;
            double.TryParse(num, out result);
            if (result == -1.0 || result == 0.0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 任务处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_DoWork_单人单科(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(10);
            WorkerArgs args = e.Argument as WorkerArgs;
            try
            {
                if (string.IsNullOrEmpty(args.StudentId) && !string.IsNullOrEmpty(args.StudentName))
                {
                    //姓名不为空，直接查询Id
                    using (TableAdapter<Student> adapter = TableAdapter<Student>.Open())
                    {
                        var ret = adapter.Select().Where(Where.Equal("Name", args.StudentName));
                        if (ret != null && ret.Count() > 0)
                        {
                            args.StudentId = ret.First().StuId;
                            //MessageBox.Show(args.StudentId);

                            string q_para = querypara.Replace("$stuid$", args.StudentId).
                                    Replace("#classid#", classes[args.SelectedClassIndex].Key);
                            string q_result = Helper.HttpGet(args.Url, q_para);
                            var scoremodel = Helper.AnalyseHtmlResult(q_result, args.StudentId);
                            if (scoremodel.ClassName.Equals("课程名称"))
                            {
                                scoremodel.ClassName = "未选择该门课程";
                            }
                            //ClassScores.Add(scoremodel);
                            worker.ReportProgress(100, scoremodel);
                        }
                        else
                        {
                            MessageBox.Show("本地数据库中查询不到该学生姓名");
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(args.StudentId) && string.IsNullOrEmpty(args.StudentName))
                {
                    string q_para = querypara.Replace("$stuid$", args.StudentId).
                                   Replace("#classid#", classes[args.SelectedClassIndex].Key);
                    string q_result = Helper.HttpGet(args.Url, q_para);
                    var scoremodel = Helper.AnalyseHtmlResult(q_result, args.StudentId);
                    if (scoremodel.ClassName.Equals("课程名称"))
                    {
                        scoremodel.ClassName = "未选择该门课程";
                    }
                    //ClassScores.Add(scoremodel);
                    worker.ReportProgress(100, scoremodel);
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        void worker_DoWork_单人多科(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(10);
            ScoreModel scoremodel = null;
            WorkerArgs args = e.Argument as WorkerArgs;
            string q_para;
            try
            {
                if (string.IsNullOrEmpty(args.StudentId) && !string.IsNullOrEmpty(args.StudentName))
                {
                    //姓名不为空，直接查询Id
                    using (TableAdapter<Student> adapter = TableAdapter<Student>.Open())
                    {
                        var ret = adapter.Select().Where(Where.Equal("Name", args.StudentName));
                        if (ret != null && ret.Count() > 0)
                        {
                            args.StudentId = ret.First().StuId;
                            //MessageBox.Show(args.StudentId);

                            for (int i = 0; i < classes.Count; i++)
                            {
                                q_para = querypara.Replace("$stuid$", args.StudentId).
                                            Replace("#classid#", classes[i].Key);
                                string q_result = Helper.HttpGet(args.Url, q_para);
                                scoremodel = Helper.AnalyseHtmlResult(q_result, args.StudentId);
                                if (scoremodel.ClassName.Equals("课程名称"))
                                {
                                    scoremodel = null;
                                }
                                //ClassScores.Add(scoremodel);
                                worker.ReportProgress((int)((i + 1) / (float)classes.Count * 100), scoremodel);
                            }
                        }
                        else
                        {
                            MessageBox.Show("本地数据库中查询不到该学生姓名");
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(args.StudentId) && string.IsNullOrEmpty(args.StudentName))
                {
                    for (int i = 0; i < classes.Count; i++)
                    {
                        q_para = querypara.Replace("$stuid$", args.StudentId).
                                    Replace("#classid#", classes[i].Key);
                        string q_result = Helper.HttpGet(args.Url, q_para);
                        scoremodel = Helper.AnalyseHtmlResult(q_result, args.StudentId);
                        if (scoremodel.ClassName.Equals("课程名称"))
                        {
                            scoremodel = null;
                        }
                        //ClassScores.Add(scoremodel);
                        worker.ReportProgress((int)((i + 1) / (float)classes.Count * 100), scoremodel);
                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        void worker_DoWork_多人单科(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(10);
            WorkerArgs args = e.Argument as WorkerArgs;
            try
            {
                Student student = new Student();

                if (args.isStudentNoRangeValid)
                {
                    int count = (int.Parse(args.StudentEndNo) - int.Parse(args.StudentStartNo)) + 1;
                    if (count <= 0)
                    {
                        MessageBox.Show("结束值不能小于起始值");
                        return;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        //输入的是学号范围,先根据学号查询Id
                        using (TableAdapter<Student> adapter = TableAdapter<Student>.Open())
                        {
                            var ret = adapter.Select().Where(Where.Equal("Number", (int.Parse(args.StudentStartNo) + i).ToString("D4")));
                            if (ret != null && ret.Count() > 0)
                            {
                                args.StudentId = ret.First().StuId;

                                string q_para = querypara.Replace("$stuid$", args.StudentId).
                                        Replace("#classid#", classes[args.SelectedClassIndex].Key);
                                string q_result = Helper.HttpGet(args.Url, q_para);
                                var scoremodel = Helper.AnalyseHtmlResult(q_result, args.StudentId);
                                if (scoremodel.ClassName.Equals("课程名称"))
                                {
                                    scoremodel.ClassName = "";
                                }
                                worker.ReportProgress((int)((i + 1) / (float)count * 100), scoremodel);
                            }
                        }
                    }
                }
                else
                {
                    int count = (int.Parse(args.StudentEndId) - int.Parse(args.StudentStartId)) + 1;
                    if (count <= 0)
                    {
                        MessageBox.Show("结束值不能小于起始值");
                        return;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        string q_para = querypara.Replace("$stuid$", (int.Parse(args.StudentStartId) + i).ToString("D4")).
                                        Replace("#classid#", classes[args.SelectedClassIndex].Key);
                        string q_result = Helper.HttpGet(args.Url, q_para);
                        var scoremodel = Helper.AnalyseHtmlResult(q_result, (int.Parse(args.StudentStartId) + i).ToString("D4"));
                        if (scoremodel.ClassName.Equals("课程名称"))
                        {
                            scoremodel.ClassName = "";
                        }
                        #region 保存学生信息
                        /*
                    if (scoremodel != null)
                    { 
                        //保存学生信息到数据库
                        using (TableAdapter<Student> adapter = TableAdapter<Student>.Open())
                        {
                            var ret = adapter.Select().Where(Where.Equal("StuId", scoremodel.Id));
                            if (ret != null && ret.Count() > 0)
                            {
                                //student.Id = ret.First().Id;    
                                //如果已经写入，就不写入
                                continue;
                            }
                            student.Id = 0;
                            student.StuId = scoremodel.Id;
                            student.Name = scoremodel.StudentName;
                            student.Number = scoremodel.StudentNumber;
                            student.Save();//保存到数据库
                        }

                    }
                     * */
                        #endregion
                        //ClassScores.Add(scoremodel);
                        worker.ReportProgress((int)((i + 1) / (float)count * 100), scoremodel);
                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        /// <summary>
        /// 多人加权平均分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_DoWork_加权平均(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(10);
            WorkerArgs args = e.Argument as WorkerArgs;
            try
            {
                Student student = new Student();

                if (args.isStudentNoRangeValid)
                {
                    int count = (int.Parse(args.StudentEndNo) - int.Parse(args.StudentStartNo)) + 1;
                    if (count <= 0)
                    {
                        MessageBox.Show("结束值不能小于起始值");
                        return;
                    }
                   
                    using (TableAdapter<Student> adapter = TableAdapter<Student>.Open())
                    {
                        List<ScoreModel> scorelist;
                        ScoreModel scoremodel = null;
                        //遍历所有学生
                        for (int i = 0; i < count; i++)
                        {
                            //输入的是学号范围,先根据学号查询Id
                            var ret = adapter.Select().Where(Where.Equal("Number", (int.Parse(args.StudentStartNo) + i).ToString("D4")));
                            if (ret != null && ret.Count() > 0)
                            {
                                scorelist = new List<ScoreModel>();
                                ScoreModel model = new ScoreModel();
                                //学生id
                                args.StudentId = ret.First().StuId;
                                //查询各科成绩
                                for (int j = 0; j < classes.Count; j++)
                                {
                                    string q_result = Helper.HttpGet(args.Url,
                                        querypara.Replace("$stuid$", args.StudentId).
                                        Replace("#classid#", classes[j].Key));
                                    scoremodel = Helper.AnalyseHtmlResult(q_result, args.StudentId);
                                    model.StudentNumber = scoremodel.StudentNumber;
                                    model.StudentName = scoremodel.StudentName;
                                    model.Id = scoremodel.Id;
                                    if (scoremodel.ClassName.Equals("课程名称")) scoremodel = null;
                                    if (scoremodel!=null) scorelist.Add(scoremodel);
                                }

                                //计算加权平均分
                                double avaScore;
                                var list = scorelist.Where(t => (!string.IsNullOrEmpty(t.Score) && !string.IsNullOrEmpty(t.CreditGet) && isValidNumber(t.CreditGet)));//学分不为空的
                                double totalCredit = list.Select(t => double.Parse(t.CreditGet)).Sum();
                                if (totalCredit != 0)
                                {
                                    avaScore = (list.Select(t => double.Parse(t.Score == "" ? "0.0" : t.Score) * double.Parse(t.CreditGet)).Sum() / totalCredit);

                                    model.Credit = totalCredit.ToString();
                                    model.Teacher = "";
                                    model.Score = avaScore.ToString("F2");
                                    model.CreditGet = totalCredit.ToString();
                                    
                                    worker.ReportProgress((int)((i + 1) / (float)count * 100), model);
                                }
                            }
                        }
                    }
                }
                else
                {
                    int count = (int.Parse(args.StudentEndId) - int.Parse(args.StudentStartId)) + 1;
                    if (count <= 0)
                    {
                        MessageBox.Show("结束值不能小于起始值");
                        return;
                    }

                    List<ScoreModel> scorelist;
                    ScoreModel scoremodel = null;
                    for (int i = 0; i < count; i++)
                    {
                        scorelist = new List<ScoreModel>();
                        ScoreModel model = new ScoreModel();
                        //查询各科成绩
                        for (int j = 0; j < classes.Count; j++)
                        {
                            string q_result = Helper.HttpGet(args.Url,
                                querypara.Replace("$stuid$", (int.Parse(args.StudentStartId) + i).ToString("D4")).
                                        Replace("#classid#", classes[j].Key));
                            scoremodel = Helper.AnalyseHtmlResult(q_result, (int.Parse(args.StudentStartId) + i).ToString("D4"));
                            model.StudentNumber = scoremodel.StudentNumber;
                            model.StudentName = scoremodel.StudentName;
                            model.Id = scoremodel.Id;
                            if (scoremodel.ClassName.Equals("课程名称")) scoremodel = null;
                            if (scoremodel != null) scorelist.Add(scoremodel);
                        }

                        //计算加权平均分
                        double avaScore;
                        var list = scorelist.Where(t => (!string.IsNullOrEmpty(t.Score) && !string.IsNullOrEmpty(t.CreditGet) && isValidNumber(t.CreditGet)));//学分不为空的
                        double totalCredit = list.Select(t => double.Parse(t.CreditGet)).Sum();
                        if (totalCredit != 0)
                        {
                            avaScore = (list.Select(t => double.Parse(t.Score == "" ? "0.0" : t.Score) * double.Parse(t.CreditGet)).Sum() / totalCredit);

                            model.ClassNumber = "";
                            model.Term = "";
                            model.ClassName = "";
                            model.Credit = totalCredit.ToString();
                            model.Teacher = "";
                            model.Score = avaScore.ToString("F2");
                            model.CreditGet = totalCredit.ToString();

                            worker.ReportProgress((int)((i + 1) / (float)count * 100), model);
                        }

                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        #endregion

        #region 变更通知
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private void AddClassMap_Click(object sender, RoutedEventArgs e)
        {
            AddClassIdWindow aciWindow = new AddClassIdWindow(classes);
            //aciWindow.Owner = this;
            aciWindow.ShowDialog();
        }

        /// <summary>
        /// 清空列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearListBtn_Click(object sender, RoutedEventArgs e)
        {
            ClassScores.Clear();
        }

        /// <summary>
        /// 保存txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveTxtBtn_Click(object sender, RoutedEventArgs e)
        {
            XlsExporter gridOrderList = new XlsExporter(scorelist);
            string data = gridOrderList.ExportDataGrid(true);
            byte[] tmp;
            tmp = Encoding.Unicode.GetBytes(data);
            SaveFileDialog sfd = new SaveFileDialog()
            {
                DefaultExt = "csv",
                Filter = "表格文件(*.csv)|*.csv|文本文件(*.txt)|*.txt|所有文件 (*.*)|*.*",
                FilterIndex = 1
            };
            if (sfd.ShowDialog() == true)
            {
                using (System.IO.Stream stream = sfd.OpenFile())
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream, Encoding.Unicode, 30))
                    {
                        writer.Write(data);
                        writer.Close();
                    }
                    stream.Close();
                }
                MessageBox.Show("保存成功！");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mode.SelectedIndex = 0;
        }

        private void decNoBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((int.Parse(stuidTextBox.Text)) > 0)
                {
                    stuidTextBox.Text = ((int.Parse(stuidTextBox.Text)) - 1).ToString("D4");
                }
            }
            catch
            {
                
            }
        }

        private void incNoBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((int.Parse(stuidTextBox.Text))<9999)
                {
                    stuidTextBox.Text = ((int.Parse(stuidTextBox.Text)) + 1).ToString("D4");
                }
            }
            catch
            {

            }
        }
        
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            stuidTextBox.Text = (sender as ListView).SelectedItem.ToString().Substring(1,4);            
        }

        /// <summary>
        /// 显示柱状图
        /// </summary>
        private void ShowHistChart()
        {
            DataSeries dataSeries = new DataSeries();
            DataPoint dataPoint;
            foreach(var v in ClassScores)
            {
                if (!string.IsNullOrEmpty(v.Score))
                {
                    dataPoint = new DataPoint() { XValue = v.StudentNumber, YValue = double.Parse(v.Score) };
                    dataSeries.DataPoints.Add(dataPoint);                    
                }
            }
            dataSeries.DataPoints.OrderBy(a => a.XValue);

            // Create a new instance of Chart
            Chart chart = new Chart();
            chart.View3D = false;
            //chart.Width = 600;
            //chart.Height = 500;
            chart.AnimationEnabled = false;
            chart.ZoomingEnabled = true;
            chart.IndicatorEnabled = true;
            chart.ToolBarEnabled = true;
            
            Title title = new Title();            
            title.Text = classes[selectedClass.SelectedIndex].Value + "成绩分布";
            chart.Titles.Add(title);
            //显示标注
            dataSeries.LabelStyle = LabelStyles.OutSide;
            dataSeries.LabelEnabled = true;
            // Set DataSeries property
            dataSeries.RenderAs = RenderAs.Line;
            dataSeries.AxisXType = AxisTypes.Primary;
            dataSeries.MarkerEnabled = false;
            // Add dataSeries to Series collection.
            chart.Series.Add(dataSeries);

            ChartWindow cwindow = new ChartWindow(chart);
            cwindow.ShowDialog();
        }

        private void showChartBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowHistChart();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }        
       
    }
}
