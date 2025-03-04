namespace KanbanWebApi.Dto
{
    public class BoardDto : EntityDto
    {
        public string Name { get; set; }

        public Guid MemberId { get; set; }

        public List<ColumnDto> Columns { get; set; } = new();
    }
}