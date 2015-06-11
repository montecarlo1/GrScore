using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using SqliteORM;
using GrScore.DataAccess.DataBaseAccess;

namespace GrScore
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DbConnection.Initialise(SQLiteConnectionString.GetConnectionString());

            MainWindow view = new MainWindow();
            view.Show();
        }
    }
}
