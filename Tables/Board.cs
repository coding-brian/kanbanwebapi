using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanWebApi.Tables
{
    public class Board : Entity
    {
        [Column("name")]
        [Required]
        public string Name { get; set; }

        [Column("member_id")]
        [Required]
        public Guid MemberId { get; set; }
    }
}