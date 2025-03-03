namespace KanbanWebApi.Dto
{
    public class CreateBoardDto
    {
        public string Name { get; set; }

        public Guid? MemberId { get; set; }

        public IList<CreatColumnDto> Colums { get; set; }
    }
}