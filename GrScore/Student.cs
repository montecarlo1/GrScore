using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqliteORM;
using GrScore.DataAccess.DataBaseAccess;

namespace GrScore
{
    [Table]
    public class Student :TableBase<Student>
    {
        [PrimaryKey(true)]
        public long Id
        {
            set;
            get;
        }

        /// <summary>
        /// 学号Id
        /// </summary>
        public string _StuId;
        [Field]
        public string StuId
        {
            set
            {
                _StuId = value;
                OnPropertyChanged("StuId");
            }
            get
            {
                return _StuId;
            }
        }

        /// <summary>
        /// 学号
        /// </summary>
        public string _Number;
        [Field]
        public string Number
        {
            set
            {
                _Number = value;
                OnPropertyChanged("Number");
            }
            get
            {
                return _Number;
            }
        }

        /// <summary>
        /// 学生姓名
        /// </summary>
        public string _Name;
        [Field]
        public string Name
        {
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
            get
            {
                return _Name;
            }
        }
    }
}
