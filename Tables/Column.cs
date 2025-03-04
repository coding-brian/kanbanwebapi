using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanWebApi.Tables
{
    [Table("column")]
    public class Column : Entity
    {
        [Column("board_id")]
        public Guid BoardId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}