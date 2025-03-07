using KanbanWebApi.Dto.SubTask;

namespace KanbanWebApi.Dto.Task
{
    public class UpdateTaskDto : EntityDto
    {
        public Guid ColumnId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<UpdateSubTaskDto> SubTasks { get; set; } = [];
    }
}