namespace CodeShareBackend.Services
{
    public class HtmlTemplateService
    {
        public async Task<string> GetEmailTemplateAsync(string templatePath, Dictionary<string, string> placeholders)
        {
            string templateContent = await File.ReadAllTextAsync(templatePath);

            foreach (var placeholder in placeholders)
            {
                templateContent = templateContent.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }

            return templateContent;
        }
    }
}
