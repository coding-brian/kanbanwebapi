using AutoMapper;
using KanbanWebApi.Dto;
using KanbanWebApi.Dto.SubTask;
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
            CreateMap<UpdateBoardDto, Board>();

            CreateMap<CreatColumnDto, Column>();
            CreateMap<Column, ColumnDto>();
            CreateMap<UpdateColumnDto, Column>();

            CreateMap<TaskDto, Tables.Task>();
            CreateMap<Tables.Task, TaskDto>();

            CreateMap<CreateTaskDto, Tables.Task>();
            CreateMap<CreateSubTaskDto, SubTask>();
            CreateMap<SubTaskDto, SubTask>();
            CreateMap<SubTask, SubTaskDto>();
        }
    }
}