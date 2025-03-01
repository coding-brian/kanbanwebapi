using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanWebApi.Tables
{
    public class Entity
    {
        [Column("id")]
        [Required]
        public Guid Id { get; set; }

        [Column("entity_status")]
        public bool EntityStatus { get; set; }

        [Column("creation_time")]
        public DateTime? CreationTime { get; set; }

        [Column("create_by")]
        public string CreateBy { get; set; }

        [Column("modification_time")]
        public DateTime? ModificationTime { get; set; }

        [Column("modify_by")]
        public string ModifyBy { get; set; }

        [Column("deletion_time")]
        public DateTime? DeletionTime { get; set; }

        [Column("delete_by")]
        public string DeleteBy { get; set; }
    }
}