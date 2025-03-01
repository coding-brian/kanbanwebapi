using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanWebApi.Tables
{
    public class Board : Entity
    {
        [Column("name")]
        [Required]
        public string Name { get; set; }
    }
}