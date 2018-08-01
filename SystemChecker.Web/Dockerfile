# Dockerfile

# yarn
FROM kkarczmarczyk/node-yarn AS yarn
WORKDIR /yarn
COPY SystemChecker.Web/ ./
RUN yarn --pure-lockfile
RUN yarn run publish

# build project
FROM microsoft/dotnet:2.1-sdk AS builder
WORKDIR /build

COPY . .
COPY --from=yarn /yarn/wwwroot/dist/ ./SystemChecker.Web/wwwroot/dist/
RUN dotnet restore --configfile ./.nuget/nuget.config
RUN dotnet publish ./SystemChecker.Web/SystemChecker.Web.csproj --output /app/ --configuration Release

# final
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SystemChecker.Web.dll"]
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80:80