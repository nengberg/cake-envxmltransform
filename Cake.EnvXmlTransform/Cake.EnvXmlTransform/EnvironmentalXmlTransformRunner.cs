using System;
using System.Collections.Generic;
using System.Linq;

using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.XdtTransform;

namespace Cake.EnvXmlTransform {
	public class EnvironmentalXmlTransformRunner {
		private readonly ICakeContext context;

		/// <summary>
		/// Returns a new instance of 
		/// </summary>
		/// <param name="context">the cake context</param>
		public EnvironmentalXmlTransformRunner(ICakeContext context) {
			this.context = context;
		}

		/// <summary>
		/// Applies config transformations on config files for a specific environment
		/// </summary>
		/// <param name="configFilesPath">Path where your *.config files are located. Patters are supported: "folder/**/*.config"</param>
		/// <param name="environment">What environmental configuration file to use for transformation, eg. stage for transforming files named *.stage.config into *.config</param>
		public void ApplyTransformations(string configFilesPath, string environment) {
			ValidateInput(configFilesPath, environment);

			var configFiles = this.context.GetFiles(configFilesPath);
			var environmentConfigs = configFiles.Where(MatchesGivenEnvironment(environment)).ToList();

			if(environmentConfigs.Any()) {
				var organizedConfigFiles = OrganizeConfigurationsByEnvironment(environment, environmentConfigs);
				TransformConfigurations(organizedConfigFiles);
			} else {
				Console.WriteLine($"No configuration files for environment: {environment} was found.");
			}
		}

		private static void ValidateInput(string configFilesPath, string environment) {
			if(string.IsNullOrWhiteSpace(configFilesPath)) {
				throw new ArgumentException(nameof(configFilesPath));
			}
			if(string.IsNullOrWhiteSpace(environment)) {
				throw new ArgumentException(nameof(environment));
			}
		}

		private static Func<FilePath, bool> MatchesGivenEnvironment(string environment)
			=> c => c.ToString().Contains(environment);

		private static Dictionary<string, string> OrganizeConfigurationsByEnvironment(string environment,
			IEnumerable<FilePath> environmentConfigs) {
			var grouped = new Dictionary<string, string>();
			foreach(var envConfig in environmentConfigs) {
				var orginalName = envConfig.ToString().Replace($"{environment}.", "");
				grouped[envConfig.ToString()] = orginalName;
			}
			return grouped;
		}

		private static void TransformConfigurations(Dictionary<string, string> organizedConfigFiles) {
			foreach(var config in organizedConfigFiles) {
				Console.WriteLine($"Transforming {config.Key} into {config.Value}");
				XdtTransformation.TransformConfig(config.Value, config.Key, config.Value);
			}
		}
	}
}