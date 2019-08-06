using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JM.CARRIER.Tools
{
    public class BllExcel
    {
        #region 
        // 报表导出的标题
        private static string[] columnDetectName = { "序号", "工件条码", "批次码", "货架码", "时间"};

        private Dictionary<int, List<object>> dictData = new Dictionary<int, List<object>>();
        #endregion 
        BllExcel()
        {
            dictData.Clear();
        }


        /// <summary>
        /// 检测数据的导出函数
        /// </summary>
        public bool DetectExport(List<ModalWeightInfoTable> MoList)
        {
            bool bRet = true;
            dictData.Clear();
            string dateTime = "";
            if (MoList.Count == 0)
            {
                bRet = false;
                return bRet;
            }
            for (int i = 0; i < MoList.Count; i++)
            {
                List<object> columnValue = new List<object>();
                columnValue.Add(MoList[i].Number);
                columnValue.Add(MoList[i].BodyWeight);
                columnValue.Add(MoList[i].Time);                
                dictData.Add(i, columnValue);
            }
            if (bRet)
            {
                dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string filename = DateTime.Now.ToString("yyyyMMddHHmmss");
               
                bRet = ExportToExcel(filename, columnDetectName, dateTime);
                
            }
            return bRet;
        }




        /// <summary>
        /// 导出到Excel函数
        /// </summary>
        private bool ExportToExcel(string fileName, string[] columnName, string date)
        {
            bool bRet = true;
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "Excel 文件(*.xlsx;*.xls)|*.xlsx;*.xls";
            sfd.FileName = fileName;
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                bRet = false;
                return bRet;
            }

            XSSFWorkbook workbook = new XSSFWorkbook();
            XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet(fileName);
            sheet.PrintSetup.PaperSize = 9; // A4打印形式
            sheet.PrintSetup.Landscape = false; // 纵向打印 

            var p = fileName.Split('_');

            #region 创建标题名称
            XSSFRow title = (XSSFRow)sheet.CreateRow(0);

            for (int j = 0; j < columnName.Count(); j++)
            {
                XSSFCell headCell = (XSSFCell)title.CreateCell(j, CellType.String);
            }
            // 合并单元格
            CellRangeAddress region1 = new CellRangeAddress(0, 0, 0, (columnName.Count() - 1));
            sheet.AddMergedRegion(region1);

            title.CreateCell(0).SetCellValue("庄信万丰（上海）化工有限公司装载机数据表");

            ICellStyle titleStyle = workbook.CreateCellStyle();// 样式  
            // 设置单元格的样式：水平对齐居中
            titleStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            // 将新的样式赋给单元格
            title.GetCell(0).CellStyle = titleStyle;
            #endregion


            #region 创建日期
            XSSFRow dateRow = (XSSFRow)sheet.CreateRow(1);

            for (int j = 0; j < columnName.Count(); j++)
            {
                XSSFCell headCell = (XSSFCell)dateRow.CreateCell(j, CellType.String);
            }
            // 合并单元格
            CellRangeAddress region3 = new CellRangeAddress(1, 1, 0, (columnName.Count() - 1));
            sheet.AddMergedRegion(region3);
            string tempDate = "导出时间:" + date;
            dateRow.CreateCell(0).SetCellValue(tempDate);
            #endregion

            // 加载数据
            XSSFRow headRow = (XSSFRow)sheet.CreateRow(2);
            for (int i = 0; i < columnName.Count(); i++)
            {
                XSSFCell headCell = (XSSFCell)headRow.CreateCell(i, CellType.String);
                headCell.SetCellValue(columnName[i]);
                headCell.CellStyle = AddStytle(workbook);
            }
            for (int i = 0; i < dictData.Count; i++)
            {
                XSSFRow row = (XSSFRow)sheet.CreateRow(i + 3);
                for (int j = 0; j < dictData[i].Count; j++)
                {
                    XSSFCell cell = (XSSFCell)row.CreateCell(j);
                    cell.CellStyle = AddStytle(workbook);
                    if (dictData[i][j] == null)
                    {
                        cell.SetCellType(CellType.Blank);
                    }
                    else
                    {
                        if (dictData[i][j] is string)
                        {
                            cell.SetCellValue(dictData[i][j].ToString());
                        }
                        else if (dictData[i][j] is decimal)
                        {
                            cell.SetCellValue(Convert.ToSingle(dictData[i][j]));
                        }
                        else if (dictData[i][j] is int)
                        {
                            cell.SetCellValue(Convert.ToInt32(dictData[i][j]));
                        }
                        else if (dictData[i][j] is double)
                        {
                            cell.SetCellValue(Convert.ToDouble(dictData[i][j]));
                        }
                        else if (dictData[i][j] is Decimal)
                        {
                            cell.SetCellValue(Convert.ToDouble(dictData[i][j]));
                        }
                        else if (dictData[i][j] is DateTime)
                        {
                            cell.SetCellValue(Convert.ToDateTime(dictData[i][j]).ToString("yyyy-MM-dd hh:ss:mm"));
                        }
                    }
                }
            }
            for (int i = 0; i < columnName.Count(); i++)
            {
                sheet.AutoSizeColumn(i);
            }
            #region 保存到Excel
            using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
            {
                workbook.Write(fs);
            }
            #endregion
            AutoDeleteMessageBox Auto = new AutoDeleteMessageBox(); // 自动关闭窗口
            MessageBox.Show("恭喜，导出成功!", "MessageBox");
            return bRet;
        }


        /// <summary>
        /// 给单元格加边框，居中
        /// </summary>
        private ICellStyle AddStytle(XSSFWorkbook workbook)
        {
            ICellStyle style = workbook.CreateCellStyle();// 样式
            // 设置单元格的样式：水平对齐居中
            style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;// 下边框为细线边框
            style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;// 左边框
            style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;// 上边框
            style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;// 右边框
            return style;
        }

        #region 单类模式
        public static BllExcel Instance { get; private set; }

        static BllExcel()
        {
            Instance = new BllExcel();
        }
        #endregion
    }
}
