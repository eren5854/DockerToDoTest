namespace DockerToDoTest.DTOs;

public sealed record UpdateTodoDto(
    Guid Id,
    string Note,
    bool Complated);
