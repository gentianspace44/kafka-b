namespace VPS.ControlCenter.Core.HollyTopUp
{
    public class AuditTrail
    {
        public AuditTrail()
        {
        }

        public int AuditTrailID { get; set; }
        public System.DateTime AuditTrailDateTime { get; set; }
        public int UserID { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Type { get; set; }
        public string IPAddress { get; set; }

    }
}
