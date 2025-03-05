using AutoMapper;
using KanbanWebApi.Dto;
using KanbanWebApi.Dto.Task;
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
            CreateMap<UpdateBoardDto, Board>();
            CreateMap<UpdateColumnDto, Column>();
            CreateMap<Column, ColumnDto>();
            CreateMap<TaskDto, Tables.Task>();
            CreateMap<CreateTaskDto, Tables.Task>();
            CreateMap<CreateSubTaskDto, SubTask>();
        }
    }
}