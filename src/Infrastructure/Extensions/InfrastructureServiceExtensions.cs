using AIKnowledgeAssistant.Application.Interfaces;
using AIKnowledgeAssistant.Domain.Interfaces;
using AIKnowledgeAssistant.Infrastructure.Persistence;
using AIKnowledgeAssistant.Infrastructure.Repositories;
using AIKnowledgeAssistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace AIKnowledgeAssistant.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // Document text extractors — each registered individually so the factory can enumerate them.
        services.AddSingleton<IDocumentExtractor, PdfExtractor>();
        services.AddSingleton<IDocumentExtractor, TxtExtractor>();
        services.AddSingleton<IDocumentExtractor, DocxExtractor>();
        services.AddSingleton<IDocumentExtractorFactory, DocumentExtractorFactory>();

        // Text chunking
        services.AddSingleton<ITextChunkingService, TextChunkingService>();

        // Ollama — embeddings
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient<ITextGenerationService, OllamaLlmService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        // Semantic Kernel core services.
        services.AddKernel();

        // Qdrant — vector store
        services.Configure<QdrantOptions>(configuration.GetSection(QdrantOptions.SectionName));
        services.AddSingleton<QdrantClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
            return new QdrantClient(opts.Host, opts.Port, opts.Https, opts.ApiKey);
        });

        // Semantic Kernel memory connector backed by Qdrant vector store.
        services.AddQdrantVectorStore(
            sp => sp.GetRequiredService<QdrantClient>(),
            _ => new QdrantVectorStoreOptions(),
            ServiceLifetime.Singleton);

        services.AddScoped<IVectorRepository, QdrantVectorRepository>();

        return services;
    }
}
