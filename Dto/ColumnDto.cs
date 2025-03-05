namespace KanbanWebApi.Dto
{
    public class ColumnDto : EntityDto
    {
        public Guid BoardId { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public List<TaskDto> Tasks { get; set; } = [];
    }
}