FROM microsoft/dotnet:latest-sdk AS builder
ARG BUILD_VERSION  
WORKDIR /source

COPY . .
RUN dotnet publish /p:Version=$env:BUILD_VERSION --output /app/ --configuration Release

# Stage 2
FROM microsoft/dotnet:latest-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .
ENV ASPNETCORE_ENVIRONMENT Production

ENTRYPOINT ["dotnet", "Wikiled.Twitter.Monitor.Service.dll"]

