FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DocumentManagementAPI.csproj", "."]
RUN dotnet restore "DocumentManagementAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "DocumentManagementAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocumentManagementAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for uploads and logs
RUN mkdir -p uploads logs

ENTRYPOINT ["dotnet", "DocumentManagementAPI.dll"]