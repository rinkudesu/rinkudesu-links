using System;

namespace Rinkudesu.Services.Links.Utilities
{
    public static class EnvironmentalVariablesReader
    {
        public const string DbContextVariableName = "RINKU_LINKS_CONNECTIONSTRING";
        public static string GetRequiredVariable(string variableName)
        {
            var variableValue = Environment.GetEnvironmentVariable(variableName);
            if (variableValue is null)
            {
                throw new InvalidOperationException($"Environmental variable {variableName} is not set");
            }
            return variableValue;
        }
    }
}