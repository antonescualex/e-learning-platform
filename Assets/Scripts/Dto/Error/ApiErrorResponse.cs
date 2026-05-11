using System;

namespace Dto.Error
{
    [Serializable]
    public sealed class ApiErrorResponse
    {
        public string Code;
        public string Message;
        public string[] Details;
        public string CorrelationId;
    }
}