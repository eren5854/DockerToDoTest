using Azure.Core;
using DockerToDoTest.Context;
using DockerToDoTest.DTOs;
using DockerToDoTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        return Ok(new { Message = "Not kaydedildi" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var todos = await context
            .Todos
            .Where(p => !p.IsDeleted)
            .OrderByDescending(o => o.CreatedDate)
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
        return Ok(new { Message = "Not güncellendi" });
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
        return Ok(new { Message = "Not silindi" });

    }

    [HttpPost]
    [Route("/api/google-webhook")] // Google için özel ve kısa bir route
    public async Task<IActionResult> GoogleAssistantWebhook([FromBody] dynamic request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Google'dan gelen Intent adını ve parametreyi alıyoruz
            string intentName = request.GetProperty("queryResult").GetProperty("intent").GetProperty("displayName").GetString();
            var queryResult = request.GetProperty("queryResult");

            // --- NOT EKLEME İŞLEMİ ---
            if (intentName == "CreateTodo")
            {
                // Dialogflow'da parametre ismini 'note' yaptıysan buradan okur
                string noteValue = queryResult.GetProperty("parameters").GetProperty("note").GetString();

                if (string.IsNullOrEmpty(noteValue))
                    return Ok(new { fulfillmentText = "Not içeriğini anlayamadım." });

                // Senin mevcut Create mantığını burada çalıştırıyoruz
                Todo todo = new()
                {
                    Note = noteValue,
                    Complated = false,
                    CreatedBy = "Google Assistant",
                    CreatedDate = DateTime.Now,
                };

                await context.AddAsync(todo, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return Ok(new { fulfillmentText = $"Tamamdır, '{noteValue}' notunu listenize ekledim." });
            }

            // --- LİSTELEME İŞLEMİ ---
            if (intentName == "GetTodos")
            {
                var todos = await context.Todos
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(o => o.CreatedDate)
                    .Take(5) // Asistan çok uzun listeleri okuyamaz, son 5 tanesini alalım
                    .ToListAsync(cancellationToken);

                if (!todos.Any())
                    return Ok(new { fulfillmentText = "Listenizde şu an hiç not bulunmuyor." });

                string listText = string.Join(", ", todos.Select(t => t.Note));
                return Ok(new { fulfillmentText = $"Listenizdeki son notlar şunlar: {listText}" });
            }
        }
        catch (Exception ex)
        {
            // Hata durumunda asistanın sessiz kalmaması için
            return Ok(new { fulfillmentText = "API tarafında bir hata oluştu, lütfen logları kontrol et." });
        }

        return Ok(new { fulfillmentText = "Bu komutu henüz desteklemiyorum." });
    }
}
