FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AutoMender.Core/AutoMender.Core.csproj", "AutoMender.Core/"]
RUN dotnet restore "AutoMender.Core/AutoMender.Core.csproj"
COPY . .
WORKDIR "/src/AutoMender.Core"
RUN dotnet build "AutoMender.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutoMender.Core.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated9.0 AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
