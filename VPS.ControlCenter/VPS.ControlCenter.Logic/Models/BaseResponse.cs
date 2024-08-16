namespace VPS.ControlCenter.Logic.Models
{
    public class BaseResponse
    {
        public bool? IsSuccess { get; internal set; }
        public string? Message { get; internal set; }
        public string[]? ErrorMessages { get; internal set; }
    }
}
