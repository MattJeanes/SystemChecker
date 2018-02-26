# Dockerfile

# yarn
FROM gcr.io/cloud-builders/nodejs/yarn AS yarn
WORKDIR /yarn
COPY SystemChecker.Web/ ./
RUN yarn --pure-lockfile
RUN yarn run publish

# build project
FROM gcr.io/cloud-builders/csharp/dotnet AS builder
WORKDIR /build

COPY . .
COPY --from=yarn /yarn/wwwroot/dist/ ./SystemChecker.Web/wwwroot/dist/
RUN dotnet restore --configfile ./.nuget/nuget.config
RUN dotnet publish ./SystemChecker.Web/SystemChecker.Web.csproj --output /app/ --configuration Release

# final
FROM gcr.io/google-appengine/aspnetcore:2.0
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SystemChecker.Web.dll"]
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80:80