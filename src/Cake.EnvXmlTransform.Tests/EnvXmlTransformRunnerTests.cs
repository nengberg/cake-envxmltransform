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
	public class EnvXmlTransformRunnerTests : IDisposable {
		private readonly EnvironmentalXmlTransformRunner sut;
		private readonly List<Path> configFiles;
		private readonly string environment;
		private readonly string baseFilePath;
		private readonly string configFolder;
		private readonly string environmentFilePath;

		public EnvXmlTransformRunnerTests() {
			var globber = Substitute.For<IGlobber>();
			this.configFiles = new List<Path>();
			globber.Match(Arg.Any<string>()).Returns(this.configFiles);
			var cakeContext = Substitute.For<ICakeContext>();
			cakeContext.Globber.Returns(globber);
			this.sut = new EnvironmentalXmlTransformRunner(cakeContext);
			var configPath = $@"{AppContext.BaseDirectory}\..\..\";
			this.environment = "environment";
			this.baseFilePath = $"{configPath}file.config";
			this.environmentFilePath = $"{configPath}file.environment.config";
			WriteTextToFile(BaseConfigurationFile, this.baseFilePath);
			WriteTextToFile(EnvironmentSpecificConfigFile, this.environmentFilePath);
			this.configFolder = $@"{AppContext.BaseDirectory}\..\..\**\*.config";
		}

		[Theory, InlineData(""), InlineData(null)]
		public void ApplyTransformations_ConfigFilesPathNullOrEmpty_ThrowsArgumentException(string givenConfigFilesPath) {
			var exception = Record.Exception(() => this.sut.ApplyTransformations(givenConfigFilesPath, this.environment));

			exception.ShouldBeOfType<ArgumentException>();
		}

		[Theory, InlineData(""), InlineData(null)]
		public void ApplyTransformations_EnvironmentNullOrEmpty_ThrowsArgumentException(string givenEnvironment) {
			var exception = Record.Exception(() => this.sut.ApplyTransformations(this.configFolder, givenEnvironment));

			exception.ShouldBeOfType<ArgumentException>();
		}

		[Fact]
		public void ApplyTransformations_EnvironmentalFilePresentThatMatchesInput_TransformsEnvironmentalFileIntoBaseFile() {
			this.sut.ApplyTransformations(this.configFolder, this.environment);

			File.Exists(this.baseFilePath).ShouldBeTrue();

			var result = File.ReadAllText(this.baseFilePath);
			result.ShouldBe(ExpectedConfigurationFile);
		}

		[Theory, InlineData("Environment"), InlineData("anyenvironment")]
		public void ApplyTransformations_EnvironmentalFilePresentEnvironmentThatDoesNotMatchPresentOne_NoFileShouldBeTransformed(string givenEnvironment) {
			this.sut.ApplyTransformations(this.configFolder, givenEnvironment);

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

		public void Dispose() {
			File.Delete(this.baseFilePath);
			File.Delete(this.environmentFilePath);
		}
	}
}