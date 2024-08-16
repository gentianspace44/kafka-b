namespace VPS.Domain.Models.Common
{
    public abstract class OutComeBase
    {
        public int OutComeTypeId { get; set; }

        public required string OutcomeMessage { get; set; }

    }
}
