FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/API/AIKnowledgeAssistant.API.csproj", "src/API/"]
COPY ["src/Application/AIKnowledgeAssistant.Application.csproj", "src/Application/"]
COPY ["src/Domain/AIKnowledgeAssistant.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/AIKnowledgeAssistant.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/API/AIKnowledgeAssistant.API.csproj"
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "AIKnowledgeAssistant.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AIKnowledgeAssistant.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIKnowledgeAssistant.API.dll"]
