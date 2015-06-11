using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;

namespace GrScore
{
    class Helper
    {
        public static string HttpGet(string Url, string postDataStr)
        {            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("GB2312"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }


        public static ScoreModel AnalyseHtmlResult(string result,string stuid)
        {
            ScoreModel model = new ScoreModel();

            string pattern = @"<td.*>([^<>]+)</td>";
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
            /*
            foreach (Match m in reg.Matches(result))
            {
                Console.WriteLine(m.Groups[1].Value.Trim());
            }
            */
            var matches = reg.Matches(result);
            model.StudentNumber = matches[0].Groups[1].Value.Replace("学号：",string.Empty).Trim();
            model.StudentName = matches[1].Groups[1].Value.Replace("姓名：", string.Empty).Trim();
            model.ClassNumber = matches[2].Groups[1].Value.Replace("课程编号：", string.Empty).Trim();
            model.Term = matches[3].Groups[1].Value.Replace("上课学期", string.Empty).Trim();
            model.ClassName = matches[4].Groups[1].Value.Replace("课程名称：", string.Empty).Trim();
            model.Credit = matches[5].Groups[1].Value.Replace("学分", string.Empty).Trim();
            model.Teacher = matches[6].Groups[1].Value.Replace("任课教师", string.Empty).Trim();
            model.Score = matches[7].Groups[1].Value.Replace("总评成绩", string.Empty).Trim();
            model.CreditGet = matches[8].Groups[1].Value.Replace("获得学分", string.Empty).Trim();
            if (matches.Count >= 15)
            {
                //英语基础（有两门成绩）
                model.Credit = "3.0";
                model.CreditGet = "3.0";
                if (model.Score.Equals(""))
                {
                    model.Score = double.Parse(matches[14].Groups[1].Value.Trim() == "" ? "0.0" : matches[14].Groups[1].Value.Trim()) > 0.0 ?
                   matches[14].Groups[1].Value.Trim() : model.Score;
                }
                else
                {
                    model.Score = double.Parse(matches[14].Groups[1].Value.Trim() == "" ? "0.0" : matches[14].Groups[1].Value.Trim()) > double.Parse(model.Score) ?
                   matches[14].Groups[1].Value.Trim() : model.Score;
                }
            }
            model.Id = stuid;

            return model;
        }
    }

  
}