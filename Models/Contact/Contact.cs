using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Contacts
{
    public class Contact
    {
        [Key]
        public int Id{set;get;}

        [Column(TypeName ="nvarchar")]
        [StringLength(50)]
        [Required(ErrorMessage ="Phải nhập {0}")]
        [Display(Name ="Họ Tên")]
        public string? FullName{set;get;}

        [Required(ErrorMessage ="Phải nhập {0}")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage ="Phải là địa chỉ email")]
         [DisplayName("Địa chỉ email")]
        public string? Email{set;get;}

        public DateTime DateSent{set;get;}

        [DisplayName("Nội dung")]
        public string? Message{set;get;}

        [StringLength(50)]
        [Phone(ErrorMessage ="Phải là số điện thoại")]
        [DisplayName("Số điện thoại")]
        public string? Phone{set;get;}
    }
}