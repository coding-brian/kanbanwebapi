﻿using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanWebApi.Tables
{
    [Table("task")]
    public class Task : Entity
    {
        [Column("column_id")]
        public Guid ColumnId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }
}