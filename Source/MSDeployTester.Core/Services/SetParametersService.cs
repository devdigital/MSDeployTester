namespace MSDeployTester.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using MSDeployTester.Core.Models;

    public class SetParametersService
    {
        private readonly ProcessService processService;

        public SetParametersService()
        {
            this.processService = new ProcessService();            
        }

        public void TokenReplacement(
            string parametersFilePath,
            IEnumerable<SetParameter> parameterValues,
            string prePostFix)
        {
            if (string.IsNullOrWhiteSpace(parametersFilePath))
            {
                throw new ArgumentNullException(nameof(parametersFilePath));
            }

            if (parameterValues == null)
            {
                throw new ArgumentNullException(nameof(parameterValues));
            }

            if (string.IsNullOrWhiteSpace(prePostFix))
            {
                throw new ArgumentNullException(nameof(prePostFix));
            }

            if (!File.Exists(parametersFilePath))
            {
                throw new InvalidOperationException($"Could not locate SetParameters file '{parametersFilePath}");
            }

            var document = XDocument.Load(parametersFilePath);
            if (document.Root == null)
            {
                throw new InvalidOperationException($"Unexpected missing document root in file '{parametersFilePath}'");
            }

            var setParameterNodes = document.Root
                .Descendants("setParameter")
                .ToList();

            if (setParameterNodes == null || !setParameterNodes.Any())
            {
                throw new InvalidOperationException($"No <setParameter> nodes founds within '{parametersFilePath}'");
            }

            // TODO: reimplement - currently ignores errors
            foreach (var parameter in parameterValues)
            {
                if (parameter.IsKeyed)
                {
                    var setParameterNode =
                        setParameterNodes.FirstOrDefault(n => n.Attribute("name")?.Value == parameter.Key);

                    setParameterNode?.SetAttributeValue("value", parameter.Value);
                    continue;
                }

                foreach (var setParameterNode in setParameterNodes)
                {                   
                    var setParameterNodeValue = setParameterNode?.Attribute("value")?.Value;
                    if (string.IsNullOrWhiteSpace(setParameterNodeValue))
                    {
                        continue;
                    }
                    
                    var prePostFixedParameterKey = $"{prePostFix}{parameter.Key}{prePostFix}";
                    var newValue = setParameterNodeValue.Replace(prePostFixedParameterKey, parameter.Value);

                    setParameterNode.SetAttributeValue("value", newValue);
                }               
            }            

            document.Save(parametersFilePath);
        }

        [Obsolete("Use TokenReplacement instead")]
        public void ReplaceParametersByName(
            string parametersFilePath, 
            IDictionary<string, string> parameterValues)
        {
            if (string.IsNullOrWhiteSpace(parametersFilePath))
            {
                throw new ArgumentNullException(nameof(parametersFilePath));
            }

            if (parameterValues == null)
            {            
                throw new ArgumentNullException(nameof(parameterValues));
            }

            if (!File.Exists(parametersFilePath))
            {
                throw new InvalidOperationException($"Could not locate SetParameters file '{parametersFilePath}");
            }

            var document = XDocument.Load(parametersFilePath);            
            if (document.Root == null)
            {
                throw new InvalidOperationException($"Unexpected missing document root in file '{parametersFilePath}'");
            }

            var setParameterNodes = document.Root
                .Descendants("setParameter")                
                .ToList();

            if (setParameterNodes == null || !setParameterNodes.Any())
            {
                throw new InvalidOperationException($"No <setParameter> nodes founds within '{parametersFilePath}'");
            }

            foreach (var setParameterNode in setParameterNodes)
            {
                var setParameterNodeName = setParameterNode?.Attribute("name")?.Value;
                
                var parameterValue = parameterValues
                    .Where(p => p.Key == setParameterNodeName)
                    .Select(kvp => new { kvp.Key, kvp.Value })
                    .FirstOrDefault();

                if (parameterValue == null)
                {
                    throw new InvalidOperationException(
                        $"Attempting to update SetParmeter node '{setParameterNodeName}' but no corresponding value found");
                }

                setParameterNode?.SetAttributeValue("value", parameterValue.Value);
            }

            document.Save(parametersFilePath);
        }       

        public void Deploy(string deployScriptFilePath, string parameters)
        {
            if (string.IsNullOrWhiteSpace(nameof(deployScriptFilePath)))
            {
                throw new ArgumentNullException(nameof(deployScriptFilePath));
            }

            // Launch deployment
            var result = this.processService.Launch(
                deployScriptFilePath,
                parameters);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"There was an error running the MSDeploy deploy script '{deployScriptFilePath}'");
            }
        }

        //private static bool IsPrePostFixed(XElement node, string prePostFix)
        //{
        //    var nodeValue = node?.Attribute("value")?.Value;

        //    return !string.IsNullOrWhiteSpace(nodeValue)
        //        && Regex.IsMatch(nodeValue, $".*?{prePostFix}.*?{prePostFix}.*?");
        //}
    }
}
