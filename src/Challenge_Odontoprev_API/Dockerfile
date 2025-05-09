FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Challenge_Odontoprev_API.csproj", "./"]
RUN dotnet restore "Challenge_Odontoprev_API.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Challenge_Odontoprev_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Challenge_Odontoprev_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Challenge_Odontoprev_API.dll"]