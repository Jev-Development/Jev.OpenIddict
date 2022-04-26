namespace Jev.OpenIddict.Domain.Dto
{
    public class ErrorResponseDto
    {
        public string Message { get; set; } = null!;
        public int ResponseCode { get; set; }
    }
}
