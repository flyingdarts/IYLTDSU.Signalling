FROM mcr.microsoft.com/dotnet/nightly/sdk:7.0 AS build-env

WORKDIR /src

COPY ./ ./

RUN dotnet restore

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/nightly/runtime:7.0

WORKDIR /src

COPY --from=build-env /src/out .

EXPOSE 5005

ENTRYPOINT [ "dotnet", "IYLTDSU.Signalling.OnDisconnect.dll" ]