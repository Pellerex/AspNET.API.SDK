FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
RUN apt-get update && apt-get install -y libgdiplus

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER 0

WORKDIR /src

COPY ["Api/LanguageProcessingApi.csproj", "Api/"]
COPY ["Business/Business.csproj", "Business/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["ML/ML.csproj", "ML/"]

RUN dotnet restore \
	-s https://api.nuget.org/v3/index.json \
	"Api/LanguageProcessingApi.csproj"

COPY . . 

RUN dotnet build "Api/LanguageProcessingApi.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "Api/LanguageProcessingApi.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "LanguageProcessingApi.dll"]