using KanbanWebApi.Dto.Column;

namespace KanbanWebApi.Dto
{
    public class UpdateBoardDto : EntityDto
    {
        public string Name { get; set; }

        public List<UpdateColumnDto> Columns { get; set; }
    }
}