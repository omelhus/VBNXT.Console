param([String]$token)
dotnet new tool-manifest
dotnet tool install StrawberryShake.Tools --local
dotnet graphql init https://business.visma.net/api/graphql -n VismaBusinessClient -x Authorization="Bearer $token"