FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/FCG.API/FCG.API.csproj", "src/FCG.API/"]
COPY ["src/FCG.Application/FCG.Application.csproj", "src/FCG.Application/"]
COPY ["src/FCG.Domain/FCG.Domain.csproj", "src/FCG.Domain/"]
COPY ["src/FCG.Infrastructure/FCG.Infrastructure.csproj", "src/FCG.Infrastructure/"]
RUN dotnet restore "src/FCG.API/FCG.API.csproj"

COPY . .
RUN dotnet publish "src/FCG.API/FCG.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.API.dll"]
