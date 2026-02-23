using EmployeeCollaborationUI.Interfaces;
using EmployeeCollaborationUI.Data;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EmployeeCollaborationUI.Services
{
    public class EmployeeCollaborationService : IEmployeeCollaborationService
    {
        public IEnumerable<Assignment> ReadAssignments(IFormFile assignmentBlob, string format)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null,
                HasHeaderRecord = false,
            };

            using var stream = assignmentBlob.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap(new AssignmentNoHeaderMap(format));

            return csv.GetRecords<Assignment>()
                      .Where(a => a.EmpId > 0 && a.ProjectId > 0 && a.DateFrom != default)
                      .ToList();
        }

        public CollaborationResult FindLongestCollaboration(IEnumerable<Assignment> assignments)
        {
            var normalised = assignments.Select(a => new
            {
                a.EmpId,
                a.ProjectId,
                Start = a.DateFrom,
                End = a.DateTo ?? DateTime.Now
            });

            var projectToEmployeeIntervals = normalised
                .GroupBy(x => x.ProjectId)
                .ToDictionary(
                    pg => pg.Key,
                    pg => pg.GroupBy(x => x.EmpId)
                            .ToDictionary(
                                eg => eg.Key,
                                eg => MergeIntervals(eg.Select(x => (x.Start, x.End)))
                            ));

            var pairContributions = projectToEmployeeIntervals
                .SelectMany(proj =>
                    from e1 in proj.Value.Keys
                    from e2 in proj.Value.Keys
                    where e1 < e2
                    let days = CalculateTotalOverlapDays(proj.Value[e1], proj.Value[e2])
                    where days > 0
                    select new
                    {
                        EmpA = e1,
                        EmpB = e2,
                        ProjectId = proj.Key,
                        OverlapDays = days
                    });

            var pairTotals = pairContributions
                .GroupBy(p => (p.EmpA, p.EmpB))
                .Select(g => new
                {
                    Pair = g.Key,
                    TotalDays = g.Sum(x => x.OverlapDays),
                    Projects = g
                        .Select(x => new ProjectContribution
                        {
                            ProjectId = x.ProjectId,
                            Days = x.OverlapDays
                        })
                        .OrderBy(p => p.ProjectId)
                        .ToList()
                })
                .OrderByDescending(x => x.TotalDays);

            var winner = pairTotals.FirstOrDefault();

            if (winner == null)
            {
                return new CollaborationResult();
            }

            return new CollaborationResult
            {
                Employee1 = Math.Min(winner.Pair.Item1, winner.Pair.Item2),
                Employee2 = Math.Max(winner.Pair.Item1, winner.Pair.Item2),
                TotalDays = winner.TotalDays,
                Contributions = winner.Projects
            };
        }

        private List<(DateTime start, DateTime end)> MergeIntervals(IEnumerable<(DateTime start, DateTime end)> ranges)
        {
            var sorted = ranges.OrderBy(r => r.start).ToList();

            if (sorted.Count == 0)
            {
                return new List<(DateTime start, DateTime end)>();
            }

            var merged = new List<(DateTime start, DateTime end)> { sorted[0] };

            foreach (var current in sorted.Skip(1))
            {
                var last = merged[^1];

                if (last.end >= current.start)
                {
                    merged[^1] = (last.start, last.end > current.end ? last.end : current.end);
                }
                else
                {
                    merged.Add(current);
                }
            }

            return merged;
        }

        private long CalculateTotalOverlapDays(List<(DateTime start, DateTime end)> a, List<(DateTime start, DateTime end)> b)
        {
            long total = 0;
            int i = 0, j = 0;

            while (i < a.Count && j < b.Count)
            {
                var left = a[i];
                var right = b[j];

                var overlapStart = left.start > right.start ? left.start : right.start;
                var overlapEnd = left.end < right.end ? left.end : right.end;

                if (overlapStart <= overlapEnd)
                {
                    total += (long)(overlapEnd - overlapStart).TotalDays + 1;
                }

                if (left.end < right.end)
                    i++;
                else
                    j++;
            }

            return total;
        }
    }
}