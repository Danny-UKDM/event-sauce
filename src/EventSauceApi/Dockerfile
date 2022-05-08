FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal-arm64v8 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal-arm64v8 AS build
WORKDIR /src
COPY EventSauceApi.csproj EventSauceApi/
RUN dotnet restore EventSauceApi/EventSauceApi.csproj
COPY . EventSauceApi/
ARG VERSION=1.0.0
RUN dotnet publish EventSauceApi/EventSauceApi.csproj -c Release --no-self-contained -o /app -p:Version=${VERSION}

FROM build AS testrunner
WORKDIR /test
COPY test/EventSauceApi.Tests/. .
ENTRYPOINT ["dotnet", "test", "--logger:trx"]

FROM base AS final
COPY --from=build /app .
ENTRYPOINT ["dotnet", "EventSauceApi.dll"]
