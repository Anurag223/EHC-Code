// ReSharper disable once CheckNamespace
namespace TLM.EHC.Common.Exceptions
{
    public static class ExceptionTranslator
    {
        public static HttpStatusException GetExceptionType(string message)
        {
            if(message.ToLower().Contains(EhcConstants.PartialWriteError.ToLower()))
            {
                return new BadRequestException($"{EhcConstants.WriteSuffix}{message}");
            }
            return new ServerErrorException($"{EhcConstants.WriteSuffix}{message}");
        }
    }
}
