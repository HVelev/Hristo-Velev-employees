using System;
using EmployeeCollaborationUI.Data;

namespace EmployeeCollaborationUI.Interfaces {
    public interface IEmployeeCollaborationService {
        IEnumerable<Assignment> ReadAssignments(IFormFile assignmentsBlob, string format);
        CollaborationResult FindLongestCollaboration(IEnumerable<Assignment> assignments);
    }
}