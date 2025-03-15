using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.TodoLists.Queries.GetTodos;

public record GetTodosQuery : IRequest<TodosVm>;

public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, TodosVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTodosQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        return new TodosVm
        {
            PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                .Cast<PriorityLevel>()
                .Select(p => new PriorityLevelDto { Value = (int)p, Name = p.ToString() })
                .ToList(),

            Lists = await _context.TodoLists
            .Where(t => !t.IsDeleted)
            .AsNoTracking()
            .Select(t => new TodoListDto
            {
                Id = t.Id,
                Title = t.Title,
                Items = t.Items.Where(i => !i.IsDeleted).Select(i => new TodoItemDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Done = i.Done,
                    Priority = (int)i.Priority,
                    Note = i.Note,
                    BackgroundColor = i.BackgroundColor
                }).ToList()
            })
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken),

    };
    }
}
