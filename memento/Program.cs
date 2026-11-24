using memento.Resumes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(_ => Resume.CreateSample());
builder.Services.AddSingleton<ResumeHistory>();
builder.Services.AddSingleton<ResumeEditor>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
