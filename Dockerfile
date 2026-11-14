FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ComercioTecnologia.Api.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish ComercioTecnologia.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
# Render/Railway definen PORT; por defecto 8080
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet ComercioTecnologia.Api.dll"]
