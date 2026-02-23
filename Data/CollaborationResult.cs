namespace EmployeeCollaborationUI.Data {
public class CollaborationResult
    {
        public int Employee1 { get; set; }
        public int Employee2 { get; set; }
        public long TotalDays { get; set; }
        public List<ProjectContribution> Contributions { get; set; } = new();
    }
}