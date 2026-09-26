FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["EXAMENPARCIAL.csproj", "./"]
RUN dotnet restore "EXAMENPARCIAL.csproj"
COPY . .
RUN dotnet build "EXAMENPARCIAL.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EXAMENPARCIAL.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EXAMENPARCIAL.dll"]