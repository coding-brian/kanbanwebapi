namespace KanbanWebApi.Dto.SubTask
{
    public class UpdateSubTaskDto
    {
        public Guid? Id { get; set; }

        public Guid TaskId { get; set; }

        public string Title { get; set; }
    }
}