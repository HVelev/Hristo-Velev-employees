using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace EmployeeCollaborationUI.Data
{
    public sealed class AssignmentNoHeaderMap : ClassMap<Assignment>
    {
        public AssignmentNoHeaderMap(string dateFormat)
        {
            Map(m => m.EmpId).Index(0);
            Map(m => m.ProjectId).Index(1);

            Map(m => m.DateFrom)
                .Index(2)
                .TypeConverterOption.Format(dateFormat)
                .TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);

            Map(m => m.DateTo)
                .Index(3)
                .Convert(args =>
                {
                    var val = args.Row.GetField<string>(3)?.Trim();

                    if (string.IsNullOrWhiteSpace(val) ||
                        val.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    if (DateTime.TryParseExact(val, dateFormat,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    {
                        return dt;
                    }

                    return null;
                });
        }
    }
}