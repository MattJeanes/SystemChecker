# Dockerfile

# build project
FROM gcr.io/cloud-builders/csharp/dotnet AS builder
WORKDIR /build

COPY . .
RUN dotnet restore
RUN dotnet publish ./SystemChecker.Service/SystemChecker.Service.csproj --output /app/ --configuration Release

# final
FROM gcr.io/google-appengine/aspnetcore:2.0
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SystemChecker.Service.dll"]