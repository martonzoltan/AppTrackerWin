using AppTrackerWin.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTrackerWin.Helper
{
    public class ExcelHelper
    {
        public ErrorHandling Save(List<TrackedWindowStorage> allStoredData)
        {
            //Create a new ExcelPackage
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                //Set some properties of the Excel document
                excelPackage.Workbook.Properties.Author = "MzA";
                excelPackage.Workbook.Properties.Title = "AppTrackerExport";
                excelPackage.Workbook.Properties.Created = DateTime.Now;

                //Create the WorkSheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet 1");

                //Headers
                worksheet.Cells["A1"].Value = "Date";
                worksheet.Cells["B1"].Value = "File";
                worksheet.Cells["C1"].Value = "Time Open";

                worksheet.Column(1).Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Column(1).Width = 15;
                worksheet.Column(2).Width = 35;
                worksheet.Column(3).Width = 15;


                //Fill in with date
                int i = 2;
                foreach(var entry in allStoredData)
                {
                    worksheet.Cells[i, 1].Value = entry.Date;
                    worksheet.Cells[i, 2].Value = entry.Name;
                    worksheet.Cells[i, 3].Value = Math.Round(Convert.ToDouble(entry.TimeSpent)/60,2);
                    i++;
                }

                try
                {
                    //Save your file
                    FileInfo fi = new FileInfo(@"apps_usage.xlsx");
                    excelPackage.SaveAs(fi);
                    return new ErrorHandling { isError = false };
                }
                catch (Exception e)
                {
                    return new ErrorHandling
                    {
                        Message = e.Message,
                        isError = true
                    };
                }
            }
        }
    }
}
