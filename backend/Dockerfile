FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos solo los archivos de proyecto primero para aprovechar la cache de Docker
COPY Altairis.API/Altairis.API.csproj Altairis.API/
COPY Altairis.Application/Altairis.Application.csproj Altairis.Application/
COPY Altairis.Domain/Altairis.Domain.csproj Altairis.Domain/
COPY Altairis.Infrastructure/Altairis.Infrastructure.csproj Altairis.Infrastructure/

RUN dotnet restore Altairis.API/Altairis.API.csproj

# Copiamos el resto del código
COPY . .

RUN dotnet publish Altairis.API/Altairis.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:5000

EXPOSE 5000

ENTRYPOINT ["dotnet", "Altairis.API.dll"]

