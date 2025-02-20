using System;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using rbac.Infra.Exceptions;
using Serilog;

namespace rbac.Infra.Tools;

public static class ExportTools
{
    #region Read Excel
    /// <summary>
    /// 按工作表名称读取Excel
    /// </summary>
    /// <param name="package"></param>
    /// <param name="name"></param>
    /// <param name="startRow"></param>
    /// <param name="startCol"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static DataTable Read(this ExcelPackage package, string name, int startRow = 1, int startCol = 1)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        if (package is null)
            throw new ArgumentNullException(nameof(package));

        if (!package.Workbook.Worksheets.Any(w => w.Name == name))
            return package.Read(0);

        var worksheet = package.Workbook.Worksheets[name];
        return Read(worksheet, startRow, startCol);
    }
    /// <summary>
    /// 按工作表索引读取Excel
    /// </summary>
    /// <param name="package"></param>
    /// <param name="index"></param>
    /// <param name="startRow"></param>
    /// <param name="startCol"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static DataTable Read(this ExcelPackage package, int index, int startRow = 1, int startCol = 1)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        if (package is null)
            throw new ArgumentNullException(nameof(package));

        var worksheet = package.Workbook.Worksheets[index];
        return Read(worksheet, startRow, startCol);
    }
    /// <summary>
    /// 读取工作表
    /// </summary>
    /// <param name="ws"></param>
    /// <param name="startRow"></param>
    /// <param name="startCol"></param>
    /// <returns></returns>
    /// <exception cref="DuplicateDomainException"></exception>
    public static DataTable Read(this ExcelWorksheet ws, int startRow = 1, int startCol = 1)
    {
        try
        {
            ws.View.UnFreezePanes();
            if (ws.Cells == null || ws.Dimension == null)
            {
                // 如果 ExcelWorksheet 为空，直接返回一个空的 DataTable
                return new DataTable();
            }
            var table = new DataTable();
            foreach (var firstRowCell in ws.Cells[startRow, startCol, 1, ws.Dimension.End.Column])
            {
                var name = !string.IsNullOrEmpty(firstRowCell.Text) ? firstRowCell.Text : $"Column {firstRowCell.Start.Column}";

                if (!string.IsNullOrEmpty(name) && table.Columns.Contains(name))
                {
                    throw new DomainException($"重复列名{name}");
                }

                table.Columns.Add(name);
            }


            for (var rowNum = startRow + 1; rowNum <= ws.Dimension.End.Row; rowNum++)
            {
                var row = table.NewRow();
                var values = new string[table.Columns.Count];
                for (var colNum = startCol; colNum <= table.Columns.Count; colNum++)
                {
                    if (colNum <= table.Columns.Count + 1)
                        values[colNum - 1] = ws.Cells[rowNum, colNum]?.Value?.ToString()?.Trim() ?? "";
                }
                row.ItemArray = values;
                table.Rows.Add(row);
            }
            return table;
        }
        catch (Exception ex)
        {
            Log.Error($"读取Excel出现错误: {ex.Message}", ex);
            throw;
        }
    }

    #endregion

    #region Export Excel
    /// <summary>
    /// 导出Excel
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="contents"></param>
    /// <returns></returns>
    public static bool Export(string filepath, params ExcelExportOption[] contents)
    {
        return Export(contents.ToList(), filepath);
    }

    /// <summary>
    /// 导出Excel
    /// </summary>
    /// <param name="data"></param>
    /// <param name="filepath">路径</param>
    /// <returns></returns>
    public static bool Export(List<ExcelExportOption> data, string filepath)
    {
        try
        {
            if (File.Exists(filepath))
                File.Delete(filepath);

            using (Stream newStream = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite)) // 注意 FileAccess 参数
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage package = new ExcelPackage(newStream))
                {
                    foreach (var item in data)
                    {
                        if (package.Workbook.Worksheets.Any(t => t.Name == item.Name))
                            package.Workbook.Worksheets.Delete(item.Name);

                        var worksheet = package.Workbook.Worksheets.Add(item.Name);
                        CreateWorksheet(worksheet, item);
                    }
                    package.Save();
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "导出Excel异常");
        }
        return false;
    }


    /// <summary>
    /// 导出Excel
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="exportOptions"></param>
    public static void CreateWorksheet(ExcelWorksheet worksheet, ExcelExportOption exportOptions)
    {
        exportOptions.Rows = exportOptions.Rows ?? new List<Dictionary<string, object>> { };
        //获取列头最大行
        List<int> all = new List<int>();
        GetTreeCount(exportOptions.Columns, all);
        int headRow = all.Max();
        //获取列头
        var headList = GetHeads(exportOptions.Columns.OrderBy(t => t.Sort).ToList(), exportOptions.IgnoreColumns);
        //列数行数
        int InceptionRow = 1; int InceptionRol = 1;
        // 判断是否有Title
        if (!string.IsNullOrEmpty(exportOptions.Title))
        {
            //生成标题
            int titleColSpan = headList.Count;
            worksheet.Cells[1, 1, 1, titleColSpan].Merge = true;
            worksheet.Cells[1, 1, 1, titleColSpan].Value = exportOptions.Title;
            worksheet.Cells[1, 1, 1, titleColSpan].Style.Font.Size = 16;
            worksheet.Cells[1, 1, 1, titleColSpan].Style.Font.Bold = true;
            worksheet.Cells[1, 1, 1, titleColSpan].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1, 1, titleColSpan].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 1, 1, titleColSpan].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            //行高
            worksheet.Row(1).Height = 30;
            // 调整headRow
            headRow++;
            InceptionRow = 2;
        }
        //生成列头
        GenerateHeads(worksheet, exportOptions.Columns, exportOptions.IgnoreColumns, headRow, InceptionRow, InceptionRol);

        for (int col = 0; col < headList.Count; col++)
        {
            for (int row = 0; row < exportOptions.Rows.Count; row++)
            {
                var temp = exportOptions.Rows[row];
                if (temp.ContainsKey("@Indent") && int.TryParse(temp["@Indent"]?.ToString() ?? "0", out int indent))
                {
                    worksheet.Cells[row + headRow + 1, col + 1].Style.Indent = indent * 2;
                }
                if (temp.TryGetValue(headList[col].Field, out object? value))
                {
                    // 如果是日期类型，设置日期格式
                    if (value is DateTime tempTime)
                    {
                        worksheet.Cells[row + headRow + 1, col + 1].Value = tempTime.ToString();
                        worksheet.Cells[row + headRow + 1, col + 1].Style.Numberformat.Format = "yyyy年mm月dd日";
                    }
                    // 否则将值写入单元格，如果值为 null，则写入空字符串
                    else
                    {
                        worksheet.Cells[row + headRow + 1, col + 1].Value = value ?? "";
                    }
                }
            }
        }

        if (exportOptions.Aggregate)
        {
            var sRow = headRow + 1;
            var eRow = exportOptions.Rows.Count + headRow;

            worksheet.SetValue(eRow + 1, 1, "合计");
            for (int col = 2; col <= headList.Count; col++)
            {
                var sum = $"SUM({new ExcelAddress(sRow, col, eRow, col).Address})";
                worksheet.Cells[eRow + 1, col].Formula = $"IF({sum}=0, \"\", {sum})";
            }
        }

        #region 样式设置

        for (int row = worksheet.Dimension.Start.Row; row <= worksheet.Dimension.End.Row; row++)
        {
            if (!string.IsNullOrEmpty(exportOptions.Title) && row == 1) continue;
            worksheet.Row(row).Height = 20;
            for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
            {
                var horizontalAlignment = ExcelHorizontalAlignment.Center;
                if (col - 1 >= 0 && col <= headList.Count)
                {
                    horizontalAlignment = (headList.ElementAt(col - 1).Alignment?.ToUpper()) switch
                    {
                        "LEFT" => ExcelHorizontalAlignment.Left,
                        "RIGHT" => ExcelHorizontalAlignment.Right,
                        "CENTER" => ExcelHorizontalAlignment.Center,
                        _ => ExcelHorizontalAlignment.Center,
                    };
                }

                worksheet.Cells[row, col].Style.HorizontalAlignment = horizontalAlignment;
                worksheet.Cells[row, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
        }

        worksheet.View.FreezePanes(headRow + 1, 1);
        worksheet.Cells.AutoFitColumns();
        for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
        {
            var width = 10d;
            if (col - 1 >= 0 && col <= headList.Count)
                width = Enumerable.ElementAt(headList, col - 1)?.Width ?? width;

            worksheet.Column(col).Width = width;
        }
        #endregion

    }
    /// <summary>
    /// 获得列头
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="ignoreColumns"></param>
    /// <returns></returns>
    private static List<ExcelColumn> GetHeads(List<ExcelColumn> columns, List<string> ignoreColumns)
    {
        List<ExcelColumn> heads = new List<ExcelColumn>();
        foreach (var item in columns.OrderBy(w => w.Sort))
        {
            if (!ignoreColumns.Contains(item.Field) && !ignoreColumns.Contains(item.Title))
            {
                if (item.Children?.Any() != true)
                    heads.Add(item);
                else
                    heads.AddRange(GetHeads(item.Children, ignoreColumns));
            }
        }
        return heads;
    }
    /// <summary>
    /// 生成excel列头
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="columns"></param>
    /// <param name="ignoreColumns"></param>
    /// <param name="headRow"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    private static void GenerateHeads(ExcelWorksheet worksheet, List<ExcelColumn> columns, List<string> ignoreColumns, int headRow, int row, int col)
    {
        int colIndex = col;
        foreach (var item in columns.OrderBy(w => w.Sort))
        {
            if (!ignoreColumns.Contains(item.Title) && !ignoreColumns.Contains(item.Field))
            {
                int children = GetMinChildCount(item.Children, 0);
                worksheet.Cells[row, colIndex].Value = string.IsNullOrEmpty(item.Title) ? item.Field : item.Title;
                if (!string.IsNullOrWhiteSpace(item.Comment))
                    worksheet.Cells[row, colIndex].AddComment(item.Comment);

                worksheet.Cells[row, colIndex].Style.Font.Bold = true;
                if (children != 0)
                {
                    worksheet.Cells[row, colIndex, row, colIndex + children - 1].Merge = true;

                    GenerateHeads(worksheet, item.Children, ignoreColumns, headRow, row + 1, colIndex);
                    colIndex += children;
                }
                else
                {
                    worksheet.Cells[row, colIndex, headRow, colIndex].Merge = true;
                    colIndex++;
                }
            }

        }
    }
    /// <summary>
    /// 获取树最大深度
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="all"></param>
    /// <param name="level"></param>
    private static void GetTreeCount(List<ExcelColumn> columns, List<int> all, int level = 1)
    {
        foreach (var item in columns.Select(w => w.Children))
        {
            if (item?.Any() == true)
            {
                GetTreeCount(item, all, level + 1);
            }
            else
                all.Add(level);
        }
    }
    /// <summary>
    /// 获得最小的子集数量
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    private static int GetMinChildCount(List<ExcelColumn> columns, int count)
    {
        if (columns?.Any() == true)
        {
            foreach (var col in columns.Select(w => w.Children))
            {
                if (col?.Any() != true)
                    count++;
                else
                    count = GetMinChildCount(col, count);
            }
        }
        return count;
    }

    #endregion

}

/// <summary>
/// 导出Excel配置
/// </summary>
public class ExcelExportOption
{
    /// ctor
    public ExcelExportOption()
    {
        Rows = new List<Dictionary<string, object>>();
        Columns = new List<ExcelColumn>();
        IgnoreColumns = new List<string>();
    }
    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }
    /// <summary>
    /// 工作表名
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 数据
    /// </summary>
    public List<Dictionary<string, object>> Rows { get; set; }
    /// <summary>
    /// 列头
    /// </summary>
    public List<ExcelColumn> Columns { get; set; }
    /// <summary>
    /// 不需要导出的列
    /// </summary>
    public List<string> IgnoreColumns { get; set; }
    /// <summary>
    /// 添加汇总
    /// </summary>
    public bool Aggregate { get; set; } = false;
}

/// <summary>
/// 导出Excel列头配置
/// </summary>
public class ExcelColumn
{
    /// <inheritdoc/>
    public ExcelColumn()
    {
    }

    /// ctor
    public ExcelColumn(string title, string field, double width = 20, string alignment = "Center", decimal sort = 0m, List<ExcelColumn>? children = null, string comment = "")
    {
        Title = title;
        Field = field;
        Width = width;
        Alignment = alignment;
        Sort = sort;
        Comment = comment;
        Children = children ?? new List<ExcelColumn>();
    }
    /// <summary>
    /// 显示名称
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 字段名称
    /// </summary>
    public string Field { get; set; }
    /// <summary>
    /// 列宽
    /// </summary>
    public double? Width { get; set; }
    /// <summary>
    /// Left,Center,Right,
    /// </summary>
    public string? Alignment { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public string Comment { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public decimal Sort { get; set; }
    /// <summary>
    /// 子列头
    /// </summary>
    public List<ExcelColumn> Children { get; set; }
}
