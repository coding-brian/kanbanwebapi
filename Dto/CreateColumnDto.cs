namespace KanbanWebApi.Dto
{
    public class CreateColumnDto
    {
        public Guid BoardId { get; set; }
        public string Name { get; set; }
    }
}