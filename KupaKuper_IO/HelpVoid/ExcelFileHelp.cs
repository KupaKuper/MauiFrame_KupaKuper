using OfficeOpenXml;

namespace KupaKuper_IO.HelpVoid
{
    public static class ExcelFileHelp
    {
        /// <summary>
        /// 将指定Csv文件复制到新的Excel文件中
        /// </summary>
        /// <param name="CsvAddress"></param>
        /// <param name="ExcelAddress"></param>
        public static void CsvToExcel(string CsvAddress,string ExcelAddress)
        {
            File.Copy(CsvAddress, ExcelAddress, true);
        }
        /// <summary>
        /// 在表格指定位置填写数据
        /// </summary>
        /// <param name="ExcelAddress"></param>
        /// <param name="Row"></param>
        /// <param name="Column"></param>
        /// <param name="Data"></param>
        public static void AddDataToExcel(string ExcelAddress,int Row ,int Column,object Data)
        {
            //设置Excel
            ExcelPackage package = new(ExcelAddress);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets["Sheet1"];
            sheet1.Cells[Row,Column].Value = Data;
        }
    }
}
