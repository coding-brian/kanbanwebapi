namespace KanbanWebApi.Dto
{
    public class TaskDto : EntityDto
    {
        public Guid ColumnId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}