FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AutoMender.Web/AutoMender.Web.csproj", "AutoMender.Web/"]
RUN dotnet restore "AutoMender.Web/AutoMender.Web.csproj"
COPY . .
WORKDIR "/src/AutoMender.Web"
RUN dotnet publish "AutoMender.Web.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY AutoMender.Web/nginx.conf /etc/nginx/conf.d/default.conf
