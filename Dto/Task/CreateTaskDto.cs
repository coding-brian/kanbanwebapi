using KanbanWebApi.Dto.SubTask;

namespace KanbanWebApi.Dto.Task
{
    public class CreateTaskDto
    {
        public Guid BoardId { get; set; }

        public Guid ColumnId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<CreateSubTaskDto> SubTasks { get; set; }
    }
}