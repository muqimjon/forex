namespace Forex.Application.Features.Files.Queries.GetPresignedUrl;

using FluentValidation;

public class GetPresignedUrlQueryValidator : AbstractValidator<GetPresignedUrlQuery>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx"];
    private static readonly string[] AllowedFolders = ["products", "users", "documents"];

    public GetPresignedUrlQueryValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(HaveAllowedExtension)
            .WithMessage($"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

        RuleFor(x => x.Folder)
            .NotEmpty()
            .Must(BeAllowedFolder)
            .WithMessage($"Folder not allowed. Allowed folders: {string.Join(", ", AllowedFolders)}");
    }

    private static bool HaveAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    private static bool BeAllowedFolder(string folder)
    {
        return AllowedFolders.Contains(folder.ToLowerInvariant());
    }
}
