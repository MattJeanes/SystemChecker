# Dockerfile

# build project
FROM gcr.io/cloud-builders/csharp/dotnet AS builder
WORKDIR /build

COPY . .
RUN dotnet restore
RUN dotnet publish ./SystemChecker.Migrations.csproj --output /app/ --configuration Release

# final
FROM microsoft/dotnet
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SystemChecker.Migrations.dll"]