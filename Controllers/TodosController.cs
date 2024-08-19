using Azure.Core;
using DockerToDoTest.Context;
using DockerToDoTest.DTOs;
using DockerToDoTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace DockerToDoTest.Controllers;
[Route("api/[controller]/[action]")]
[ApiController]
public class TodosController(ApplicationDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateTodoDto request, CancellationToken cancellationToken)
    {
        Todo todo = new()
        {
            Note = request.Note,
            Complated = false,
            CreatedBy = "User",
            CreatedDate = DateTime.Now,
        };

        await context.AddAsync(todo,cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Ok("Not kaydedildi");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var todos = await context
            .Todos
            .Where(p => !p.IsDeleted)
            .OrderBy(o => o.CreatedDate)
            .ToListAsync(cancellationToken);
        return Ok(todos);
    }

    [HttpPost]
    public async Task<IActionResult> Update(UpdateTodoDto request, CancellationToken cancellationToken)
    {
        Todo? todo = await context
            .Todos
            .Where(p => p.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (todo is null)
        {
            throw new ArgumentException("Not bulunamadı");
        }

        todo.Note = request.Note;
        todo.Complated = request.Complated;
        todo.UpdatedBy = "User";
        todo.UpdatedDate = DateTime.Now;

        context.Update(todo);
        await context.SaveChangesAsync(cancellationToken);
        return Ok("Not güncellendi");
    }

    [HttpGet]
    public async Task<IActionResult> DeleteById(Guid Id, CancellationToken cancellationToken)
    {
        Todo? todo = await context
            .Todos
            .Where(p => p.Id == Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (todo is null)
        {
            throw new ArgumentException("Not bulunamadı");
        }

        todo.IsDeleted = true;

        context.Update(todo);
        await context.SaveChangesAsync(cancellationToken);
        return Ok("Not silindi");

    }
}
