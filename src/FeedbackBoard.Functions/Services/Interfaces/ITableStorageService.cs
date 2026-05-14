using FeedbackBoard.Core.Models;

namespace FeedbackBoard.Functions.Services.Interfaces;

public interface ITableStorageService
{
    Task LogAuditAsync(AuditLog log);
    Task<List<AuditLog>> GetAllAuditLogsAsync();
}
