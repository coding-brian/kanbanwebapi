using KanbanWebApi.Dto.Column;

namespace KanbanWebApi.Dto
{
    public class CreateBoardDto
    {
        public string Name { get; set; }

        public Guid? MemberId { get; set; }

        public IList<CreateColumnDto> Columns { get; set; }
    }
}