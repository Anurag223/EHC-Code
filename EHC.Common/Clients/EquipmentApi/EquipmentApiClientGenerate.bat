REM download CLI tools ZIP archive here
REM https://github.com/RicoSuter/NSwag/wiki/CommandLine

REM see options here
REM https://github.com/RicoSuter/NSwag/blob/master/src/NSwag.Commands/Commands/CodeGeneration/OpenApiToCSharpClientCommand.cs

del EquipmentApiClient.cs

C:\NSwag\nswag openapi2csclient ^
  /input:swagger.json ^
  /output:EquipmentApiClient.cs ^
  /namespace:TLM.EHC.Common.Clients.EquipmentApi ^
  /classname:EquipmentApiClient ^
  /GenerateClientInterfaces:true ^

pause
@pause