//Model/Users.cs
using Auth.API.Data;
using Auth.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Auth.API.Models
{
    [Table("GS_users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Column("phone")]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("password_hash")]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Column("role")]
        [StringLength(20)]
        public string Role { get; set; }

        [Column("is_active")]
        [StringLength(1)]
        public string IsActive { get; set; } = "Y";

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("organization_id")]
        public int? OrganizationId { get; set; }
    }
}
