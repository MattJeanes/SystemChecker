# Dockerfile

# build project
FROM microsoft/dotnet:2.1-sdk AS builder
WORKDIR /build

COPY . .
RUN dotnet restore --configfile ./.nuget/nuget.config
RUN dotnet publish ./SystemChecker.Service/SystemChecker.Service.csproj --output /app/ --configuration Release

# final
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SystemChecker.Service.dll"]