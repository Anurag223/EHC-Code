REM download CLI tools ZIP archive here
REM https://github.com/RicoSuter/NSwag/wiki/CommandLine

REM see options here
REM https://github.com/RicoSuter/NSwag/blob/master/src/NSwag.Commands/Commands/CodeGeneration/OpenApiToCSharpClientCommand.cs

REM swagger.json is taken from masterData V3 api : https://api.test.tlm.slb.com/api-doc/v3/masterdata/index.html

del EpicV3ApiClient.cs

C:\NSwag\nswag openapi2csclient ^
  /input:swagger.json ^
  /output:EpicV3ApiClient.cs ^
  /namespace:TLM.EHC.Common.Clients.EpicV3Api ^
  /classname:EpicV3ApiClient ^
  /GenerateClientInterfaces:true ^

pause
@pause