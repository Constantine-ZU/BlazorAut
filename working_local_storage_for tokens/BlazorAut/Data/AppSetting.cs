using System.ComponentModel.DataAnnotations;

namespace BlazorAut.Data
{
    public class AppSetting
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}