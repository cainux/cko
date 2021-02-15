# Payment Gateway

## Required

- docker
- docker-compose

## Running

Start the containers with:

    docker-compose up -d --build

Give it a moment to build and spin up. Unit tests are run as part of the build.

Once ready, you can browse to the following:

- Swagger UI: http://localhost:5000/swagger
- Seq: http://localhost:5342/

You can interact with the Payment Gateway using the Swagger UI and view the logs in Seq.

## Sandbox Tests

There is also a Dockerfile to run sandbox tests. The following command will execute them:

    docker run --rm $(docker build -q -f ./tests/sandbox/Dockerfile .)

(should work in both PowerShell and bash)

## Cleaning Up

    docker-compose down

## Developer Notes

Built on/with:

- Windows 10
- Linux (via WSL2)
- JetBrains Rider
- .NET Core SDK 3.1.405
- PowerShell/bash

### Things to add/discuss

- Metrics
- Authentication
- Performance testing
- Encryption