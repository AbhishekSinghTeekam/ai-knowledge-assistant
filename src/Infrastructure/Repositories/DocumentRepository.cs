using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Interfaces;
using AIKnowledgeAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIKnowledgeAssistant.Infrastructure.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Document>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        // Only attach if detached — if the entity was loaded/added in the same DbContext
        // scope, EF already tracks all changes (including new child entities like Chunks).
        // Calling Update() on an already-tracked entity marks the entire graph as Modified,
        // which causes INSERT-able children to be sent as UPDATEs → 0 rows → concurrency error.
        if (_context.Entry(document).State == EntityState.Detached)
        {
            _context.Documents.Update(document);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FindAsync([id], cancellationToken);
        if (document is not null)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
