# Dockerfile

# yarn
FROM node AS node
WORKDIR /yarn
COPY SystemChecker.Web/ ./
RUN yarn --pure-lockfile
RUN yarn run publish

# build project
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder
WORKDIR /build

COPY . .
COPY --from=node /yarn/wwwroot/dist/ ./SystemChecker.Web/wwwroot/dist/
RUN dotnet restore --configfile ./.nuget/nuget.config
RUN dotnet publish ./SystemChecker.Web/SystemChecker.Web.csproj --output /app/ --configuration Release

# final
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SystemChecker.Web.dll"]
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80:80