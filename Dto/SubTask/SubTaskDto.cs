namespace KanbanWebApi.Dto.SubTask
{
    public class SubTaskDto : EntityDto
    {
        public Guid TaskId { get; set; }

        public string Title { get; set; }

        public bool IsCompleted { get; set; }
    }
}