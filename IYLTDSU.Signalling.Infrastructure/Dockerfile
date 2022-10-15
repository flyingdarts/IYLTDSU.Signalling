FROM mcr.microsoft.com/dotnet/nightly/sdk:7.0 AS dotnetSdk

WORKDIR /app

COPY . .

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/nightly/aspnet:7.0

RUN apt-get update -yq && apt-get upgrade -yq && apt-get install -yq curl git nano
RUN curl -sL https://deb.nodesource.com/setup_16.x | bash - && apt-get install -yq nodejs build-essential
WORKDIR /app

COPY --from=dotnetSdk /app/out .

RUN npm install -g aws-cdk

COPY ./cdk.json .
COPY ./lambda.zip .

ENTRYPOINT [ "cdk", "diff" ]