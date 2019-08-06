using JM.CARRIER.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.CARRIER
{
    class SpurtCodeInfoTable
    {
        #region SQL const
        internal const string TABLE_COLUMNS = "Counter,SpurtCode,Tab";
        internal const string SQL_SELECT = "select " + TABLE_COLUMNS + " from WeightInfoTable";
        internal const string SQL_SELECT_ONE = "select " + TABLE_COLUMNS + " from WeightInfoTable where Counter = @Counter";
        internal const string SQL_INSERT = "insert into WeightInfoTable (" + TABLE_COLUMNS + ") values (@Counter, @SpurtCode, @Tab)";
        internal const string SQL_COUNT = "select count(*) from WeightInfoTable";
        internal const string SQL_DELETE = "delete from WeightInfoTable";
        #endregion

        #region Query模块
        public int QueryData()
        {
            int cmdresult;
            object obj = SQLiteHelper.GetSingle(SQL_COUNT);
            cmdresult = int.Parse(obj.ToString());
            return cmdresult;
        }

        /// <summary>
        /// 按条码查询工件信息
        /// </summary>
        /// <param name="workId"></param>
        /// <param name="MoList"></param>
        /// <returns></returns>
        public bool QueryData(int counter, ref List<ModalSpurtCodeInfo> MoList)
        {
            bool bRet = true;
            try
            {
                DataSet dt;
                SQLiteParameter[] parameters = {
                                            new SQLiteParameter("@Counter",  DbType.Int32)
                                           };
                parameters[0].Value = counter;
                dt = SQLiteHelper.Query(SQL_SELECT_ONE, parameters);
                if (dt != null)
                {
                    if (MoList == null)
                        MoList = new List<ModalSpurtCodeInfo>();
                    foreach (DataRow dr in dt.Tables[0].Rows)
                    {
                        ModalSpurtCodeInfo tmp = new ModalSpurtCodeInfo();
                        tmp.Row2Model(dr);
                        MoList.Add(tmp);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogManager.Logger.Error("查询记录是出现错误，错误为{0}", ex.ToString());
                bRet = false;
            }
            return bRet;
        }


        #region Add模块
        /// <summary>
        /// 按条件查询条码信息
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="MoList"></param>
        /// <returns></returns>
        public int AddData(ModalSpurtCodeInfo Mo)
        {
            int iRet = 0;
            try
            {
                SQLiteParameter[] parameters = {    new SQLiteParameter("@Counter",  DbType.Int32),
                                            new SQLiteParameter("@SpurtCode",  DbType.String),
                                            new SQLiteParameter("@Tab",  DbType.Int32),
                                           };
                parameters[0].Value = Mo.Counter;
                parameters[1].Value = Mo.SpurtCode;
                parameters[2].Value = Mo.Tab;

                iRet = SQLiteHelper.ExecuteSql(SQL_INSERT, parameters);
            }
            catch (Exception ex)
            {
                Util.LogManager.Logger.Error("插入数据库出现错误，错误为{0}", ex.ToString());
                iRet = 0;
            }
            return iRet;
        }
        #endregion

        #endregion
        /// <summary>
        /// 清除
        /// </summary>
        public int DeleteData()
        {
            int iRet = 0;
            try
            {
                iRet = SQLiteHelper.ExecuteSql(SQL_DELETE);
            }
            catch (Exception ex)
            {
                Util.LogManager.Logger.Error("清空数据库出现错误，错误为{0}", ex.ToString());
                iRet = 0;
            }
            return iRet;
        }
       

    }

    /// <summary>
    /// 表StationDistanceTable结构体
    /// </summary>
    public class ModalSpurtCodeInfo : ICloneable, INotifyPropertyChanged
    {
        #region 构造函数
        public ModalSpurtCodeInfo()
        {

        }

        public ModalSpurtCodeInfo(int Counter, string SpurtCode , int Tab)
        {
            this.Counter = Counter;
            this.SpurtCode = SpurtCode;
            this.Tab = Tab;
        }
        #endregion

        #region 字段属性

        private int _counter;

        private string _spurtcode;

        private int _tab;

        public int Counter
        {
            get
            {
                return _counter;
            }

            set
            {
                _counter = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Counter"));
            }
        }
        public string SpurtCode
        {
            get
            {
                return _spurtcode;
            }

            set
            {
                _spurtcode = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SpurtCode"));
            }
        }

        public int Tab
        {
            get
            {
                return _tab;
            }

            set
            {
                _tab = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Tab"));
            }
        }


        #endregion

        #region 辅助函数        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// 克隆函数
        /// </summary>		
        public object Clone()
        {
            ModalSpurtCodeInfo ds = new ModalSpurtCodeInfo();
            ds.Counter = Counter;
            ds.SpurtCode = SpurtCode;
            ds.Tab = Tab;
            return ds;
        }
        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\r\n", Counter, SpurtCode,Tab);
        }
        #endregion

        #region DataTable Help Function
        ///<summary>
        ///DataRow转换成Model
        ///</summary>
        public ModalSpurtCodeInfo Row2Model(DataRow row)
        {
            this.Counter = int.Parse(row.Field<object>("Counter").ToString());
            this.SpurtCode = row.Field<string>("SpurtCode").ToString();
            this.Tab = int.Parse(row.Field<object>("Tab").ToString());
            return this;
        }
        #endregion

    }

}
