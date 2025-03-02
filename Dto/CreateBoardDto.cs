namespace KanbanWebApi.Dto
{
    public class CreateBoardDto
    {
        public string Name { get; set; }
        public Guid MemberId { get; set; }
    }
}