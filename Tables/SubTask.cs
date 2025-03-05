using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanWebApi.Tables
{
    [Table("sub_task")]
    public class SubTask : Entity
    {
        [Column("task_id")]
        public Guid TaskId { get; set; }

        [Column("title")]
        public string Title { get; set; }
    }
}