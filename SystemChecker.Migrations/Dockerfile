# Dockerfile
ARG arch

# build project
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder
WORKDIR /build

COPY . .
RUN dotnet restore --configfile ./.nuget/nuget.config
RUN dotnet publish ./SystemChecker.Migrations/SystemChecker.Migrations.csproj --output /app/ --configuration Release

# final
FROM mcr.microsoft.com/dotnet/core/runtime:2.2-${arch} as final
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SystemChecker.Migrations.dll"]