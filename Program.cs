var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "BoundingBoxAPI";
    config.Title = "BoundingBoxAPI v1";
    config.Version = "v1";
});

var app = builder.Build();
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = "BoundingBoxAPI";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});

app.MapPost("/bounding-box", (Point[] points) =>
{
    if (points.Length < 3)
    {
        return Results.BadRequest("At least 3 points are required to create a bounding box.");
    }
    
    var minX = int.MaxValue;
    var maxX = int.MinValue;
    var minY = int.MaxValue;
    var maxY = int.MinValue;

    foreach (var pt in points)
    {
        if (pt.X > maxX)
            maxX = pt.X;
        if (pt.X < minX)
            minX = pt.X;
        if (pt.Y > maxY)
            maxY = pt.Y;
        if (pt.Y < minY)
            minY = pt.Y;
    }

    var box = new BoundingBox
    {
        TopLeft = new Point { X = minX, Y = maxY },
        TopRight = new Point { X = maxX, Y = maxY },
        BottomRight = new Point { X = maxX, Y = minY },
        BottomLeft = new Point { X = minX, Y = minY }
    };
    
    return Results.Ok(box);
});

app.Run();