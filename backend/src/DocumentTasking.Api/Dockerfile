FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY DocumentTasking.sln ./
COPY src/DocumentTasking.Api/ ./src/DocumentTasking.Api/
COPY tests/ ./tests/
RUN dotnet restore ./src/DocumentTasking.Api/DocumentTasking.Api.csproj
RUN dotnet publish ./src/DocumentTasking.Api/DocumentTasking.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DocumentTasking.Api.dll"]
