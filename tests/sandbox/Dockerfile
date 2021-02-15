# Run from repository root:
# docker run --rm $(docker build -q -f ./tests/sandbox/Dockerfile .)
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
WORKDIR /build

COPY tests/sandbox/PG.WebApi.Tests.Sandbox/PG.WebApi.Tests.Sandbox.csproj tests/sandbox/PG.WebApi.Tests.Sandbox/
RUN dotnet restore tests/sandbox/PG.WebApi.Tests.Sandbox/

COPY tests/sandbox tests/sandbox
RUN dotnet build ./tests/sandbox/PG.WebApi.Tests.Sandbox/

ENV PaymentGatewayUri=http://host.docker.internal:5000
ENTRYPOINT dotnet test ./tests/sandbox/PG.WebApi.Tests.Sandbox/ --logger:"console;verbosity=normal"
