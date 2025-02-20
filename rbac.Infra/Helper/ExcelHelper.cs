using System;
using OfficeOpenXml;

namespace rbac.Infra.Helper;

public static class ExcelHelper
{
    /// <summary>
    /// 简单导出excel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataList"></param>
    /// <returns></returns>
    public static byte[]  SetSimpleExcel<T>(List<T> dataList) where T : class
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");

            // 通过反射获取属性名称作为表头
            var properties = typeof(T).GetProperties();
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            int row = 2;
            foreach (var data in dataList)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row, col + 1].Value = properties[col].GetValue(data);
                }
                row++;
            }
            // 保存 Excel 文件
            return package.GetAsByteArray();
        }
    }
}
