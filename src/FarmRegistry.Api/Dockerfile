# Imagem base oficial do .NET 8 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto e restaura dependências
COPY ["src/FarmRegistry.Api/FarmRegistry.Api.csproj", "FarmRegistry.Api/"]
COPY ["src/FarmRegistry.Application/FarmRegistry.Application.csproj", "FarmRegistry.Application/"]
COPY ["src/FarmRegistry.Domain/FarmRegistry.Domain.csproj", "FarmRegistry.Domain/"]
COPY ["src/FarmRegistry.Infrastructure/FarmRegistry.Infrastructure.csproj", "FarmRegistry.Infrastructure/"]
RUN dotnet restore "FarmRegistry.Api/FarmRegistry.Api.csproj"

# Copia o restante do código e publica
COPY src/ .
WORKDIR "/src/FarmRegistry.Api"
RUN dotnet publish "FarmRegistry.Api.csproj" -c Release -o /app/publish

# Imagem final para runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FarmRegistry.Api.dll"]