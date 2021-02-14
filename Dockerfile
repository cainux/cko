FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /build

COPY src/PG.Adapters/PG.Adapters.csproj src/PG.Adapters/
COPY src/PG.Core/PG.Core.csproj src/PG.Core/
COPY src/PG.WebApi/PG.WebApi.csproj src/PG.WebApi/

COPY tests/unit/PG.Core.Tests.Unit/PG.Core.Tests.Unit.csproj tests/unit/PG.Core.Tests.Unit/
COPY tests/unit/PG.WebApi.Tests.Unit/PG.WebApi.Tests.Unit.csproj tests/unit/PG.WebApi.Tests.Unit/

RUN dotnet restore src/PG.WebApi/
RUN dotnet restore tests/unit/PG.Core.Tests.Unit/
RUN dotnet restore tests/unit/PG.WebApi.Tests.Unit/

COPY src src
COPY tests/unit tests/unit

RUN dotnet test tests/unit/PG.Core.Tests.Unit/ --logger:"console;verbosity=normal"
RUN dotnet test tests/unit/PG.WebApi.Tests.Unit/ --logger:"console;verbosity=normal"

RUN dotnet publish src/PG.WebApi/ -c Release -o /build/publish

FROM base AS final
WORKDIR /app
COPY --from=build /build/publish .
ENTRYPOINT dotnet PG.WebApi.dll
