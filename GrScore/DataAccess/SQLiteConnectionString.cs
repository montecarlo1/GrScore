using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrScore.DataAccess.DataBaseAccess
{
    class SQLiteConnectionString
    {
        private static string data_path = AppDomain.CurrentDomain.BaseDirectory + "stuinfo.db";

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="password">默认访问密码</param>
        /// <returns></returns>
        public static string GetConnectionString(string password = null)
        {
            string str = null;
            str = GetConnectionString(data_path, password);
            return str;
        }

        public static string GetConnectionString(string path, string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Data Source=" + path;
            return "Data Source=" + path + ";Password=" + password;
        }
    }
}
