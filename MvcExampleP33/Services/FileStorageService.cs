namespace MvcExampleP33.Services;

public class FileStorageService(IWebHostEnvironment environment)
{
    private string GetFullPath(string fileName)
    {
        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "images");
        var dir1 = fileName[0].ToString();
        var dir2 = fileName[1].ToString();
        var fullDirPath = Path.Combine(uploadsFolder, dir1, dir2);
        return Path.Combine(fullDirPath, fileName);
    }


    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        var filePath = GetFullPath(uniqueFileName);

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        }

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        return uniqueFileName;
    }


    public void DeleteFile(string fileName)
    {
        var filePath = GetFullPath(fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
