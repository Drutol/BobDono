Remove-Item .\Migrations -Force -Recurse
Remove-Item .\bob.db
dotnet ef migrations add ModelCreation
dotnet ef database update
Copy-Item ".\bin\Debug\netcoreapp2.0\bob.db" -Destination ".\bob.db"