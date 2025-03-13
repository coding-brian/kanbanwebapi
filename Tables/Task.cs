using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanWebApi.Tables
{
    [Table("task")]
    public class Task : Entity
    {
        [Column("column_id")]
        public Guid ColumnId { get; set; }

        [Column("board_id")]
        public Guid BoardId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("priority")]
        public int Priority { get; set; }
    }
}