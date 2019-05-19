FROM mcr.microsoft.com/dotnet/core/sdk:latest AS builder
ARG BUILD_VERSION  
WORKDIR /source

COPY . .
RUN dotnet publish /p:Version=$BUILD_VERSION --output /app/ --configuration Release

# Stage 2
FROM mcr.microsoft.com/dotnet/core/aspnet:latest
WORKDIR /app
COPY --from=builder /app .
ENV ASPNETCORE_ENVIRONMENT Production

ENTRYPOINT ["dotnet", "Wikiled.Twitter.Monitor.Service.dll"]

