using FeedbackBoard.Core.Entities;

namespace FeedbackBoard.Functions.Services.Interfaces;

public interface ITableStorageService
{
    Task LogAuditAsync(AuditLog log);
    Task<List<AuditLog>> GetAllAuditLogsAsync();
}
