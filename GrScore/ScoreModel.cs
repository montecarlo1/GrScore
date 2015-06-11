using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrScore
{
    public class ScoreModel
    {
        public string Id { set; get; }
        public string StudentNumber { set; get; }
        public string StudentName { set; get; }
        public string ClassNumber { set; get; }
        public string Term { set; get; }
        public string ClassName { set; get; }
        /// <summary>
        /// 学分
        /// </summary>
        public string Credit { set; get; }
        /// <summary>
        /// 教师
        /// </summary>
        public string Teacher { set; get; }
        /// <summary>
        /// 成绩
        /// </summary>
        public string Score { set; get; }
        /// <summary>
        /// 获得的学分
        /// </summary>
        public string CreditGet { set; get; }
    }
}
