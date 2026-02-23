namespace EmployeeCollaborationUI.Data 
{
    public record EmployeePair(int EmpA, int EmpB, long TotalDays)
    {
        public override string ToString() =>
            $"{Math.Min(EmpA, EmpB)}, {Math.Max(EmpA, EmpB)}, {TotalDays}";
    }
}