# Dockerfile

# build project
FROM microsoft/dotnet:2.1-sdk AS builder
WORKDIR /build

COPY . .
RUN dotnet restore
RUN dotnet publish ./SystemChecker.Migrations.csproj --output /app/ --configuration Release

# final
FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SystemChecker.Migrations.dll"]