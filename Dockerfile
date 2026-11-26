# Etapa 1: Build (.NET 10 SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar los archivos .csproj exactos de tu solución
COPY ["Backend_TecniHub/Backend_TecniHub.csproj", "Backend_TecniHub/"]
COPY ["TecniHub.Application/TecniHub.Application.csproj", "TecniHub.Application/"]
COPY ["TecniHub.Domain/TecniHub.Domain.csproj", "TecniHub.Domain/"]
COPY ["TecniHub.Infrastructure/TecniHub.Infrastructure.csproj", "TecniHub.Infrastructure/"]

# Restaurar dependencias del proyecto principal
RUN dotnet restore "Backend_TecniHub/Backend_TecniHub.csproj"

# Copiar todo el código fuente y compilar en Release
COPY . .
WORKDIR "/src/Backend_TecniHub"
RUN dotnet publish "Backend_TecniHub.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime (.NET 10 ASP.NET)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Backend_TecniHub.dll"]
