using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using JM.CARRIER.Tools;
using System.Data.SQLite;

namespace JM.CARRIER
{
    public class WeightInfoTable
    {
        #region SQL const
        internal const string TABLE_COLUMNS = "Number,BodyWeight,Time , Tab";
        internal const string SQL_SELECT = "select " + TABLE_COLUMNS + " from WeightInfoTable";
        internal const string SQL_SELECT_ONE = "select " + TABLE_COLUMNS + " from WeightInfoTable where Tab = @Tab ORDER BY Number ASC ";
        internal const string SQL_INSERT = "insert into WeightInfoTable (" + TABLE_COLUMNS + ") values (@Number, @BodyWeight, @Time, @Tab)";
        internal const string SQL_COUNT = "select count(*) from WeightInfoTable";
        internal const string SQL_DELETE = "delete from WeightInfoTable";
        internal const string SQL_DELETE_ONE = "delete from WeightInfoTable where Number=@Number and Time =@Time";
        internal const string SQL_SELECT_ALL = "select * from WeightInfoTable where 1=1 ORDER BY Number DESC";
        internal const string SQL_UPDATA_ONE = "update WeightInfoTable set Tab = @Tab where Number=@Number";
        #endregion

        #region Query模块
        public int QueryData()
        {
            int cmdresult;
            object obj = SQLiteHelper.GetSingle(SQL_COUNT);
            cmdresult = int.Parse(obj.ToString());
            return cmdresult;
        }

        public bool QueryData(ref List<ModalWeightInfoTable> MoList)
        {
            bool bRet = true;
            try
            {
                DataSet dt = SQLiteHelper.Query(SQL_SELECT_ALL);
                if (dt != null)
                {
                    if (MoList == null)
                        MoList = new List<ModalWeightInfoTable>();
                    foreach (DataRow dr in dt.Tables[0].Rows)
                    {
                        ModalWeightInfoTable tmp = new ModalWeightInfoTable();
                        tmp.Row2Model(dr);
                        MoList.Add(tmp);
                    }
                }
            }
            catch(Exception ex)
            {
                Util.LogManager.Logger.Error("查询记录是出现错误，错误为{0}", ex.ToString());
                bRet = false;
            }
            return bRet;
        }

        /// <summary>
        /// 按条码查询工件信息
        /// </summary>
        /// <param name="workId"></param>
        /// <param name="MoList"></param>
        /// <returns></returns>
        public bool QueryData(Int32 tab, ref List<ModalWeightInfoTable> MoList)
        {
            bool bRet = true;
            try
            {
                DataSet dt;
                SQLiteParameter[] parameters = {
                                            new SQLiteParameter("@Tab",  DbType.Int32)
                                           };
                parameters[0].Value = tab;
                dt = SQLiteHelper.Query(SQL_SELECT_ONE, parameters);
                if (dt != null)
                {
                    if (MoList == null)
                        MoList = new List<ModalWeightInfoTable>();
                    foreach (DataRow dr in dt.Tables[0].Rows)
                    {
                        ModalWeightInfoTable tmp = new ModalWeightInfoTable();
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

        /// <summary>
        /// 按时间查询工件信息
        /// </summary>
        /// <param name="workId"></param>
        /// <param name="MoList"></param>
        /// <returns></returns> 
        public bool QueryData(string time1, string time2, ref List<ModalWeightInfoTable> MoList)
        {
            bool bRet = true;
            try
            {
                DataSet dt;
                string sql = string.Format("{0}  where Time BETWEEN  '{1}'  AND '{2}'", SQL_SELECT, time1, time2);
                dt = SQLiteHelper.Query(sql);
                if (dt != null)
                {
                    if (MoList == null)
                        MoList = new List<ModalWeightInfoTable>();

                    foreach (DataRow dr in dt.Tables[0].Rows)
                    {
                        ModalWeightInfoTable tmp = new ModalWeightInfoTable();
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
        #endregion

        public int UpData(Int32 number, Int32 tab)
        {
            int iRet = 0;
            try
            {
                SQLiteParameter[] parameters = {
                                            new SQLiteParameter("@Number",  DbType.Int32),
                                            new SQLiteParameter("@Tab",  DbType.Int32)
                                           };
                parameters[0].Value = number;
                parameters[1].Value = tab;

                iRet = SQLiteHelper.ExecuteSql(SQL_UPDATA_ONE, parameters);
            }
            catch (Exception ex)
            {
                Util.LogManager.Logger.Error("修改数据库出现错误，错误为{0}", ex.ToString());
                iRet = 0;
            }
            return iRet;
        }

        #region Add模块
        /// <summary>
        /// 按条件查询条码信息
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="MoList"></param>
        /// <returns></returns>
        public int AddData(ModalWeightInfoTable Mo)
        {
            int iRet = 0;
            try
            {
                SQLiteParameter[] parameters = {    new SQLiteParameter("@Number",  DbType.Int32),
                                            new SQLiteParameter("@BodyWeight",  DbType.String),
                                            new SQLiteParameter("@Time",  DbType.String),
                                            new SQLiteParameter("@Tab",  DbType.Int32)
                                           };
                parameters[0].Value = Mo.Number;
                parameters[1].Value = Mo.BodyWeight;
                parameters[2].Value = Mo.Time;
                parameters[3].Value = Mo.Tab;

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

        #region Delete模块
        /// <summary>
        /// 删除一个
        /// </summary>
        /// <returns></returns>
        public int DeleteData(ModalWeightInfoTable Mo)
        {
            int iRet = 0;
            try
            {
                SQLiteParameter[] parameters = {    new SQLiteParameter("@Number",  DbType.Int32),
                                            new SQLiteParameter("@Time",  DbType.String)
                                           };
                parameters[0].Value = Mo.Number;
                parameters[1].Value = Mo.Time;

                iRet = SQLiteHelper.ExecuteSql(SQL_DELETE_ONE, parameters);
            }
            catch (Exception ex)
            {
                Util.LogManager.Logger.Error("删除数据出现错误，错误为{0}", ex.ToString());
                iRet = 0;
            }
            return iRet;
        }

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
        #endregion

    }

    /// <summary>
    /// 表StationDistanceTable结构体
    /// </summary>
    public class ModalWeightInfoTable : ICloneable, INotifyPropertyChanged
    {
        #region 构造函数
        public ModalWeightInfoTable()
        {

        }

        public ModalWeightInfoTable(int number, string bodyweight, string time , int tab)
        {
            this.Number = number;
            this.BodyWeight = bodyweight;
            this.Time = time;
            this.Tab = tab;
        }
        #endregion

        #region 字段属性

        private int _number;

        public int Number
        {
            get
            {
                return _number;
            }
            set
            {
                _number = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Number"));
            }
        }

        private string _bodyweight;

        public string BodyWeight
        {
            get
            {
                return _bodyweight;
            }
            set
            {
                _bodyweight = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BodyWeight"));
            }
        }

        private string _time;

        public string Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Time"));
            }
        }

        private int _tab;
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
            ModalWeightInfoTable ds = new ModalWeightInfoTable();
            ds.Number = Number;
            ds.BodyWeight = BodyWeight;
            ds.Time = Time;
            ds.Tab = Tab;
            return ds;
        }
        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2} \r\n", Number, BodyWeight, Time);
        }
        #endregion

        #region DataTable Help Function
        ///<summary>
        ///DataRow转换成Model
        ///</summary>
        public ModalWeightInfoTable Row2Model(DataRow row)
        {
            this.Number = int.Parse(row.Field<object>("Number").ToString());
            this.BodyWeight = row.Field<string>("BodyWeight").ToString();
            this.Time = row.Field<string>("Time").ToString();
           this.Tab = int.Parse(row.Field<object>("Tab").ToString());
            return this;
        }
        #endregion

    }

}
