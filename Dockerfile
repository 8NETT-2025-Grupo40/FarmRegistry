# ===============================
# Stage 1: Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

# Suporte à globalização (formatação de moeda, datas)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Segurança: roda como usuário não-root que já existe na imagem
USER app
WORKDIR /app

# Kestrel ouvindo em todas as interfaces na porta 5067
ENV ASPNETCORE_URLS=http://+:5067
EXPOSE 5067

# ===============================
# Stage 2: Build/Publish
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copia a solução e os .csproj para aproveitar o cache do Docker
COPY ["src/FarmRegistry.Api/FarmRegistry.Api.csproj", "src/FarmRegistry.Api/"]
COPY ["src/FarmRegistry.Application/FarmRegistry.Application.csproj", "src/FarmRegistry.Application/"]
COPY ["src/FarmRegistry.Domain/FarmRegistry.Domain.csproj", "src/FarmRegistry.Domain/"]
COPY ["src/FarmRegistry.Infrastructure/FarmRegistry.Infrastructure.csproj", "src/FarmRegistry.Infrastructure/"]

# Restaura dependências do projeto de API (puxa o grafo inteiro)
RUN dotnet restore "src/FarmRegistry.Api/FarmRegistry.Api.csproj"

# Copia o restante do código
COPY . .

# Publica a API (Release) para uma pasta única
WORKDIR "/src/src/FarmRegistry.Api"
RUN dotnet publish "FarmRegistry.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ===============================
# Stage 3: Final (imagem enxuta)
# ===============================
FROM base AS final
WORKDIR /app

# Copia artefatos publicados
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FarmRegistry.Api.dll"]
