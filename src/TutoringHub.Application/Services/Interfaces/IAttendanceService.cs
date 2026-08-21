using TutoringHub.Application.DTOs.Attendance;
using TutoringHub.Application.DTOs.Sessions;

namespace TutoringHub.Application.Services.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceRosterDto> GetRosterAsync(int classGroupId, DateOnly date, CancellationToken cancellationToken = default);
    Task<AttendanceEntryDto> TickAsync(int classGroupId, int studentId, DateOnly date, CancellationToken cancellationToken = default);
    Task UntickAsync(int classGroupId, int studentId, DateOnly date, CancellationToken cancellationToken = default);
    Task<List<ClassSessionDto>> GetSessionsAsync(int classGroupId, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
}