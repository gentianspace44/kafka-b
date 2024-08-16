# VPS Control Center


## Run the below commands at the solution level
## Add new migrations
dotnet ef migrations add FeatureAndSettingsTables --project  VPS.ControlCenter.Core --startup-project VPS.ControlCenter.Api

## Update Target DB
dotnet ef database update --project  VPS.ControlCenter.Core --startup-project VPS.ControlCenter.Api

## Remove Last Migrations
dotnet ef migrations remove --project  VPS.ControlCenter.Core --startup-project VPS.ControlCenter.Api
