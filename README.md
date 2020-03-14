# Event-Sauce

Just playing with AWS Serverless and CQRS/Event Sourcing.

## Getting Started

These instructions will get you a copy of the solution up and running on your local machine for development and testing purposes.

### Prerequisites

* [.NET Core SDK](https://dotnet.microsoft.com/download/dotnet-core)
* [Docker](https://docs.docker.com/docker-for-windows/)
* [AWS CLI](https://aws.amazon.com/cli/)
* [SAM Local](https://github.com/awslabs/aws-sam-cli)
* Cake installed - `dotnet tool install -g --version 0.33.0 Cake.Tool`

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
* [Fluent Assertions](https://fluentassertions.com/) 🧪
* [LocalStack](https://github.com/localstack/localstack) ☁

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
