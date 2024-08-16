using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VPS.ControlCenter.Core.Entities
{
    public class DynamicSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DynamicSettingId { get; set; }
        [MaxLength(200)]
        public required string Name { get; set; }
        [MaxLength(1000)]
        public required string Value { get; set; }
    }
}
