using Microsoft.AspNetCore.Mvc;

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
    
    var minX = float.MaxValue;
    var maxX = float.MinValue;
    var minY = float.MaxValue;
    var maxY = float.MinValue;

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

app.MapPost("/bounding-box/expand", (BoundingBox boundingBox, [FromQuery]float percentage) =>
{
    if (percentage <= 0)
    {
        return Results.BadRequest("Percentage to expand must be greater than zero.");
    }
    
    var multiplier = percentage / 100f;
    var dxTop = Math.Abs(boundingBox.TopRight.X - boundingBox.TopLeft.X) * multiplier;
    var dxBottom = Math.Abs(boundingBox.BottomRight.X - boundingBox.BottomLeft.X) * multiplier;
    var dyRight = Math.Abs(boundingBox.TopRight.Y - boundingBox.BottomRight.Y) * multiplier;
    var dyLeft = Math.Abs(boundingBox.TopLeft.Y - boundingBox.BottomLeft.Y) * multiplier;

    return Results.Ok(new BoundingBox
    {
        TopRight = new Point
        {
            X = boundingBox.TopRight.X + dxTop,
            Y = boundingBox.TopRight.Y + dyRight
        },
        TopLeft = new Point
        {
            X = boundingBox.TopLeft.X - dxTop,
            Y = boundingBox.TopLeft.Y + dyLeft
        },
        BottomRight = new Point
        {
            X = boundingBox.BottomRight.X + dxBottom,
            Y = boundingBox.BottomRight.Y - dyRight
        },
        BottomLeft = new Point
        {
            X = boundingBox.BottomLeft.X - dxBottom,
            Y = boundingBox.BottomLeft.Y - dyLeft
        },
    });
});

app.Run();