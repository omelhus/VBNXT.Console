param([String]$token)
dotnet graphql update -x Authorization="Bearer $token"