using System;
using System.Collections.Generic;
using System.IO;

using Cake.Core;
using Cake.Core.IO;

using NSubstitute;

using Shouldly;

using Xunit;

using Path = Cake.Core.IO.Path;

namespace Cake.EnvXmlTransform.Tests {
	public class EnvXmlTransformRunnerTests {
		private readonly EnvXmlTransformRunner sut;
		private readonly List<Path> configFiles;
		private readonly string environment;
		private readonly string baseFilePath;
		private readonly string configFolder;

		public EnvXmlTransformRunnerTests() {
			var globber = Substitute.For<IGlobber>();
			this.configFiles = new List<Path>();
			globber.Match(Arg.Any<string>()).Returns(this.configFiles);
			var cakeContext = Substitute.For<ICakeContext>();
			cakeContext.Globber.Returns(globber);
			this.sut = new EnvXmlTransformRunner(cakeContext);
			var configPath = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}\..\..\";
			this.environment = "environment";
			this.baseFilePath = $"{configPath}file.config";
			WriteTextToFile(BaseConfigurationFile, this.baseFilePath);
			WriteTextToFile(EnvironmentSpecificConfigFile, $"{configPath}file.environment.config");
			this.configFolder = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}\..\..\**\*.config";
		}

		[Fact]
		public void ApplyTransformations_EnvironmentalFilePresentThatMatchesInput_TransformsEnvironmentalFileIntoBaseFile() {
			this.sut.ApplyTransformations(this.configFolder, this.environment);

			File.Exists(this.baseFilePath).ShouldBeTrue();
			var result = File.ReadAllText(this.baseFilePath);
			result.ShouldBe(ExpectedConfigurationFile);
		}

		[Fact]
		public void ApplyTransformations_EnvironmentalFilePresentThatMatchesInputButNotByCases_NoFileShouldBeTransformed() {
			this.sut.ApplyTransformations(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\..\..\**\*.config", "Environment");

			File.Exists(this.baseFilePath).ShouldBeTrue();
			var result = File.ReadAllText(this.baseFilePath);
			result.ShouldBe(BaseConfigurationFile);
		}

		private void WriteTextToFile(string content, string filename) {
			File.WriteAllText(filename, content);
			this.configFiles.Add(new FilePath(filename));
		}

		private const string EnvironmentSpecificConfigFile =
			@"<?xml version=""1.0""?>
				<configuration xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
					<appSettings>
						<add key=""anykeyreplaced"" value=""anyvaluereplaced"" xdt:Transform=""Replace"" />
					</appSettings>
				</configuration>";

		private const string BaseConfigurationFile =
			@"<?xml version=""1.0""?>
				<configuration>
					<appSettings>
						<add key=""anykey"" value=""anyvalue""/>
					</appSettings>
				</configuration>";

		private const string ExpectedConfigurationFile =
			@"<?xml version=""1.0""?>
				<configuration>
					<appSettings>
						<add key=""anykeyreplaced"" value=""anyvaluereplaced""/>
					</appSettings>
				</configuration>";
	}
}