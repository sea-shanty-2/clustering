# Build build environment
FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish EnvueClustering/EnvueClusteringAPI/ -c Release -o ../../out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "EnvueClusteringAPI.dll"]
