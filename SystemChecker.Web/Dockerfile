# Dockerfile
ARG arch

# yarn
#FROM node AS node
#WORKDIR /yarn
#COPY ./ClientApp ./
#RUN yarn --pure-lockfile --cwd ClientApp
#RUN yarn run publish

# build project
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder
WORKDIR /build

COPY . .
#COPY --from=node /yarn/ClientApp/dist/ ./SystemChecker.Web/ClientApp/dist/
RUN curl -sL https://deb.nodesource.com/setup_10.x | bash -
RUN apt-get install -y nodejs
RUN curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
RUN echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list
RUN apt-get update && apt-get install yarn
RUN dotnet restore --configfile ./.nuget/nuget.config
RUN dotnet publish ./SystemChecker.Web/SystemChecker.Web.csproj --output /app/ --configuration Release

# final
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-${arch} as final
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "SystemChecker.Web.dll"]
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80:80