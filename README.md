# Event-Sauce [![CircleCI](https://circleci.com/gh/Danny-UKDM/event-sauce/tree/main.svg?style=svg&circle-token=41ca009b555604af36ce3887afa8e1cb64afb62c)](https://circleci.com/gh/Danny-UKDM/event-sauce/tree/main)

Just playing with AWS Serverless and CQRS/Event Sourcing.

## Getting Started

These instructions will get you a copy of the solution up and running on your local machine for development and testing.

### Prerequisites

* [.NET Core SDK](https://dotnet.microsoft.com/download/dotnet-core)
* [Docker](https://docs.docker.com/docker-for-windows/)
* [AWS CLI](https://aws.amazon.com/cli/)
* [SAM Local](https://github.com/awslabs/aws-sam-cli)
* [Cake](https://cakebuild.net/docs/getting-started/setting-up-a-new-project#choose-your-runner)

Note: Some environment dependencies can be installed simply using [Chocolatey](https://chocolatey.org/install).

Example: `choco install dotnetcore-sdk docker-desktop python awscli`

### Using The Solution

This project uses [Cake](https://cakebuild.net/) as a build tool:

* Clean, restore and build all projects with `dotnet cake --target=build`.

* Package all projects with `dotnet cake --target=package`.

* Run all tests with `dotnet cake --target-test-e2e`.

* Run the API locally with `dotnet cake --target=start-api`.

Running the API locally will provision necessary infrastructure before making the HTTP endpoints available.

## Built With

* [AWS Lambda for .NET Core](https://github.com/aws/aws-lambda-dotnet) 👨‍💻
* [Cake](https://cakebuild.net/) 🏗
* [Docker](https://docs.docker.com/docker-for-windows/) 🐳
* [LocalStack](https://github.com/localstack/localstack) ☁

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
