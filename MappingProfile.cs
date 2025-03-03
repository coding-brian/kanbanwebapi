using AutoMapper;
using KanbanWebApi.Dto;
using KanbanWebApi.Tables;

namespace KanbanWebApi
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateBoardDto, Board>();
            CreateMap<Board, BoardDto>();

            CreateMap<CreatColumnDto, Column>();
        }
    }
}