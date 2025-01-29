FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PuppeteerPdfGenerator.Api/PuppeteerPdfGenerator.Api.csproj", "PuppeteerPdfGenerator.Api/"]
RUN dotnet restore "./PuppeteerPdfGenerator.Api/PuppeteerPdfGenerator.Api.csproj"
COPY . .
WORKDIR "/src/PuppeteerPdfGenerator.Api"
RUN dotnet build "./PuppeteerPdfGenerator.Api.csproj" -c Release -o /app/build
RUN dotnet publish "./PuppeteerPdfGenerator.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM zenika/alpine-chrome AS final
USER root
ENV PUPPETEER_SKIP_DOWNLOAD=true
ENV PUPPETEER_SKIP_CHROMIUM_DOWNLOAD=true
ENV PUPPETEER_CHROMIUM_PATH=/usr/bin/chromium
RUN apk add --no-cache --repository=https://dl-cdn.alpinelinux.org/alpine/edge/community \
      tini aspnetcore8-runtime
WORKDIR /app
COPY --from=build /app/publish .
USER chrome
ENTRYPOINT ["tini", "--", "dotnet", "PuppeteerPdfGenerator.Api.dll"]
