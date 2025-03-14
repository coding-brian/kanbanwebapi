﻿using AutoMapper;
using KanbanWebApi.Dto;
using KanbanWebApi.Dto.Column;
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

            CreateMap<Column, ColumnDto>();
            CreateMap<UpdateColumnDto, Column>();

            CreateMap<TaskDto, Tables.Task>();
            CreateMap<Tables.Task, TaskDto>();

            CreateMap<CreateTaskDto, Tables.Task>();
            CreateMap<CreateSubTaskDto, SubTask>();
            CreateMap<SubTaskDto, SubTask>();
            CreateMap<SubTask, SubTaskDto>();
            CreateMap<UpdateTaskDto, Tables.Task>();
            CreateMap<UpdateSubTaskDto, SubTask>();
            CreateMap<CreateColumnDto, Column>();
        }
    }
}