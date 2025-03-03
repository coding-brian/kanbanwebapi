using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanWebApi.Dto
{
    public class BoardDto
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("member_id")]
        public Guid MemberId { get; set; }
    }
}